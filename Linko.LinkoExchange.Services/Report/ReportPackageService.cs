using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.CopyOfRecord;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Mapping;
using System.Data.Entity;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Report.DataXML;
using FileInfo = Linko.LinkoExchange.Services.Report.DataXML.FileInfo;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Sample;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Xml.Linq;
using Linko.LinkoExchange.Core.Extensions;
using System.Runtime.CompilerServices;

namespace Linko.LinkoExchange.Services.Report
{
    public class ReportPackageService : BaseService, IReportPackageService
    {
        private readonly IProgramService _programService;
        private readonly ICopyOfRecordService _copyOfRecordService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ILogger _logger;
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly ISettingService _settingService;
        private readonly IOrganizationService _orgService;
        private readonly ISampleService _sampleService;
        private readonly IMapHelper _mapHelper;
        private readonly ICromerrAuditLogService _crommerAuditLogService;
        private readonly IOrganizationService _organizationService;
        private readonly IRequestCache _requestCache;

        public ReportPackageService(
           IProgramService programService,
           ICopyOfRecordService copyOfRecordService,
           ITimeZoneService timeZoneService,
           ILogger logger,
           LinkoExchangeContext linkoExchangeContext,
           IHttpContextService httpContextService,
           IUserService userService,
           IEmailService emailService,
           ISettingService settingService,
           IOrganizationService orgService,
           ISampleService sampleService,
           IMapHelper mapHelper,
           ICromerrAuditLogService crommerAuditLogService,
           IOrganizationService organizationService, 
           IRequestCache requestCache
           )
        {
            _programService = programService;
            _copyOfRecordService = copyOfRecordService;
            _timeZoneService = timeZoneService;
            _logger = logger;
            _dbContext = linkoExchangeContext;
            _httpContextService = httpContextService;
            _userService = userService;
            _emailService = emailService;
            _settingService = settingService;
            _orgService = orgService;
            _sampleService = sampleService;
            _mapHelper = mapHelper;
            _crommerAuditLogService = crommerAuditLogService;
            _organizationService = organizationService;
            _requestCache = requestCache;
        }

        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            bool retVal = false;

            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
            var currentPortalName = _httpContextService.GetClaimValue(CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value: currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            switch (apiName)
            {
                case "GetReportPackage":
                    {
                        var reportPackageId = id[0];
                        var reportPackage = _dbContext.ReportPackages
                            .SingleOrDefault(rp => rp.ReportPackageId == reportPackageId);

                        if (reportPackage == null)
                        {
                            return false;
                        }

                        //Check authorized access as either one of:
                        //1) Industry - currentOrgRegProgramId == reportPackage.OrganizationRegulatoryProgramId
                        //2) Authority - currentOrgRegProgramId == Id of authority of reportPackage.OrganizationRegulatoryProgram
                        if (currentPortalName.Equals("authority"))
                        {
                            if (currentOrgRegProgramId == _orgService.GetAuthority(reportPackage.OrganizationRegulatoryProgramId).OrganizationRegulatoryProgramId)
                            {
                                //Authority accessing an IU under their control => OK
                                retVal = true;
                            }
                        }
                        else
                        {
                            if (currentOrgRegProgramId == reportPackage.OrganizationRegulatoryProgramId)
                            {
                                //Industry accessing their own report package => OK
                                retVal = true;
                            }
                        }
                    }
                  
                    break;

                case "SignAndSubmitReportPackage":
                case "RepudiateReport":
                    {
                        var reportPackageId = id[0];

                        //this will also handle scenarios where ReportPackageId doesn't even exist (regardless of ownership)
                        var isReportPackageForThisOrgRegProgramExist = _dbContext.ReportPackages
                            .Any(rp => rp.ReportPackageId == reportPackageId
                                && rp.OrganizationRegulatoryProgramId == currentOrgRegProgramId);

                        if (isReportPackageForThisOrgRegProgramExist)
                        {
                            //Get current user
                            var orgRegProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                .Single(orpu => orpu.UserProfileId == currentUserId
                                    && orpu.OrganizationRegulatoryProgramId == currentOrgRegProgramId);

                            //Check if user has signatory rights
                            if (!orgRegProgramUser.IsSignatory)
                            {
                                return false;
                            }

                            //Check if user is removed or disabled
                            if (orgRegProgramUser.IsRemoved || !orgRegProgramUser.IsEnabled)
                            {
                                return false;
                            }

                            retVal = true;
                        }
                    }

                    break;

                default:

                    throw new Exception($"ERROR: Unhandled API authorization attempt using name = '{apiName}'");

            }

            return retVal;
        }

        /// <summary>
        /// Note:  before call this function,  make sure to update draft first.
        /// </summary>
        /// <param name="reportPackageId"></param>
        public void SignAndSubmitReportPackage(int reportPackageId)
        {
            var submitterUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
            var submitterFirstName = _httpContextService.GetClaimValue(CacheKey.FirstName);
            var submitterLastName = _httpContextService.GetClaimValue(CacheKey.LastName);
            var submitterUserName = _httpContextService.GetClaimValue(CacheKey.UserName);
            ReportPackageDto reportPackageDto;
            CopyOfRecordDto copyOfRecordDto; 

            _logger.Info($"Enter ReportPackageService.SignAndSubmitReportPackage. reportPackageId={reportPackageId}, submitterUserId={submitterUserId}, submitterUserName={submitterUserName}");

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    if (!CanUserExecuteApi(id: reportPackageId))
                    {
                        throw new UnauthorizedAccessException();
                    }

                    var reportPackage = _dbContext.ReportPackages.Include(i => i.ReportStatus)
                        .Single(i => i.ReportPackageId == reportPackageId);

                    if (reportPackage.ReportStatus.Name != ReportStatusName.ReadyToSubmit.ToString())
                    {
                        ThrowSimpleException("Report Package is not ready to submit.");
                    }

                    var submitterTitleRole = _userService.GetUserProfileById(submitterUserId).TitleRole;
                    var submitterIpAddress = _httpContextService.CurrentUserIPAddress();

                    reportPackage.SubmissionDateTimeUtc = DateTimeOffset.Now;
                    reportPackage.SubmitterUserId = submitterUserId;
                    reportPackage.SubmitterFirstName = submitterFirstName;
                    reportPackage.SubmitterLastName = submitterLastName;
                    reportPackage.SubmitterTitleRole = submitterTitleRole;
                    reportPackage.SubmitterIPAddress = submitterIpAddress;
                    reportPackage.SubmitterUserName = submitterUserName;

                    UpdateStatus(reportPackageId, ReportStatusName.Submitted, false);
                    _dbContext.SaveChanges();
                    reportPackageDto = GetReportPackage(reportPackageId, true);
                    copyOfRecordDto = CreateCopyOfRecordForReportPackage(reportPackageDto);

                    // Add for crommer log
                    WriteCrommerrLog(reportPackageDto, submitterIpAddress, copyOfRecordDto);

                    _dbContext.SaveChanges();
                    transaction.Commit();
                }
                catch (DbEntityValidationException ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        DbEntityEntry entry = item.Entry;
                        var entityTypeName = entry.Entity.GetType().Name;

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = $"Error '{subItem.ErrorMessage}' occurred in {entityTypeName} at {subItem.PropertyName}";
                            errors.Add(message);
                        }
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    var errors = new List<string>() { ex.Message };
                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(item: ex.Message);
                    }
                    _logger.Error(message: "Error happens {0} ", argument: string.Join(separator: "," + Environment.NewLine, values: errors));
                    throw;
                }
            }

           if(reportPackageDto != null && copyOfRecordDto != null)
            {
                //// Send emails 
                SendSignAndSubmitEmail(reportPackageDto, copyOfRecordDto);
            } 

          _logger.Info($"Leaving ReportPackageService.SignAndSubmitReportPackage. reportPackageId={reportPackageId}, submitterUserId={submitterUserId}, submitterUserName={submitterUserName}");
        }

        public CopyOfRecordPdfFileDto GetReportPackageCopyOfRecordPdfFile(int reportPackageId)
        {
            var reportPackageDto = this.GetReportPackage(reportPackageId, true, true);
            return GetReportPackageCopyOfRecordPdfFile(reportPackageDto);
        }

        public CopyOfRecordPdfFileDto GetReportPackageCopyOfRecordPdfFile(ReportPackageDto reportPackageDto)
        {
            var pdfGenerator = new PdfGenerator(reportPackageDto);
            var draftMode = reportPackageDto.ReportStatusName == ReportStatusName.Draft || reportPackageDto.ReportStatusName == ReportStatusName.ReadyToSubmit;
            var pdfData = pdfGenerator.CreateCopyOfRecordPdf(draftMode);

            return new CopyOfRecordPdfFileDto
            {
                FileData = pdfData,
                FileName = "Copy Of Record.pdf"
            };
        }

        public CopyOfRecordDataXmlFileDto GetReportPackageCopyOfRecordDataXmlFile(ReportPackageDto reportPackageDto)
        {
            _logger.Info("Enter ReportPackageService.CopyOfRecordDataXmlFileDto. reportPackageId={0}", reportPackageDto.ReportPackageId);

            var dateTimeFormat = "MM/dd/yyyyThh:mm tt zzzz";
            var timeZoneName = _timeZoneService.GetTimeZoneNameUsingSettingForThisOrg(reportPackageDto.OrganizationRegulatoryProgramId,
                                reportPackageDto.SubmissionDateTimeLocal.Value, false);

            var reportHeader = new ReportHeader
            {
                ReportName = reportPackageDto.Name,
                ReportPeriodStartDateTimeUtc = reportPackageDto.PeriodStartDateTimeLocal.ToString(dateTimeFormat),
                ReportPeriodEndDateTimeUtc = reportPackageDto.PeriodEndDateTimeLocal.ToString(dateTimeFormat),
                ReportSubmissionDateUtc = reportPackageDto.SubmissionDateTimeLocal.Value.ToString(dateTimeFormat),
                AuthorityTimeZone = timeZoneName,
            };

            var dataXmlObj = new CopyOfRecordDataXml
            {
                XmlFileVersion = new XmlFileVersion
                {
                    VersionNumber = "1.0.0"
                },

                ReportHeader = reportHeader,

                SubmittedTo = new SubmittedTo
                {
                    OrganizationName = reportPackageDto.RecipientOrganizationName,
                    Address1 = reportPackageDto.RecipientOrganizationAddressLine1.GetValueOrEmptyString(),
                    Address2 = reportPackageDto.RecipientOrganizationAddressLine2.GetValueOrEmptyString(),
                    City = reportPackageDto.RecipientOrganizationCityName.GetValueOrEmptyString(),
                    State = reportPackageDto.RecipientOrganizationJurisdictionName.GetValueOrEmptyString(),
                    ZipCode = reportPackageDto.RecipientOrganizationZipCode.GetValueOrEmptyString()
                },
                SubmittedOnBehalfOf = new SubmittedOnBehalfOf
                {
                    OrganizationName = reportPackageDto.OrganizationName,
                    ReferenceNumber = reportPackageDto.OrganizationReferenceNumber.GetValueOrEmptyString(),
                    Address1 = reportPackageDto.OrganizationAddressLine1.GetValueOrEmptyString(),
                    Address2 = reportPackageDto.OrganizationAddressLine2.GetValueOrEmptyString(),
                    City = reportPackageDto.OrganizationCityName.GetValueOrEmptyString(),
                    State = reportPackageDto.OrganizationJurisdictionName.GetValueOrEmptyString(),
                    ZipCode = reportPackageDto.OrganizationZipCode.GetValueOrEmptyString()
                },
                SubmittedBy = new SubmittedBy
                {
                    FirstName = reportPackageDto.SubmitterFirstName.GetValueOrEmptyString(),
                    LastName = reportPackageDto.SubmitterLastName.GetValueOrEmptyString(),
                    Title = reportPackageDto.SubmitterTitleRole.GetValueOrEmptyString(),
                    UserName = reportPackageDto.SubmitterUserName.GetValueOrEmptyString(),
                    ReportSubmissionFromIP = reportPackageDto.SubmitterIPAddress.GetValueOrEmptyString()
                },
                FileManifest = new FileManifest() //populated below
            };

            foreach (var attachmentReportPackageElementType in reportPackageDto.AttachmentTypes)
            {
                foreach (var fileStore in attachmentReportPackageElementType.FileStores)
                {
                    dataXmlObj.FileManifest.Files.Add(new FileInfo()
                    {
                        OriginalFileName = fileStore.OriginalFileName,
                        SystemGeneratedUniqueFileName = fileStore.Name,
                        AttachmentType = fileStore.ReportElementTypeName
                    });
                }
            }

            dataXmlObj.FileManifest.Files.Add(
                new FileInfo
                {
                    OriginalFileName = "Copy Of Record Data.xml",
                    SystemGeneratedUniqueFileName = "Copy Of Record Data.xml",
                    AttachmentType = "XML Raw Data"
                });

            dataXmlObj.FileManifest.Files.Add(
                new FileInfo
                {
                    OriginalFileName = "Copy Of Record.pdf",
                    SystemGeneratedUniqueFileName = "Copy Of Record.pdf",
                    AttachmentType = "Copy Of Record PDF"
                });

            dataXmlObj.Certifications = new List<Certification>();
            dataXmlObj.Certifications = reportPackageDto.CertificationTypes.Select(i => new Certification
            {
                CertificationType = i.ReportElementTypeName,
                CertificationText = i.ReportElementTypeContent.GetValueOrEmptyString()
            }).ToList();

            dataXmlObj.Comment = reportPackageDto.Comments;

            // SampleResults part 
            dataXmlObj.Samples = new List<SampleNode>();
            foreach (var sampleElementType in reportPackageDto.SamplesAndResultsTypes)
            {
                foreach (var sampleDto in sampleElementType.Samples)
                {
                    var sampleNode = new SampleNode
                    {
                        SampleName = sampleDto.Name,
                        MonitoringPointName = sampleDto.MonitoringPointName,
                        CtsEventTypeCategoryName = sampleDto.CtsEventCategoryName,
                        CtsEventTypeName = sampleDto.CtsEventTypeName,
                        CollectionMethodName = sampleDto.CollectionMethodName,
                        LabSampleIdentifier = sampleDto.LabSampleIdentifier.GetValueOrEmptyString(),

                        StartDateTimeUtc = sampleDto.StartDateTimeLocal.ToString(dateTimeFormat),
                        EndDateTimeUtc = sampleDto.EndDateTimeLocal.ToString(dateTimeFormat),

                        SampleFlowForMassCalcs = sampleDto.FlowValue.GetValueOrEmptyString(),
                        SampleFlowForMassCalcsUnitName = sampleDto.FlowUnitName.GetValueOrEmptyString(),

                        MassLoadingsConversionFactorPounds = sampleDto.MassLoadingConversionFactorPounds?.ToString(),
                        MassLoadingCalculationDecimalPlaces = sampleDto.MassLoadingCalculationDecimalPlaces?.ToString(),
                        IsMassLoadingResultToUseLessThanSign = sampleDto.IsMassLoadingResultToUseLessThanSign.ToString(),

                        SampledBy = sampleDto.ByOrganizationTypeName.GetValueOrEmptyString(),
                        SampleResults = new List<SampleResultNode>(),
                    };

                    dataXmlObj.Samples.Add(sampleNode);

                    foreach (var sampleResultDto in sampleDto.SampleResults)
                    {
                        var analysisDateTime = "";
                        if (sampleResultDto.AnalysisDateTimeLocal.HasValue)
                        {
                            analysisDateTime = sampleResultDto.AnalysisDateTimeLocal.Value.ToString(dateTimeFormat);
                        }

                        var sampleResultNode = new SampleResultNode
                        {
                            ParameterName = sampleResultDto.ParameterName,
                            Qualifier = System.Net.WebUtility.HtmlEncode(sampleResultDto.Qualifier).GetValueOrEmptyString(),
                            EnteredValue = sampleResultDto.EnteredValue.GetValueOrEmptyString(), 
                            Value = sampleResultDto.Value?.ToString(CultureInfo.InvariantCulture) ?? "",
                            UnitName = sampleResultDto.UnitName.GetValueOrEmptyString(),
                            EnteredMethodDetectionLimit = sampleResultDto.EnteredMethodDetectionLimit.GetValueOrEmptyString(),
                            MethodDetectionLimit = sampleResultDto.MethodDetectionLimit?.ToString() ?? "",
                            AnalysisMethod = sampleResultDto.AnalysisMethod.GetValueOrEmptyString(),
                            AnalysisDateTimeUtc = analysisDateTime,
                            IsApprovedEPAMethod = sampleResultDto.IsApprovedEPAMethod ? "Yes" : "No",
                            LimitBasis = LimitBasisName.Concentration.ToString()
                        };

                        sampleNode.SampleResults.Add(sampleResultNode);

                        if (!string.IsNullOrWhiteSpace(sampleResultDto.MassLoadingValue))
                        {
                            sampleResultNode = new SampleResultNode
                            {
                                ParameterName = sampleResultDto.ParameterName,
                                Qualifier = System.Net.WebUtility.HtmlEncode(sampleResultDto.Qualifier).GetValueOrEmptyString(),
                                Value = sampleResultDto.MassLoadingValue.GetValueOrEmptyString(),
                                UnitName = sampleResultDto.MassLoadingUnitName.GetValueOrEmptyString(),
                                LimitBasis = LimitBasisName.MassLoading.ToString()
                            };

                            sampleNode.SampleResults.Add(sampleResultNode);
                        }

                    }
                }
            }

            var xmlString = GenerateXmlString(dataXmlObj, reportPackageDto);
            var xmlData = new CopyOfRecordDataXmlFileDto
            {
                FileData = Encoding.UTF8.GetBytes(s: xmlString),
                FileName = "Copy Of Record Data.xml"
            };

            _logger.Info("Leave ReportPackageService.CopyOfRecordDataXmlFileDto. reportPackageId={0}", reportPackageDto.ReportPackageId);

            return xmlData;
        }

        public CopyOfRecordValidationResultDto VerififyCopyOfRecord(int reportPackageId)
        {
            _logger.Info("Enter ReportPackageService.VerififyCopyOfRecord. reportPackageId={0}", reportPackageId);

            var validationResult = _copyOfRecordService.ValidCopyOfRecordData(reportPackageId);

            _logger.Info("Enter ReportPackageService.VerififyCopyOfRecord. reportPackageId={0}", reportPackageId);

            return validationResult;
        }

        public ReportPackageDto GetReportPackage(int reportPackageId, bool isIncludeAssociatedElementData, bool isAuthorizationRequired = false)
        {
            _logger.Info("Enter ReportPackageService.GetReportPackage. reportPackageId={0}", reportPackageId);

            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));

            if (isAuthorizationRequired && !CanUserExecuteApi(id: reportPackageId))
            {
                throw new UnauthorizedAccessException();
            }

            var reportPackage = _dbContext.ReportPackages
                .Include(rp => rp.ReportPackageElementCategories)
                .Include(rp => rp.ReportPackageElementCategories.Select(rc => rc.ReportElementCategory))
                .Include(rp => rp.ReportPackageElementCategories.Select(rc => rc.ReportPackageElementTypes))
                .Include(rp => rp.ReportPackageElementCategories.Select(rc => rc.ReportPackageElementTypes.Select(rt => rt.ReportSamples)))
                .Include(rp => rp.ReportPackageElementCategories.Select(rc => rc.ReportPackageElementTypes.Select(rt => rt.ReportFiles)))
                .SingleOrDefault(rp => rp.ReportPackageId == reportPackageId);

            if (reportPackage == null)
            {
                throw new Exception($"ERROR: Could not find Report Package associated with reportPackageId={reportPackageId}");
            }

            var reportPackagegDto = GetReportPackageDtoFromReportPackage(reportPackage, timeZoneId);

            //
            //ADD SAMPLE ASSOCIATIONS (AND OPTIONALLY SAMPLE DATA)
            //

            //Find entry in tReportPackageElementType for this reportPackage associated with Samples category
            var samplesReportPackageElementCategory = reportPackage.ReportPackageElementCategories
                .SingleOrDefault(rpet => rpet.ReportElementCategory.Name == ReportElementCategoryName.SamplesAndResults.ToString());

            var sortedSamplesAndResultsTypes = new List<ReportPackageElementTypeDto>();
            if (samplesReportPackageElementCategory != null)
            {

                foreach (var existingSamplesReportPackageElementType in samplesReportPackageElementCategory.ReportPackageElementTypes)
                {
                    var reportPackageElementTypeDto = _mapHelper.GetReportPackageElementTypeDtoFromReportPackageElementType(existingSamplesReportPackageElementType);
                    reportPackageElementTypeDto.Samples = new List<SampleDto>();
                    //Should just be one iteration through this loop for the current phase, but in the future
                    //we might have more than one "Sample and Results" section in a Report Package

                    foreach (var reportSampleAssociated in existingSamplesReportPackageElementType.ReportSamples)
                    {
                        var sampleDto = new SampleDto()
                        {
                            SampleId = reportSampleAssociated.SampleId
                        };
                        if (isIncludeAssociatedElementData)
                        {
                            sampleDto = _sampleService.GetSampleDetails(reportSampleAssociated.SampleId);
                        }
                        reportPackageElementTypeDto.Samples.Add(sampleDto);

                    }

                    sortedSamplesAndResultsTypes.Add(reportPackageElementTypeDto);
                }
                sortedSamplesAndResultsTypes = sortedSamplesAndResultsTypes.OrderBy(item => item.SortOrder).ToList();
            }
            reportPackagegDto.SamplesAndResultsTypes = sortedSamplesAndResultsTypes;


            //
            //ADD FILE ASSOCIATIONS (AND OPTIONALLY FILE DATA)
            //

            //Find entry in tReportPackageElementType for this reportPackage associated with Samples category
            var filesReportPackageElementCategory = reportPackage.ReportPackageElementCategories
                .SingleOrDefault(rpet => rpet.ReportElementCategory.Name == ReportElementCategoryName.Attachments.ToString());

            var sortedAttachmentTypes = new List<ReportPackageElementTypeDto>();
            if (filesReportPackageElementCategory != null)
            {
                foreach (var existingFilesReportPackageElementType in filesReportPackageElementCategory.ReportPackageElementTypes)
                {
                    var reportPackageElementTypeDto = _mapHelper.GetReportPackageElementTypeDtoFromReportPackageElementType(existingFilesReportPackageElementType);
                    reportPackageElementTypeDto.FileStores = new List<FileStoreDto>();

                    foreach (var reportFileAssociated in existingFilesReportPackageElementType.ReportFiles)
                    {
                        var fileStoreDto = new FileStoreDto()
                        {
                            FileStoreId = reportFileAssociated.FileStoreId
                        };
                        if (isIncludeAssociatedElementData)
                        {
                            var fileStore = _dbContext.FileStores.Single(s => s.FileStoreId == reportFileAssociated.FileStoreId);
                            fileStoreDto = _mapHelper.GetFileStoreDtoFromFileStore(fileStore);
                            fileStoreDto.Data = fileStore.FileStoreData.Data;
                            fileStoreDto.UploadDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(fileStore.UploadDateTimeUtc.UtcDateTime, timeZoneId);
                        }
                        reportPackageElementTypeDto.FileStores.Add(fileStoreDto);

                    }
                    sortedAttachmentTypes.Add(reportPackageElementTypeDto);
                }
                sortedAttachmentTypes = sortedAttachmentTypes.OrderBy(item => item.SortOrder).ToList();
            }
            reportPackagegDto.AttachmentTypes = sortedAttachmentTypes;


            //
            //ADD CERTIFICATIONS
            //
            var certificationReportPackageElementCategory = reportPackage.ReportPackageElementCategories
                .SingleOrDefault(rpet => rpet.ReportElementCategory.Name == ReportElementCategoryName.Certifications.ToString());

            var sortedCertificationTypes = new List<ReportPackageElementTypeDto>();
            if (certificationReportPackageElementCategory != null)
            {
                foreach (var existingCertsReportPackageElementType in certificationReportPackageElementCategory.ReportPackageElementTypes)
                {
                    var reportPackageElementTypeDto = _mapHelper.GetReportPackageElementTypeDtoFromReportPackageElementType(existingCertsReportPackageElementType);
                    reportPackageElementTypeDto.FileStores = new List<FileStoreDto>();

                    foreach (var reportFileAssociated in existingCertsReportPackageElementType.ReportFiles)
                    {
                        var fileStoreDto = new FileStoreDto()
                        {
                            FileStoreId = reportFileAssociated.FileStoreId
                        };
                        if (isIncludeAssociatedElementData)
                        {
                            var fileStore = _dbContext.FileStores.Single(s => s.FileStoreId == reportFileAssociated.FileStoreId);
                            fileStoreDto = _mapHelper.GetFileStoreDtoFromFileStore(fileStore);
                            fileStoreDto.UploadDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(fileStore.UploadDateTimeUtc.UtcDateTime, timeZoneId);
                        }
                        reportPackageElementTypeDto.FileStores.Add(fileStoreDto);

                    }
                    sortedCertificationTypes.Add(reportPackageElementTypeDto);
                }
                sortedCertificationTypes = sortedCertificationTypes.OrderBy(item => item.SortOrder).ToList();
            }
            reportPackagegDto.CertificationTypes = sortedCertificationTypes;

            _logger.Info("Leave ReportPackageService.GetReportPackage. reportPackageId={0}", reportPackageId);
            return reportPackagegDto;
        }

        public CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId, ReportPackageDto reportPackageDto = null)
        {
            _logger.Info("Enter ReportPackageService.GetCopyOfRecordByReportPackageId. reportPackageId={0}", reportPackageId);

            if (reportPackageDto == null)
            {
                reportPackageDto = GetReportPackage(reportPackageId, false, true);
            }

            var copyOfRecordDto = _copyOfRecordService.GetCopyOfRecordByReportPackage(reportPackageDto);
            _logger.Info("Leave ReportPackageService.GetCopyOfRecordByReportPackageId. reportPackageId={0}", reportPackageId);

            return copyOfRecordDto;
        }

        private CopyOfRecordDto CreateCopyOfRecordForReportPackage(ReportPackageDto reportPackageDto)
        {
            _logger.Info("Enter ReportPackageService.CreateCopyOfRecordForReportPackage. reportPackageId={0}", reportPackageDto.ReportPackageId);
            int reportPackageId = reportPackageDto.ReportPackageId;

            var attachments = new List<FileStoreDto>();
            foreach (var attachmentReportPackageElementType in reportPackageDto.AttachmentTypes)
            {
                foreach (var fileStore in attachmentReportPackageElementType.FileStores)
                {
                    attachments.Add(fileStore);
                }
            }

            var copyOfRecordPdfFile = GetReportPackageCopyOfRecordPdfFile(reportPackageDto);
            var copyOfRecordDataXmlFile = GetReportPackageCopyOfRecordDataXmlFile(reportPackageDto);
            var copyOfRecordDto = _copyOfRecordService.CreateCopyOfRecordForReportPackage(reportPackageId, attachments, copyOfRecordPdfFile, copyOfRecordDataXmlFile);

            _logger.Info("Leave ReportPackageService.CreateCopyOfRecordForReportPackage. reportPackageId={0}", reportPackageId);

            return copyOfRecordDto;
        }

        private void WriteCrommerrLog(ReportPackageDto reportPackageDto, string submitterIpAddress, CopyOfRecordDto copyOfRecordDto)
        {
            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
            cromerrAuditLogEntryDto.RegulatoryProgramId = reportPackageDto.OrganizationRegulatoryProgramDto.RegulatoryProgramId;
            cromerrAuditLogEntryDto.OrganizationId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationId));
            var autorityOrg = _orgService.GetAuthority(reportPackageDto.OrganizationRegulatoryProgramId);
            cromerrAuditLogEntryDto.RegulatorOrganizationId = autorityOrg.OrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = reportPackageDto.SubmitterUserId;
            cromerrAuditLogEntryDto.UserName = reportPackageDto.SubmitterUserName;
            cromerrAuditLogEntryDto.UserFirstName = reportPackageDto.SubmitterFirstName;
            cromerrAuditLogEntryDto.UserLastName = reportPackageDto.SubmitterLastName;
            cromerrAuditLogEntryDto.UserEmailAddress = _httpContextService.GetClaimValue(CacheKey.Email);

            cromerrAuditLogEntryDto.IPAddress = submitterIpAddress;
            cromerrAuditLogEntryDto.HostName = _httpContextService.CurrentUserHostName();

            var crommerContentReplacements = new Dictionary<string, string>();
            crommerContentReplacements.Add("organizationName", reportPackageDto.OrganizationName);
            crommerContentReplacements.Add("reportPackageName", reportPackageDto.Name);

            var dateTimeFormat = "MM/dd/yyyy hh:mm tt";
            crommerContentReplacements.Add("periodStartDate", reportPackageDto.PeriodStartDateTimeLocal.ToString(dateTimeFormat));
            crommerContentReplacements.Add("periodEndDate", reportPackageDto.PeriodEndDateTimeLocal.ToString(dateTimeFormat));
            crommerContentReplacements.Add("corSignature", copyOfRecordDto.Signature);

            crommerContentReplacements.Add("firstName", reportPackageDto.SubmitterFirstName);
            crommerContentReplacements.Add("lastName", reportPackageDto.SubmitterLastName);
            crommerContentReplacements.Add("userName", reportPackageDto.SubmitterUserName);
            crommerContentReplacements.Add("emailAddress", cromerrAuditLogEntryDto.UserEmailAddress);

            _crommerAuditLogService.Log(CromerrEvent.Report_Submitted, cromerrAuditLogEntryDto, crommerContentReplacements);
        }

        private void SendSignAndSubmitEmail(ReportPackageDto reportPackage, CopyOfRecordDto copyOfRecordDto)
        {
            _logger.Info("Enter ReportPackageService.SendSignAndSubmitEmail. reportPackageId={0}", reportPackage.ReportPackageId);

            var emailContentReplacements = new Dictionary<string, string>();

            emailContentReplacements.Add("iuOrganizationName", reportPackage.OrganizationName);
            emailContentReplacements.Add("reportPackageName", reportPackage.Name);

            emailContentReplacements.Add("periodStartDate", reportPackage.PeriodStartDateTimeLocal.ToString("MMM dd, yyyy"));
            emailContentReplacements.Add("periodEndDate", reportPackage.PeriodEndDateTimeLocal.ToString("MMM dd, yyyy"));

            var timeZoneNameAbbreviation = _timeZoneService.GetTimeZoneNameUsingSettingForThisOrg(reportPackage.OrganizationRegulatoryProgramId, reportPackage.SubmissionDateTimeLocal.Value, true);
            var submissionDateTime =
                $"{reportPackage.SubmissionDateTimeLocal.Value.ToString("MMM dd, yyyy h:mmtt ")}{timeZoneNameAbbreviation}";

            emailContentReplacements.Add("submissionDateTime", submissionDateTime);
            emailContentReplacements.Add("corSignature", copyOfRecordDto.Signature);
            emailContentReplacements.Add("submitterFirstName", _httpContextService.GetClaimValue(CacheKey.FirstName));
            emailContentReplacements.Add("submitterLastName", _httpContextService.GetClaimValue(CacheKey.LastName));
            emailContentReplacements.Add("submitterTitle",  reportPackage.SubmitterTitleRole.GetValueOrEmptyString());

            emailContentReplacements.Add("permitNumber", reportPackage.OrganizationReferenceNumber);

            emailContentReplacements.Add("organizationAddressLine1", reportPackage.OrganizationAddressLine1);
            var organizationAddressLine2 = "";
            if (!string.IsNullOrWhiteSpace(reportPackage.OrganizationAddressLine2))
            {
                organizationAddressLine2 = $"{Environment.NewLine}\t{reportPackage.OrganizationAddressLine2}";
            }

            emailContentReplacements.Add("organizationAddressLine2", organizationAddressLine2);

            emailContentReplacements.Add("organizationCityName", reportPackage.OrganizationCityName);

            var juridicationName = "";
            if (!string.IsNullOrWhiteSpace(reportPackage.OrganizationJurisdictionName))
            {
                juridicationName = reportPackage.OrganizationJurisdictionName;
            }
            emailContentReplacements.Add("organizationJurisdictionName", juridicationName);

            var zipCode = "";
            if (!string.IsNullOrWhiteSpace(reportPackage.OrganizationZipCode))
            {
                zipCode = reportPackage.OrganizationZipCode;
            }
            emailContentReplacements.Add("organizationZipCode", zipCode);

            emailContentReplacements.Add("userName", _httpContextService.GetClaimValue(CacheKey.UserName));

            emailContentReplacements.Add("recipientOrganizationName", reportPackage.RecipientOrganizationName);
            emailContentReplacements.Add("recipientOrganizationAddressLine1", reportPackage.RecipientOrganizationAddressLine1);

            var recipientOrganizationAddressLine2 = "";
            if (!string.IsNullOrWhiteSpace(reportPackage.RecipientOrganizationAddressLine2))
            {
                recipientOrganizationAddressLine2 = $"{Environment.NewLine}\t{reportPackage.RecipientOrganizationAddressLine2}";
            }

            emailContentReplacements.Add("recipientOrganizationAddressLine2", recipientOrganizationAddressLine2);
            emailContentReplacements.Add("recipientOrganizationCityName", reportPackage.RecipientOrganizationCityName);
            emailContentReplacements.Add("recipientOrganizationJurisdictionName", reportPackage.RecipientOrganizationJurisdictionName);
            emailContentReplacements.Add("recipientOrganizationZipCode", reportPackage.RecipientOrganizationZipCode);

            var contactUserNameOnEmail = _settingService.GetOrgRegProgramSettingValue(reportPackage.OrganizationRegulatoryProgramDto.RegulatoryProgramId, SettingType.EmailContactInfoName);
            var emailAddressOnEmail = _settingService.GetOrgRegProgramSettingValue(reportPackage.OrganizationRegulatoryProgramDto.RegulatoryProgramId, SettingType.EmailContactInfoEmailAddress);
            var phoneNumberOnEmail = _settingService.GetOrgRegProgramSettingValue(reportPackage.OrganizationRegulatoryProgramDto.RegulatoryProgramId, SettingType.EmailContactInfoPhone);

            emailContentReplacements.Add("authorityName", contactUserNameOnEmail);
            emailContentReplacements.Add("supportEmail", emailAddressOnEmail);
            emailContentReplacements.Add("supportPhoneNumber", phoneNumberOnEmail);

            emailContentReplacements.Add("corViewLink", $"{_httpContextService.GetRequestBaseUrl()}reportPackage/{reportPackage.ReportPackageId}/Details");

            // Send emails to all IU signatories 
            var signatoriesEmails = _userService.GetOrgRegProgSignators(reportPackage.OrganizationRegulatoryProgramId).Select(i => i.Email).ToList();
            
            var iuOrganizationRegulatoryProgram = _organizationService.GetOrganizationRegulatoryProgram(reportPackage.OrganizationRegulatoryProgramId);

            _requestCache.SetValue(CacheKey.EmailRecipientRegulatoryProgramId, iuOrganizationRegulatoryProgram.RegulatoryProgramId);
            _requestCache.SetValue(CacheKey.EmailRecipientOrganizationId, iuOrganizationRegulatoryProgram.OrganizationId);
            _requestCache.SetValue(CacheKey.EmailRecipientRegulatoryOrganizationId, iuOrganizationRegulatoryProgram.RegulatorOrganizationId);
            
            _emailService.SendEmail(signatoriesEmails, EmailType.Report_Submission_IU, emailContentReplacements, false).Wait();

            // Send emails to all Standard Users for the authority  
            var authorityOrganzationId = reportPackage.OrganizationRegulatoryProgramDto.RegulatorOrganizationId.Value;
            var authorityAdminAndStandardUsersEmails = _userService.GetAuthorityAdministratorAndStandardUsers(authorityOrganzationId).Select(i => i.Email).ToList();
            
            var auOrganizationRegulatoryProgram = _organizationService.GetOrganizationRegulatoryProgram(reportPackage.OrganizationRegulatoryProgramId);

            _requestCache.SetValue(CacheKey.EmailRecipientRegulatoryProgramId, auOrganizationRegulatoryProgram.RegulatoryProgramId);
            _requestCache.SetValue(CacheKey.EmailRecipientOrganizationId, auOrganizationRegulatoryProgram.OrganizationId);
            _requestCache.SetValue(CacheKey.EmailRecipientRegulatoryOrganizationId, auOrganizationRegulatoryProgram.RegulatorOrganizationId);
            _emailService.SendEmail(authorityAdminAndStandardUsersEmails, EmailType.Report_Submission_AU, emailContentReplacements, false).Wait();

            _logger.Info("Leave ReportPackageService.SendSignAndSubmitEmail. reportPackageId={0}", reportPackage.ReportPackageId);
        }

        /// <summary>
        /// *WARNING: NO VALIDATION CHECK -- CASCADE DELETE*
        /// Hard delete of row from tReportPackage table associated with passed in parameter.
        /// Programmatically cascade deletes rows in the following associated tables:
        /// - tReportPackageElementCategory (via ReportPackageId)
        /// - tReportPackageElementType (via ReportPackageElementCategoryId)
        /// - tReportSample (via ReportPackageElementTypeId)
        /// - tReportFile (via ReportPackageElementTypeId)
        /// - tCopyofRecord (via ReportPackageId)
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        public void DeleteReportPackage(int reportPackageId)
        {
            _logger.Info($"Enter ReportPackageService.DeleteReportPackage. reportPackageId={reportPackageId}");

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var reportPackage = _dbContext.ReportPackages
                        .Include(rp => rp.ReportPackageElementCategories)
                        .Include(rp => rp.ReportPackageElementCategories.Select(rc => rc.ReportPackageElementTypes))
                        .Single(rp => rp.ReportPackageId == reportPackageId);

                    foreach (var rpec in reportPackage.ReportPackageElementCategories)
                    {
                        foreach (var rpet in rpec.ReportPackageElementTypes)
                        {
                            var reportSamples = _dbContext.ReportSamples
                                .Where(rs => rs.ReportPackageElementTypeId == rpet.ReportPackageElementTypeId);
                            if (reportSamples.Count() > 0)
                            {
                                _dbContext.ReportSamples.RemoveRange(reportSamples);
                            }

                            var reportFiles = _dbContext.ReportFiles
                                .Where(rf => rf.ReportPackageElementTypeId == rpet.ReportPackageElementTypeId);
                            if (reportFiles.Count() > 0)
                            {
                                _dbContext.ReportFiles.RemoveRange(reportFiles);
                            }

                        }
                        _dbContext.ReportPackageElementTypes.RemoveRange(rpec.ReportPackageElementTypes);

                    }
                    _dbContext.ReportPackageElementCategories.RemoveRange(reportPackage.ReportPackageElementCategories);

                    _dbContext.ReportPackages.Remove(reportPackage);

                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (DbEntityValidationException ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        DbEntityEntry entry = item.Entry;
                        string entityTypeName = entry.Entity.GetType().Name;

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                            errors.Add(message);
                        }
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;

                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(ex.Message);
                    }

                    _logger.Error("Error(s): {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;
                }

            }

            _logger.Info($"Leave ReportPackageService.DeleteReportPackage. reportPackageId={reportPackageId}");

        }

        /// <summary>
        /// To be called after a User selects a template and date range but before 
        /// the User clicks the "Save Draft" button (no reportPackageDto to save yet)
        /// </summary>
        /// <param name="reportPackageTemplateId"></param>
        /// <param name="startDateTimeLocal"></param>
        /// <param name="endDateTimeLocal"></param>
        /// <returns>The newly created tReportPackage.ReportPackageId</returns>
        public int CreateDraft(int reportPackageTemplateId, DateTime startDateTimeLocal, DateTime endDateTimeLocal)
        {
            _logger.Info($"Enter ReportPackageService.CreateDraft. reportPackageTemplateId={reportPackageTemplateId}, startDateTimeLocal={startDateTimeLocal}, endDateTimeLocal={endDateTimeLocal}");

            var newReportPackageId = -1;

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
                    var currentOrgRegProgram = _dbContext.OrganizationRegulatoryPrograms
                                            .Include(orp => orp.Organization)
                                            .Include(orp => orp.Organization.Jurisdiction)
                                            .Single(orp => orp.OrganizationRegulatoryProgramId == currentOrgRegProgramId);
                    var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
                    var timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));

                    //Step 1 - copy fields from template to new Report Package instance (tReportPackage)

                    //Get template
                    var reportPackageTemplate = _dbContext.ReportPackageTempates
                        .Include(rpt => rpt.CtsEventType)
                        .Include(rpt => rpt.OrganizationRegulatoryProgram)
                        .Include(rpt => rpt.OrganizationRegulatoryProgram.Organization)
                        .Include(rpt => rpt.OrganizationRegulatoryProgram.Organization.Jurisdiction)
                        .Include(rpt => rpt.ReportPackageTemplateAssignments)
                        .Single(rpt => rpt.ReportPackageTemplateId == reportPackageTemplateId);

                    //Check if current IU is assigned to (and has access to) template
                    var hasAccessViaAssignment = reportPackageTemplate.ReportPackageTemplateAssignments
                        .Any(rpta => rpta.OrganizationRegulatoryProgramId == currentOrgRegProgramId);
                    if (!hasAccessViaAssignment)
                    {
                        throw new Exception($"The current Org Reg Program Id (= {currentOrgRegProgram}) is not assigned to Report Template Id = {reportPackageTemplateId}");
                    }

                    var newReportPackage = _mapHelper.GetReportPackageFromReportPackageTemplate(reportPackageTemplate);

                    //Need to ensure these range dates are set to the beginning of the day (12am) for the start date 
                    //and end of the day (11:59:59pm) for the end date
                    //
                    startDateTimeLocal = startDateTimeLocal.Date; //12am
                    endDateTimeLocal = endDateTimeLocal.Date.AddDays(1).AddSeconds(-1); //11:59:59
                    var startDateTimeUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(startDateTimeLocal, timeZoneId);
                    var endDateTimeUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(endDateTimeLocal, timeZoneId);


                    newReportPackage.PeriodStartDateTimeUtc = startDateTimeUtc;
                    newReportPackage.PeriodEndDateTimeUtc = endDateTimeUtc;
                    newReportPackage.ReportStatusId = _dbContext.ReportStatuses
                        .Single(rs => rs.Name == ReportStatusName.Draft.ToString()).ReportStatusId;
                    newReportPackage.CreationDateTimeUtc = DateTimeOffset.Now;
                    newReportPackage.LastModificationDateTimeUtc = DateTimeOffset.Now;
                    newReportPackage.LastModifierUserId = currentUserId;

                    //Need to populate IU fields
                    newReportPackage.OrganizationRegulatoryProgramId = currentOrgRegProgramId;
                    newReportPackage.OrganizationReferenceNumber = currentOrgRegProgram.ReferenceNumber;
                    newReportPackage.OrganizationName = currentOrgRegProgram.Organization.Name;
                    newReportPackage.OrganizationAddressLine1 = currentOrgRegProgram.Organization.AddressLine1;
                    newReportPackage.OrganizationAddressLine2 = currentOrgRegProgram.Organization.AddressLine2;
                    newReportPackage.OrganizationCityName = currentOrgRegProgram.Organization.CityName;
                    newReportPackage.OrganizationJurisdictionName = currentOrgRegProgram.Organization.Jurisdiction?.Name;
                    newReportPackage.OrganizationZipCode = currentOrgRegProgram.Organization.ZipCode;

                    //Step 2 - create a row in tReportPackageElementCategory for each row in tReportPackageTemplateElementCategory (where ReportPackageTemplateId="n")

                    //Step 3 - create a row in tReportPackageElementType for each row in tReportPackageTemplateElementType(associated with the rows found in Step 1)

                    var reportPackageTemplateElementCategories = _dbContext.ReportPackageTemplateElementCategories
                        .Include(rptec => rptec.ReportPackageTemplateElementTypes)
                        .Where(rptec => rptec.ReportPackageTemplateId == reportPackageTemplateId)
                        .ToList();

                    foreach (var rptec in reportPackageTemplateElementCategories)
                    {
                        //Create a row in tReportPackageElementCategory
                        var newReportPackageElementCategory = new ReportPackageElementCategory()
                        {
                            ReportElementCategoryId = rptec.ReportElementCategoryId,
                            SortOrder = rptec.SortOrder,
                            ReportPackageElementTypes = new List<ReportPackageElementType>()
                        };
                        newReportPackage.ReportPackageElementCategories.Add(newReportPackageElementCategory); //handles setting ReportPackageId

                        foreach (var rptet in rptec.ReportPackageTemplateElementTypes)
                        {
                            var reportElementType = _dbContext.ReportElementTypes
                                .Include(ret => ret.CtsEventType)
                                .Single(ret => ret.ReportElementTypeId == rptet.ReportElementTypeId);

                            //Create a row in tReportPackageElementType
                            var newReportPackageElementType = new ReportPackageElementType();
                            newReportPackageElementType.ReportElementTypeId = rptet.ReportElementTypeId;
                            newReportPackageElementType.ReportElementTypeName = reportElementType.Name;
                            newReportPackageElementType.ReportElementTypeContent = reportElementType.Content;
                            newReportPackageElementType.ReportElementTypeIsContentProvided = reportElementType.IsContentProvided;

                            if (reportElementType.CtsEventType != null)
                            {
                                newReportPackageElementType.CtsEventTypeId = reportElementType.CtsEventType.CtsEventTypeId;
                                newReportPackageElementType.CtsEventTypeName = reportElementType.CtsEventType.Name;
                                newReportPackageElementType.CtsEventCategoryName = reportElementType.CtsEventType.CtsEventCategoryName;
                            }

                            newReportPackageElementType.IsRequired = rptet.IsRequired;
                            newReportPackageElementType.SortOrder = rptet.SortOrder;
                            newReportPackageElementType.ReportPackageElementCategory = newReportPackageElementCategory;
                            //handles setting ReportPackageElementCategoryId
                            newReportPackageElementCategory.ReportPackageElementTypes.Add(newReportPackageElementType);
                        }
                    }

                    _dbContext.ReportPackages.Add(newReportPackage);

                    _dbContext.SaveChanges(); //Need to do this to get new Id for samplesAndResultsReportPackageElementCategory

                    var samplesAndResultsReportPackageElementCategory = _dbContext.ReportPackageElementCategories
                        .Include(rpec => rpec.ReportElementCategory)
                        .Include(rpec => rpec.ReportPackageElementTypes)
                        .SingleOrDefault(rpec => rpec.ReportPackageId == newReportPackage.ReportPackageId
                            && rpec.ReportElementCategory.Name == ReportElementCategoryName.SamplesAndResults.ToString());

                    if (samplesAndResultsReportPackageElementCategory != null)
                    {
                        //Associate all existing and eligible samples to this draft
                        //
                        //From UC-16.4: "System pulls all samples in status "Ready to Report" with 
                        //Sample Start date or Sample End Date on or between report period into Draft"
                        //

                        var samplesAndResultsReportPackageElementType = samplesAndResultsReportPackageElementCategory.ReportPackageElementTypes.FirstOrDefault();
                        //Should be 1 and only 1 Report Package Element Type for Sample and Results 
                        //(but may be more than 1 in the future -- if so, which one is used to add samples??)
                        if (samplesAndResultsReportPackageElementType != null)
                        {
                            var existingEligibleSamples = _dbContext.Samples
                            .Where(s => s.ForOrganizationRegulatoryProgramId == currentOrgRegProgramId
                                && s.IsReadyToReport
                                && ((s.StartDateTimeUtc <= endDateTimeUtc && s.StartDateTimeUtc >= startDateTimeUtc) ||
                                    (s.EndDateTimeUtc <= endDateTimeUtc && s.EndDateTimeUtc >= startDateTimeUtc)));

                            foreach (var sample in existingEligibleSamples)
                            {
                                var reportSampleAssociation = new ReportSample();
                                reportSampleAssociation.ReportPackageElementTypeId = samplesAndResultsReportPackageElementType.ReportPackageElementTypeId;
                                reportSampleAssociation.SampleId = sample.SampleId;
                                _dbContext.ReportSamples.Add(reportSampleAssociation);
                            }

                            _dbContext.SaveChanges();
                        }
                        else
                        {
                            //Log -- there is no Sample & Results element type for this Report Package
                            //  therefore Samples & Results could not be automatically added.
                            _logger.Info(message: $"WARNING: Missing 'Samples & Results' element type for this Report Package Template (reportPackageTemplateId={reportPackageTemplateId}). Samples could not be added during 'CreateDraft'.");
                        }

                    }
                    else
                    {

                        //Log -- there is no Sample & Results category for this Report Package
                        //  therefore Samples & Results could not be automatically added.
                        _logger.Info(message: $"WARNING: Missing 'Samples & Results' element category for this Report Package Template (reportPackageTemplateId={reportPackageTemplateId}). Samples could not be added during 'CreateDraft'.");
                    }

                    transaction.Commit();

                    newReportPackageId = newReportPackage.ReportPackageId;
                }
                catch (DbEntityValidationException ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        DbEntityEntry entry = item.Entry;
                        string entityTypeName = entry.Entity.GetType().Name;

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                            errors.Add(message);
                        }
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;

                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(ex.Message);
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;
                }


            }

            _logger.Info($"Leave ReportPackageService.CreateDraft. newReportPackageId={newReportPackageId}, reportPackageTemplateId={reportPackageTemplateId}, startDateTimeLocal={startDateTimeLocal}, endDateTimeLocal={endDateTimeLocal}");

            return newReportPackageId;
        }

        /// <summary>
        /// Cannot be used to CREATE, only to UPDATE. Use "CreateDraft" to create.
        /// reportPackageDto.ReportPackageId must exist or exception thrown
        /// </summary>
        /// <param name="reportPackageDto">Existing Report Package to update</param>
        /// <param name="isUseTransaction">If true, runs within transaction object</param>
        /// <returns>Existing ReportPackage.ReportPackageId</returns>
        public int SaveReportPackage(ReportPackageDto reportPackageDto, bool isUseTransaction)
        {
            //Report Package will have already been saved before this call and therefore exists
            if (reportPackageDto.ReportPackageId < 1)
            {
                throw new Exception("ERROR: Cannot call 'SaveReportPackage' without setting reportPackageDto.ReportPackageId.");
            }

            _logger.Info($"Enter ReportPackageService.SaveReportPackage. reportPackageDto.ReportPackageId={reportPackageDto.ReportPackageId}");

            DbContextTransaction transaction = null;
            if (isUseTransaction)
            {
                transaction = _dbContext.BeginTransaction();
            }

            try
            {

                var reportPackage = _dbContext.ReportPackages
                    .Include(rp => rp.ReportPackageElementCategories)
                    .Include(rp => rp.ReportPackageElementCategories.Select(rc => rc.ReportElementCategory))
                    .Include(rp => rp.ReportPackageElementCategories.Select(rc => rc.ReportPackageElementTypes))
                    .Include(rp => rp.ReportPackageElementCategories.Select(rc => rc.ReportPackageElementTypes.Select(rt => rt.ReportSamples)))
                    .Include(rp => rp.ReportPackageElementCategories.Select(rc => rc.ReportPackageElementTypes.Select(rt => rt.ReportFiles)))
                    .Single(rp => rp.ReportPackageId == reportPackageDto.ReportPackageId);

                //Comments
                reportPackage.Comments = reportPackageDto.Comments;

                //
                //Add/remove report package elements 
                // 1. Samples
                // 2. Files
                //

                //SAMPLES
                //===================

                //Find entry in tReportPackageElementType for this reportPackage associated with Samples category
                var samplesReportPackageElementCategory = reportPackage.ReportPackageElementCategories
                    .SingleOrDefault(rpet => rpet.ReportElementCategory.Name == ReportElementCategoryName.SamplesAndResults.ToString());

                if (samplesReportPackageElementCategory != null)
                {
                    //Handle deletions first
                    // - Iterate through all SampleAndResult rows in ReportSample and delete ones that cannot be matched with an item in reportPackageDto.SamplesAndResultsTypes
                    foreach (var existingSamplesReportPackageElementType in samplesReportPackageElementCategory.ReportPackageElementTypes)
                    {
                        //Should just be one iteration through this loop for the current phase, but in the future
                        //we might have more than one "Sample and Results" section in a Report Package

                        var existingReportSamples = existingSamplesReportPackageElementType.ReportSamples.ToArray();
                        for (var i = 0; i < existingReportSamples.Length; i++)
                        {
                            var existingReportSample = existingReportSamples[i];
                            //Find match in dto samples
                            var matchedSampleAssociation = reportPackageDto.SamplesAndResultsTypes
                                                            .SingleOrDefault(rpet => rpet.ReportPackageElementTypeId == existingReportSample.ReportPackageElementTypeId)
                                                            ?.Samples
                                                                .SingleOrDefault(s => s.SampleId == existingReportSample.SampleId);

                            if (matchedSampleAssociation == null)
                            {
                                //existing association must have been deleted -- remove
                                _dbContext.ReportSamples.Remove(existingReportSample);
                            }
                        }
                    }

                    //Now handle additions
                    // - Iteration through all requested sample associations (in dto) and add ones that do not already exist
                    foreach (var requestedSampleAssociation in reportPackageDto.SamplesAndResultsTypes)
                    {
                        foreach (var sample in requestedSampleAssociation.Samples)
                        {
                            var foundReportSample = _dbContext.ReportSamples
                           .SingleOrDefault(rs => rs.ReportPackageElementTypeId == requestedSampleAssociation.ReportPackageElementTypeId
                               && rs.SampleId == sample.SampleId);

                            if (foundReportSample == null)
                            {
                                //Need to add association
                                _dbContext.ReportSamples.Add(new ReportSample()
                                {
                                    SampleId = sample.SampleId.Value,
                                    ReportPackageElementTypeId = requestedSampleAssociation.ReportPackageElementTypeId
                                });
                            }
                        }

                    }
                }
                else
                {
                    if (reportPackageDto.SamplesAndResultsTypes != null && reportPackageDto.SamplesAndResultsTypes.Count > 0)
                    {
                        _logger.Info(message: $"WARNING: Missing 'Samples & Results' element category for this Report Package (ReportPackageId={reportPackageDto.ReportPackageId}). Samples could not be added.");
                    }
                }


                //ATTACHMENTS
                //===================

                //Find entry in tReportPackageElementType for this reportPackage associated with Attachments category
                var attachmentsReportPackageElementCategory = reportPackage.ReportPackageElementCategories
                    .SingleOrDefault(rpet => rpet.ReportElementCategory.Name == ReportElementCategoryName.Attachments.ToString());

                if (attachmentsReportPackageElementCategory != null)
                {
                    //Handle deletions first
                    // - Iterate through all Attachment rows in ReportFile and delete ones that cannot be matched with an item in reportPackageDto.AttachmentTypes
                    foreach (var existingAttachmentsReportPackageElementType in attachmentsReportPackageElementCategory.ReportPackageElementTypes)
                    {
                        var existingReportFiles = existingAttachmentsReportPackageElementType.ReportFiles.ToArray();
                        for (var i = 0; i < existingReportFiles.Length; i++)
                        {
                            var existingReportFile = existingReportFiles[i];
                            //Find match in dto files
                            var matchedFileAssociation = reportPackageDto.AttachmentTypes
                                                        .SingleOrDefault(rpet => rpet.ReportPackageElementTypeId == existingReportFile.ReportPackageElementTypeId)
                                                        ?.FileStores
                                                            .SingleOrDefault(fs => fs.FileStoreId == existingReportFile.FileStoreId);

                            if (matchedFileAssociation == null)
                            {
                                //existing association must have been deleted -- remove
                                _dbContext.ReportFiles.Remove(existingReportFile);
                            }
                        }

                    }

                    //Now handle additions
                    // - Iteration through all requested attachment associations (in dto) and add ones that do not already exist
                    foreach (var requestedFileAssociation in reportPackageDto.AttachmentTypes)
                    {
                        foreach (var fileStore in requestedFileAssociation.FileStores)
                        {
                            var foundReportFile = _dbContext.ReportFiles
                                            .SingleOrDefault(rs => rs.ReportPackageElementTypeId == requestedFileAssociation.ReportPackageElementTypeId
                                                && rs.FileStoreId == fileStore.FileStoreId);

                            if (foundReportFile == null)
                            {
                                //Need to add association
                                _dbContext.ReportFiles.Add(new ReportFile()
                                {
                                    FileStoreId = fileStore.FileStoreId.Value,
                                    ReportPackageElementTypeId = requestedFileAssociation.ReportPackageElementTypeId
                                });
                            }
                        }

                    }
                }
                else
                {
                    if (reportPackageDto.AttachmentTypes != null && reportPackageDto.AttachmentTypes.Count > 0)
                    {
                        _logger.Info(message: $"WARNING: Missing 'Attachments' element category for this Report Package (ReportPackageId={reportPackageDto.ReportPackageId}). Attachments could not be added.");
                    }
                }

                //CERTIFICATIONS
                //===================

                //Find entry in tReportPackageElementType for this reportPackage associated with Attachments category
                var certsReportPackageElementCategory = reportPackage.ReportPackageElementCategories
                    .SingleOrDefault(rpet => rpet.ReportElementCategory.Name == ReportElementCategoryName.Certifications.ToString());

                if (certsReportPackageElementCategory != null)
                {
                    //Handle deletions first
                    // - Iterate through all Certification rows in ReportFile and delete ones that cannot be matched with an item in reportPackageDto.CertificationTypes
                    foreach (var existingCertReportPackageElementType in certsReportPackageElementCategory.ReportPackageElementTypes)
                    {
                        var existingReportFiles = existingCertReportPackageElementType.ReportFiles.ToArray();
                        for (var i = 0; i < existingReportFiles.Length; i++)
                        {
                            var existingReportFile = existingReportFiles[i];
                            //Find match in dto files
                            var matchedFileAssociation = reportPackageDto.CertificationTypes
                                                        .SingleOrDefault(rpet => rpet.ReportPackageElementTypeId == existingReportFile.ReportPackageElementTypeId)
                                                        ?.FileStores
                                                            .SingleOrDefault(fs => fs.FileStoreId == existingReportFile.FileStoreId);

                            if (matchedFileAssociation == null)
                            {
                                //existing association must have been deleted -- remove
                                _dbContext.ReportFiles.Remove(existingReportFile);
                            }
                        }

                    }

                    //Now handle additions
                    // - Iteration through all requested attachment associations (in dto) and add ones that do not already exist
                    foreach (var requestedFileAssociation in reportPackageDto.CertificationTypes)
                    {
                        foreach (var fileStore in requestedFileAssociation.FileStores)
                        {
                            var foundReportFile = _dbContext.ReportFiles
                                            .SingleOrDefault(rs => rs.ReportPackageElementTypeId == requestedFileAssociation.ReportPackageElementTypeId
                                                && rs.FileStoreId == fileStore.FileStoreId);

                            if (foundReportFile == null)
                            {
                                //Need to add association
                                _dbContext.ReportFiles.Add(new ReportFile()
                                {
                                    FileStoreId = fileStore.FileStoreId.Value,
                                    ReportPackageElementTypeId = requestedFileAssociation.ReportPackageElementTypeId
                                });
                            }
                        }

                    }
                }
                else
                {
                    if (reportPackageDto.CertificationTypes != null && reportPackageDto.CertificationTypes.Count > 0)
                    {
                        _logger.Info(message: $"WARNING: Missing 'Certifications' element category for this Report Package (ReportPackageId={reportPackageDto.ReportPackageId}). Certifications could not be added.");
                    }
                }



                _dbContext.SaveChanges();

                if (isUseTransaction)
                {
                    transaction.Commit();
                }

                _logger.Info($"Leave ReportPackageService.SaveReportPackage. reportPackageDto.ReportPackageId={reportPackageDto.ReportPackageId}");

                return reportPackage.ReportPackageId;

            }
            catch (DbEntityValidationException ex)
            {
                if (isUseTransaction)
                {
                    transaction.Rollback();
                }

                var errors = new List<string>() { ex.Message };

                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;

                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        errors.Add(message);
                    }
                }

                _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                throw;

            }
            catch
            {
                if (isUseTransaction)
                {
                    transaction.Rollback();
                }

                throw;
            }
            finally
            {
                if (isUseTransaction)
                {
                    transaction.Dispose();
                }

            }

        }

        /// <summary>
        /// Performs validation to ensure only allowed state transitions are occur,
        /// throw RuleViolationException otherwise. Does NOT enter any corrsponding values into the Report Package row.
        /// </summary>
        /// <param name="reportStatus">Intended target state</param>
        /// <param name="isUseTransaction">If true, runs within transaction object</param>
        public void UpdateStatus(int reportPackageId, ReportStatusName reportStatus, bool isUseTransaction)
        {
            _logger.Info($"Enter ReportPackageService.UpdateStatus. reportPackageId={reportPackageId}, reportStatus={reportStatus}");

            DbContextTransaction transaction = null;
            if (isUseTransaction)
            {
                transaction = _dbContext.BeginTransaction();
            }

            try
            {
                var reportPackage = _dbContext.ReportPackages
                      .Include(rp => rp.ReportStatus)
                      .Single(rp => rp.ReportPackageId == reportPackageId);

                var previousStatus = reportPackage.ReportStatus.Name;

                if (previousStatus == reportStatus.ToString())
                {
                    //No transition
                }
                else if (previousStatus == ReportStatusName.Draft.ToString() && reportStatus == ReportStatusName.ReadyToSubmit)
                {
                    //allowed
                }
                else if (previousStatus == ReportStatusName.ReadyToSubmit.ToString() && reportStatus == ReportStatusName.Draft)
                {
                    //allowed
                }
                else if (previousStatus == ReportStatusName.ReadyToSubmit.ToString() && reportStatus == ReportStatusName.Submitted)
                {
                    //allowed

                    //...but check to see if required Report Package Element Types are included.
                    if (!IsRequiredReportPackageElementTypesIncluded(reportPackageId))
                    {
                        ThrowSimpleException("Minimum counts for required element types must be met before a Report Package can be submitted.");
                    }
                }
                else if (previousStatus == ReportStatusName.Submitted.ToString() && reportStatus == ReportStatusName.Repudiated)
                {
                    //allowed
                }
                else
                {
                    //not allowed
                    ThrowSimpleException($"Cannot change a Report Package status from '{previousStatus}' to '{reportStatus}'.");
                }

                string targetReportStatusName = reportStatus.ToString();
                var targetReportStatusId = _dbContext.ReportStatuses
                    .Single(rs => rs.Name == targetReportStatusName).ReportStatusId;
                reportPackage.ReportStatusId = targetReportStatusId;

                _dbContext.SaveChanges();

                if (isUseTransaction)
                {
                    transaction.Commit();
                }
            }
            catch (DbEntityValidationException ex)
            {
                if (isUseTransaction)
                {
                    transaction.Rollback();
                }

                var errors = new List<string>() { ex.Message };

                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;

                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        errors.Add(message);
                    }
                }

                _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                throw;

            }
            catch (Exception ex)
            {
                if (isUseTransaction)
                {
                    transaction.Rollback();
                }

                var errors = new List<string>() { ex.Message };

                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    errors.Add(ex.Message);
                }

                _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                throw;
            }
            finally
            {
                if (isUseTransaction)
                {
                    transaction.Dispose();
                }

            }

            _logger.Info($"Leave ReportPackageService.UpdateStatus. reportPackageId={reportPackageId}, reportStatus={reportStatus}");
        }

        /// <summary>
        /// Gets a collection of FileStoreDto's that are eligible to be added this Report Package -- also indicate which are already associated.
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <returns>Collection of FileStoreDto objects</returns>
        public ICollection<FileStoreDto> GetFilesForSelection(int reportPackageElementTypeId)
        {
            var existingFilesReportPackageElementType = _dbContext.ReportPackageElementTypes
                                .Include(rp => rp.ReportPackageElementCategory.ReportPackage)
                                .Include(rp => rp.ReportPackageElementCategory.ReportElementCategory)
                                .Include(rp => rp.ReportFiles)
                                .Single(rp => rp.ReportPackageElementTypeId == reportPackageElementTypeId);

            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));

            var reportPackage = existingFilesReportPackageElementType.ReportPackageElementCategory.ReportPackage;

            _logger.Info($"Enter ReportPackageService.GetFilesForSelection. reportPackageElementTypeId={reportPackageElementTypeId}");

            var fileStoreList = new List<FileStoreDto>();
            var ageInMonthsSinceFileUploaded = Int32.Parse(_settingService.GetGlobalSettings()[SystemSettingType.FileAvailableToAttachMaxAgeMonths]);

            var xMonthsAgo = reportPackage.CreationDateTimeUtc.AddMonths(-ageInMonthsSinceFileUploaded);

            if (existingFilesReportPackageElementType.ReportPackageElementCategory.ReportElementCategory.Name != ReportElementCategoryName.Attachments.ToString())
            {
                //throw Exception
                throw new Exception($"ERROR: Passed in reportPackageElementTypeId={reportPackageElementTypeId} does not correspond with a Attachments.");
            }

            var filesOfThisReportElementType = _dbContext.FileStores
                .Where(fs => fs.OrganizationRegulatoryProgramId == reportPackage.OrganizationRegulatoryProgramId
                        && fs.ReportElementTypeId == existingFilesReportPackageElementType.ReportElementTypeId
                        && fs.UploadDateTimeUtc >= xMonthsAgo)
                .ToList();

            foreach (var eligibleFile in filesOfThisReportElementType)
            {
                var fileStoreDto = _mapHelper.GetFileStoreDtoFromFileStore(eligibleFile);
                fileStoreDto.ReportPackageElementTypeId = existingFilesReportPackageElementType.ReportPackageElementTypeId;
                fileStoreDto.IsAssociatedWithReportPackage = existingFilesReportPackageElementType
                                                            .ReportFiles.Any(rf => rf.FileStoreId == eligibleFile.FileStoreId);
                fileStoreDto.UploadDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(eligibleFile.UploadDateTimeUtc.UtcDateTime, timeZoneId);
                var uploaderUser = _dbContext.Users.Single(user => user.UserProfileId == fileStoreDto.UploaderUserId);
                fileStoreDto.UploaderUserFullName = $"{uploaderUser.FirstName} {uploaderUser.LastName}";

                var lastSubmittedReportPackage = _dbContext.FileStores
                    .Single(fs => fs.FileStoreId == eligibleFile.FileStoreId)
                        .ReportFiles
                        .Select(rf => rf.ReportPackageElementType.ReportPackageElementCategory.ReportPackage)
                        .Where(rp => rp.SubmissionDateTimeUtc.HasValue)
                        .OrderByDescending(rp => rp.SubmissionDateTimeUtc)
                        .FirstOrDefault();

                if (lastSubmittedReportPackage != null)
                {
                    fileStoreDto.LastSubmissionDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(lastSubmittedReportPackage.SubmissionDateTimeUtc.Value.UtcDateTime, timeZoneId);
                }

                fileStoreList.Add(fileStoreDto);
            }


            _logger.Info($"Leave ReportPackageService.GetFilesForSelection. reportPackageElementTypeId={reportPackageElementTypeId}, fileStoreList.Count={fileStoreList.Count}");

            return fileStoreList;
        }

        /// <summary>
        /// Gets a collection of SampleDto's that are eligible to be added this Report Package -- also indicate which are already associated.
        /// </summary>
        /// <param name="reportPackageElementTypeId">tReportPackage.reportPackageElementTypeId</param>
        /// <returns>Collection of SampleDto objects</returns>
        public ICollection<SampleDto> GetSamplesForSelection(int reportPackageElementTypeId)
        {
            _logger.Info($"Enter ReportPackageService.GetSamplesForSelection. reportPackageElementTypeId={reportPackageElementTypeId}");

            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));

            var existingSamplesReportPackageElementType = _dbContext.ReportPackageElementTypes
                                .Include(rp => rp.ReportPackageElementCategory.ReportPackage)
                                .Include(rp => rp.ReportPackageElementCategory.ReportElementCategory)
                                .Include(rp => rp.ReportSamples)
                                .Single(rp => rp.ReportPackageElementTypeId == reportPackageElementTypeId);

            if (existingSamplesReportPackageElementType.ReportPackageElementCategory.ReportElementCategory.Name != ReportElementCategoryName.SamplesAndResults.ToString())
            {
                throw new Exception($"ERROR: Passed in reportPackageElementTypeId={reportPackageElementTypeId} does not correspond with a Samples and Results.");
            }

            var reportPackage = existingSamplesReportPackageElementType.ReportPackageElementCategory.ReportPackage;

            var eligibleSampleList = new List<SampleDto>();

            var existingEligibleSamples = _dbContext.Samples
                        .Include(s => s.SampleResults)
                        .Where(s => s.ForOrganizationRegulatoryProgramId == reportPackage.OrganizationRegulatoryProgramId
                            && s.IsReadyToReport
                            && ((s.StartDateTimeUtc <= reportPackage.PeriodEndDateTimeUtc && s.StartDateTimeUtc >= reportPackage.PeriodStartDateTimeUtc) ||
                                (s.EndDateTimeUtc <= reportPackage.PeriodEndDateTimeUtc && s.EndDateTimeUtc >= reportPackage.PeriodStartDateTimeUtc)))
                         .ToList();


            foreach (var existingEligibleSample in existingEligibleSamples)
            {
                var sampleDto = _sampleService.GetSampleDetails(existingEligibleSample, isLoggingEnabled: false);

                sampleDto.IsAssociatedWithReportPackage = existingSamplesReportPackageElementType
                                                             .ReportSamples.Any(rs => rs.SampleId == existingEligibleSample.SampleId);

                var lastSubmittedReportPackage = _dbContext.ReportSamples
                    .Where(rs => rs.SampleId == existingEligibleSample.SampleId)
                    .Select(rs => rs.ReportPackageElementType.ReportPackageElementCategory.ReportPackage)
                        .Where(rp => rp.SubmissionDateTimeUtc.HasValue)
                        .OrderByDescending(rp => rp.SubmissionDateTimeUtc)
                        .FirstOrDefault();

                if (lastSubmittedReportPackage != null)
                {
                    sampleDto.LastSubmissionDateTimeLocal = _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(lastSubmittedReportPackage.SubmissionDateTimeUtc.Value.UtcDateTime, timeZoneId);
                }

                eligibleSampleList.Add(sampleDto);
            }

            _logger.Info($"Leave ReportPackageService.GetSamplesForSelection. reportPackageElementTypeId={reportPackageElementTypeId}, eligibleSampleList.Count={eligibleSampleList.Count}");

            return eligibleSampleList;
        }

        public ReportPackageElementTypeDto GetReportReportPackageElementType(int reportPackageElementTypeId)
        {
            var reportPackageElementType = _dbContext.ReportPackageElementTypes
                .Single(rpet => rpet.ReportPackageElementTypeId == reportPackageElementTypeId);

            var dto = _mapHelper.GetReportPackageElementTypeDtoFromReportPackageElementType(reportPackageElementType);

            return dto;
        }

        public List<ReportPackageStatusCount> GetReportPackageStatusCounts()
        {
            _logger.Info($"Enter ReportPackageService.GetReportPackageStatusCounts.");

            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var currentOrganizationId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationId));
            var currentRegulatoryProgramId = _dbContext.OrganizationRegulatoryPrograms
                .Single(orp => orp.OrganizationRegulatoryProgramId == currentOrgRegProgramId)
                .RegulatoryProgramId;

            var rptStatusCounts = new List<ReportPackageStatusCount>();

            var isAuthorityViewing = _httpContextService.GetClaimValue(CacheKey.PortalName).ToLower().Equals("authority");

            var reportPackages = _dbContext.ReportPackages
                .Include(rp => rp.ReportStatus)
                .Include(rp => rp.OrganizationRegulatoryProgram);

            if (isAuthorityViewing)
            {
                // For authority, return submitted pending review count and repudiation pending review count
                var submittedPendingReview = new ReportPackageStatusCount { Status = ReportStatusName.SubmittedPendingReview };
                var repudiationRendingReview = new ReportPackageStatusCount { Status = ReportStatusName.RepudiatedPendingReview };

                reportPackages = reportPackages
                     .Where(rp => rp.OrganizationRegulatoryProgram.RegulatorOrganizationId == currentOrganizationId
                     && rp.OrganizationRegulatoryProgram.RegulatoryProgramId == currentRegulatoryProgramId);

                submittedPendingReview.Count = reportPackages.Count(rp => rp.ReportStatus.Name == ReportStatusName.Submitted.ToString()
                        && rp.SubmissionReviewDateTimeUtc == null);

                repudiationRendingReview.Count = reportPackages
                    .Count(rp => rp.ReportStatus.Name == ReportStatusName.Repudiated.ToString()
                        && rp.RepudiationReviewDateTimeUtc == null);

                rptStatusCounts.Add(submittedPendingReview);
                rptStatusCounts.Add(repudiationRendingReview);
            }
            else
            {
                // For industry portal, return daft count and ready to submit count
                var draftCount = new ReportPackageStatusCount { Status = ReportStatusName.Draft };
                var readyToSubmitCount = new ReportPackageStatusCount { Status = ReportStatusName.ReadyToSubmit };

                reportPackages = reportPackages
                    .Where(rp => rp.OrganizationRegulatoryProgramId == currentOrgRegProgramId);

                draftCount.Count = reportPackages.Count(i => i.ReportStatus.Name == ReportStatusName.Draft.ToString());
                readyToSubmitCount.Count = reportPackages.Count(i => i.ReportStatus.Name == ReportStatusName.ReadyToSubmit.ToString());

                rptStatusCounts.Add(draftCount);
                rptStatusCounts.Add(readyToSubmitCount);
            }

            _logger.Info($"Enter ReportPackageService.GetReportPackageStatusCounts.");
            return rptStatusCounts;
        }

        /// <summary>
        /// Gets Report Package information (without children element data) for displaying in a grid.
        /// </summary>
        /// <param name="reportStatusName">Fetches report packages of this status only</param>
        /// <returns>Collection of ReportPackageDto objects (without children element data)</returns>
        public IEnumerable<ReportPackageDto> GetReportPackagesByStatusName(ReportStatusName reportStatusName)
        {
            _logger.Info($"Enter ReportPackageService.GetReportPackagesByStatusName. reportStatusName={reportStatusName}");
            var isAuthorityViewing = _httpContextService.GetClaimValue(CacheKey.PortalName).ToLower().Equals("authority");
            var reportPackageDtoList = new List<ReportPackageDto>();

            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var currentOrganizationId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationId));
            var currentRegulatoryProgramId = _dbContext.OrganizationRegulatoryPrograms
                .Single(orp => orp.OrganizationRegulatoryProgramId == currentOrgRegProgramId)
                .RegulatoryProgramId;
            var timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));


            var reportPackages = _dbContext.ReportPackages
                .Include(rp => rp.ReportStatus)
                .Include(rp => rp.OrganizationRegulatoryProgram);

            if (reportStatusName == ReportStatusName.SubmittedPendingReview)
            {
                reportPackages = reportPackages
                    .Where(rp => rp.ReportStatus.Name == ReportStatusName.Submitted.ToString()
                        && rp.SubmissionReviewDateTimeUtc == null);
            }
            else if (reportStatusName == ReportStatusName.RepudiatedPendingReview)
            {
                reportPackages = reportPackages
                    .Where(rp => rp.ReportStatus.Name == ReportStatusName.Repudiated.ToString()
                        && rp.RepudiationReviewDateTimeUtc == null);
            }
            else if (reportStatusName == ReportStatusName.All)
            {
                reportPackages = reportPackages
                    .Where(rp => rp.ReportStatus.Name == ReportStatusName.Submitted.ToString()
                        || rp.ReportStatus.Name == ReportStatusName.Repudiated.ToString());
            }
            else
            {
                reportPackages = reportPackages
                    .Where(rp => rp.ReportStatus.Name == reportStatusName.ToString());
            }


            if (isAuthorityViewing)
            {
                reportPackages = reportPackages
                    .Where(rp => rp.OrganizationRegulatoryProgram.RegulatorOrganizationId == currentOrganizationId
                    && rp.OrganizationRegulatoryProgram.RegulatoryProgramId == currentRegulatoryProgramId);
            }
            else
            {
                reportPackages = reportPackages
                    .Where(rp => rp.OrganizationRegulatoryProgramId == currentOrgRegProgramId);
            }

            foreach (var reportPackage in reportPackages.ToList())
            {
                var reportPackagegDto = GetReportPackageDtoFromReportPackage(reportPackage, timeZoneId);
                reportPackageDtoList.Add(reportPackagegDto);
            }

            _logger.Info($"Leaving ReportPackageService.GetReportPackagesByStatusName. reportStatusName={reportStatusName}");

            return reportPackageDtoList;

        }

        /// <summary>
        /// Gets items to populate a dropdown list of reasons to repudiate a report package (for a specific Org Reg Program Id)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RepudiationReasonDto> GetRepudiationReasons()
        {
            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var authorityOrganization = _orgService.GetAuthority(currentOrgRegProgramId);
            var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
            var timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));


            _logger.Info($"Enter ReportPackageService.GetRepudiationReasons. currentOrgRegProgramId={currentOrgRegProgramId}");

            var repudiationReasonDtos = new List<RepudiationReasonDto>();

            var repudiationReasons = _dbContext.RepudiationReasons
                .Where(rr => rr.OrganizationRegulatoryProgramId == authorityOrganization.OrganizationRegulatoryProgramId);

            foreach (var repudiationReason in repudiationReasons)
            {
                var repudiationReasonDto = _mapHelper.GetRepudiationReasonDtoFromRepudiationReason(repudiationReason);
                repudiationReasonDto.CreationLocalDateTime = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(repudiationReason.CreationDateTimeUtc.UtcDateTime, timeZoneId);

                if (repudiationReason.LastModificationDateTimeUtc.HasValue)
                {
                    repudiationReasonDto.LastModificationLocalDateTime = _timeZoneService
                        .GetLocalizedDateTimeUsingThisTimeZoneId(repudiationReason.LastModificationDateTimeUtc.Value.UtcDateTime, timeZoneId);
                }

                repudiationReasonDtos.Add(repudiationReasonDto);
            }

            _logger.Info($"Leaving ReportPackageService.GetRepudiationReasons. currentOrgRegProgramId={currentOrgRegProgramId}");

            return repudiationReasonDtos;
        }

        /// <summary>
        /// Performs various validation checks before putting a report package into "Repudiated" status.
        /// Also logs action and emails stakeholders.
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <param name="repudiationReasonId">tRepudiationReason.RepudiationReasonId</param>
        /// <param name="repudiationReasonName">Snapshot of tRepudiationReason.Name</param>
        /// <param name="comments">Optional field to accompany "other reason"</param>
        public void RepudiateReport(int reportPackageId, int repudiationReasonId, string repudiationReasonName, string comments = null)
        {
            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));

            _logger.Info($"Enter ReportPackageService.RepudiateReport. reportPackageId={reportPackageId}, repudiationReasonId={repudiationReasonId}, currentOrgRegProgramId={currentOrgRegProgramId}, currentUserId={currentUserId}");

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    if (!CanUserExecuteApi(id: reportPackageId))
                    {
                        throw new UnauthorizedAccessException();
                    }

                    var authorityOrganization = _orgService.GetAuthority(currentOrgRegProgramId);
                    var timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
                    var timeZone = _dbContext.TimeZones.Single(tz => tz.TimeZoneId == timeZoneId);

                    //Check ARP config "Max days after report period end date to repudiate" has not passed (UC-19 5.2.)
                    var reportRepudiatedDays = Convert.ToInt32(_settingService.GetOrgRegProgramSettingValue(authorityOrganization.OrganizationRegulatoryProgramId, SettingType.ReportRepudiatedDays));

                    var reportPackage = _dbContext.ReportPackages
                        .Include(rep => rep.OrganizationRegulatoryProgram)
                        .Single(rep => rep.ReportPackageId == reportPackageId);

                    if (reportPackage.PeriodEndDateTimeUtc < DateTime.UtcNow.AddDays(-reportRepudiatedDays))
                    {
                        ThrowSimpleException($"Report repudiation time period of {reportRepudiatedDays} days has expired.");
                    }

                    //Check valid reason (UC-19 7.a.)
                    if (repudiationReasonId < 1)
                    {
                        ThrowSimpleException($"Reason is required.", "RepudiationReasonId");
                    }

                    //=========
                    //Repudiate
                    //=========
                    var currentUser = _dbContext.Users
                        .Single(user => user.UserProfileId == currentUserId);

                    reportPackage.RepudiationDateTimeUtc = DateTimeOffset.Now;
                    reportPackage.RepudiatorUserId = currentUserId;
                    reportPackage.RepudiatorFirstName = currentUser.FirstName;
                    reportPackage.RepudiatorLastName = currentUser.LastName;
                    reportPackage.RepudiatorTitleRole = currentUser.TitleRole;
                    reportPackage.RepudiationReasonId = repudiationReasonId;
                    reportPackage.RepudiationReasonName = repudiationReasonName;
                    if (!String.IsNullOrEmpty(comments))
                    {
                        reportPackage.RepudiationComments = comments;
                    }

                    reportPackage.LastModificationDateTimeUtc = reportPackage.RepudiationDateTimeUtc;
                    reportPackage.LastModifierUserId = currentUserId;

                    //Change status
                    this.UpdateStatus(reportPackageId, ReportStatusName.Repudiated, false);

                    //===========
                    //Send emails
                    //===========

                    //System sends Repudiation Receipt email to all Signatories for Industry (UC-19 8.3.)

                    var corHash = _dbContext.CopyOfRecords.SingleOrDefault(cor => cor.ReportPackageId == reportPackageId);

                    if (corHash == null)
                    {
                        ThrowSimpleException($"ERROR: Could not find COR associated with ReportPackageId={reportPackageId}");
                    }

                    //Use the same contentReplacement dictionary for both emails and Cromerr audit logging
                    var contentReplacements = new Dictionary<string, string>();

                    //Report Details:
                    contentReplacements.Add("reportPackageName", reportPackage.Name);
                    contentReplacements.Add("periodStartDate", _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.PeriodStartDateTimeUtc.UtcDateTime, timeZoneId).ToString("MMM d, yyyy"));
                    contentReplacements.Add("periodEndDate", _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.PeriodEndDateTimeUtc.UtcDateTime, timeZoneId).ToString("MMM d, yyyy"));
                    contentReplacements.Add("submissionDateTime", _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.SubmissionDateTimeUtc.Value.UtcDateTime, timeZoneId).ToString("MMM d, yyyy h:mmtt") +
                        $" {_timeZoneService.GetTimeZoneNameUsingThisTimeZone(timeZone, reportPackage.SubmissionDateTimeUtc.Value.UtcDateTime, true)}");
                    contentReplacements.Add("corSignature", corHash.Signature);
                    contentReplacements.Add("repudiatedDateTime", _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.RepudiationDateTimeUtc.Value.UtcDateTime, timeZoneId).ToString("MMM d, yyyy h:mmtt") +
                        $" {_timeZoneService.GetTimeZoneNameUsingThisTimeZone(timeZone, reportPackage.RepudiationDateTimeUtc.Value.UtcDateTime, true)}");
                    contentReplacements.Add("repudiationReason", repudiationReasonName);
                    contentReplacements.Add("repudiationReasonComments", comments ?? "");

                    //Repudiated to:
                    contentReplacements.Add("authOrganizationName", reportPackage.RecipientOrganizationName);
                    contentReplacements.Add("authOrganizationAddressLine1", reportPackage.RecipientOrganizationAddressLine1);

                    string authOrganizationAddressLine2 = "";
                    if (!string.IsNullOrWhiteSpace(reportPackage.RecipientOrganizationAddressLine2))
                    {
                        authOrganizationAddressLine2 = $"{Environment.NewLine}\t{reportPackage.RecipientOrganizationAddressLine2}";
                    }
                    contentReplacements.Add("authOrganizationAddressLine2", authOrganizationAddressLine2);

                    contentReplacements.Add("authOrganizationCityName", reportPackage.RecipientOrganizationCityName);
                    contentReplacements.Add("authOrganizationJurisdictionName", reportPackage.RecipientOrganizationJurisdictionName);
                    contentReplacements.Add("authOrganizationZipCode", reportPackage.RecipientOrganizationZipCode);

                    //Repudiated by:
                    contentReplacements.Add("submitterFirstName", currentUser.FirstName);
                    contentReplacements.Add("submitterLastName", currentUser.LastName);
                    contentReplacements.Add("submitterTitle", currentUser.TitleRole);
                    contentReplacements.Add("iuOrganizationName", reportPackage.OrganizationName);
                    contentReplacements.Add("permitNumber", reportPackage.OrganizationRegulatoryProgram.ReferenceNumber);
                    contentReplacements.Add("organizationAddressLine1", reportPackage.OrganizationAddressLine1);

                    string organizationAddressLine2 = "";
                    if (!string.IsNullOrWhiteSpace(reportPackage.OrganizationAddressLine2))
                    {
                        organizationAddressLine2 = $"{Environment.NewLine}\t{reportPackage.OrganizationAddressLine2}";
                    }
                    contentReplacements.Add("organizationAddressLine2", organizationAddressLine2);

                    contentReplacements.Add("organizationCityName", reportPackage.OrganizationCityName);
                    contentReplacements.Add("organizationJurisdictionName", reportPackage.OrganizationJurisdictionName);
                    contentReplacements.Add("organizationZipCode", reportPackage.OrganizationZipCode);

                    contentReplacements.Add("userName", currentUser.UserName);
                    contentReplacements.Add("corViewLink", $"{_httpContextService.GetRequestBaseUrl()}reportPackage/{reportPackage.ReportPackageId}/Details");

                    var authorityName = _settingService.GetOrgRegProgramSettingValue(authorityOrganization.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName);
                    var authorityEmail = _settingService.GetOrgRegProgramSettingValue(authorityOrganization.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress);
                    var authorityPhone = _settingService.GetOrgRegProgramSettingValue(authorityOrganization.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone);

                    contentReplacements.Add("authorityName", authorityName);
                    contentReplacements.Add("supportEmail", authorityEmail);
                    contentReplacements.Add("supportPhoneNumber", authorityPhone);

                    //For Cromerr
                    contentReplacements.Add("organizationName", reportPackage.OrganizationName);
                    contentReplacements.Add("firstName", currentUser.FirstName);
                    contentReplacements.Add("lastName", currentUser.LastName);
                    contentReplacements.Add("emailAddress", currentUser.Email);


                    //Cromerr Log (UC-19 8.6.)
                    var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
                    cromerrAuditLogEntryDto.RegulatoryProgramId = reportPackage.OrganizationRegulatoryProgram.RegulatoryProgramId;
                    cromerrAuditLogEntryDto.OrganizationId = reportPackage.OrganizationRegulatoryProgram.OrganizationId;
                    cromerrAuditLogEntryDto.RegulatorOrganizationId = reportPackage.OrganizationRegulatoryProgram.RegulatorOrganizationId ??
                                                                      cromerrAuditLogEntryDto.OrganizationId;
                    cromerrAuditLogEntryDto.UserProfileId = currentUser.UserProfileId;
                    cromerrAuditLogEntryDto.UserName = currentUser.UserName;
                    cromerrAuditLogEntryDto.UserFirstName = currentUser.FirstName;
                    cromerrAuditLogEntryDto.UserLastName = currentUser.LastName;
                    cromerrAuditLogEntryDto.UserEmailAddress = currentUser.Email;
                    cromerrAuditLogEntryDto.IPAddress = _httpContextService.CurrentUserIPAddress();
                    cromerrAuditLogEntryDto.HostName = _httpContextService.CurrentUserHostName();

                    _crommerAuditLogService.Log(CromerrEvent.Report_Repudiated, cromerrAuditLogEntryDto, contentReplacements);

                    _dbContext.SaveChanges();
                    
                    var signatoriesEmails = _userService.GetOrgRegProgSignators(reportPackage.OrganizationRegulatoryProgramId).Select(i => i.Email).ToList();
            
                    var iuOrganizationRegulatoryProgram = _organizationService.GetOrganizationRegulatoryProgram(reportPackage.OrganizationRegulatoryProgramId);

                    _requestCache.SetValue(CacheKey.EmailRecipientRegulatoryProgramId, iuOrganizationRegulatoryProgram.RegulatoryProgramId);
                    _requestCache.SetValue(CacheKey.EmailRecipientOrganizationId, iuOrganizationRegulatoryProgram.OrganizationId);
                    _requestCache.SetValue(CacheKey.EmailRecipientRegulatoryOrganizationId, iuOrganizationRegulatoryProgram.RegulatorOrganizationId);
            
                    _emailService.SendEmail(signatoriesEmails, EmailType.Report_Repudiation_IU, contentReplacements, false).Wait();

                    //System sends Report Repudiated Receipt to all Admin and Standard Users for the Authority (UC-19 8.4.)
                    var authorityOrganzationId = authorityOrganization.OrganizationId;
                    var authorityAdminAndStandardUsersEmails = _userService.GetAuthorityAdministratorAndStandardUsers(authorityOrganzationId).Select(i => i.Email).ToList();
            
                    var auOrganizationRegulatoryProgram = _organizationService.GetOrganizationRegulatoryProgram(reportPackage.OrganizationRegulatoryProgramId);

                    _requestCache.SetValue(CacheKey.EmailRecipientRegulatoryProgramId, auOrganizationRegulatoryProgram.RegulatoryProgramId);
                    _requestCache.SetValue(CacheKey.EmailRecipientOrganizationId, auOrganizationRegulatoryProgram.OrganizationId);
                    _requestCache.SetValue(CacheKey.EmailRecipientRegulatoryOrganizationId, auOrganizationRegulatoryProgram.RegulatorOrganizationId);

                    _emailService.SendEmail(authorityAdminAndStandardUsersEmails, EmailType.Report_Repudiation_AU, contentReplacements, false).Wait();

                    transaction.Commit();
                }
                catch (DbEntityValidationException ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        DbEntityEntry entry = item.Entry;
                        string entityTypeName = entry.Entity.GetType().Name;

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = $"Error '{subItem.ErrorMessage}' occurred in {entityTypeName} at {subItem.PropertyName}";
                            errors.Add(message);
                        }
                    }

                    _logger.Error("Error happens {0} ", string.Join("," + Environment.NewLine, errors));

                    throw;

                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }

            }


            _logger.Info($"Leave ReportPackageService.RepudiateReport. reportPackageId={reportPackageId}, repudiationReasonId={repudiationReasonId}, currentOrgRegProgramId={currentOrgRegProgramId}, currentUserId={currentUserId}");

        }

        /// <summary>
        /// Meant to be called when user has reviewed a report submission. Updates the corresponding fields in the Report Package row.
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <param name="comments">Optional field</param>
        public void ReviewSubmission(int reportPackageId, string comments = null)
        {
            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
            _logger.Info($"Enter ReportPackageService.ReviewSubmission. reportPackageId={reportPackageId}, comments={comments}, currentOrgRegProgramId={currentOrgRegProgramId}, currentUserId={currentUserId}");

            //Comments are optional here (UC-10 6.)

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var user = _dbContext.Users
                        .Single(u => u.UserProfileId == currentUserId);

                    var reportPackage = _dbContext.ReportPackages
                        .Single(rep => rep.ReportPackageId == reportPackageId);

                    reportPackage.SubmissionReviewDateTimeUtc = DateTimeOffset.Now;
                    reportPackage.SubmissionReviewerUserId = currentUserId;
                    reportPackage.SubmissionReviewerFirstName = user.FirstName;
                    reportPackage.SubmissionReviewerLastName = user.LastName;
                    reportPackage.SubmissionReviewerTitleRole = user.TitleRole;

                    if (!String.IsNullOrEmpty(comments))
                        reportPackage.SubmissionReviewComments = comments;

                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (DbEntityValidationException ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        DbEntityEntry entry = item.Entry;
                        string entityTypeName = entry.Entity.GetType().Name;

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = $"Error '{subItem.ErrorMessage}' occurred in {entityTypeName} at {subItem.PropertyName}";
                            errors.Add(message);
                        }
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;

                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }
            }

            _logger.Info($"Leave ReportPackageService.ReviewSubmission. reportPackageId={reportPackageId}, comments={comments}, currentOrgRegProgramId={currentOrgRegProgramId}, currentUserId={currentUserId}");

        }

        /// <summary>
        /// Meant to be called when user has reviewed a report repudiation. Updates the corresponding fields in the Report Package row.
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <param name="comments">Required field</param>
        public void ReviewRepudiation(int reportPackageId, string comments)
        {
            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
            _logger.Info($"Enter ReportPackageService.ReviewRepudiation. reportPackageId={reportPackageId}, comments={comments}, currentOrgRegProgramId={currentOrgRegProgramId}, currentUserId={currentUserId}");

            if (String.IsNullOrEmpty(comments))
            {
                //UC-56 (6.a.) "Required"
                ThrowSimpleException($"Comments are required.");
            }

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var user = _dbContext.Users
                        .Single(u => u.UserProfileId == currentUserId);

                    var reportPackage = _dbContext.ReportPackages
                        .Single(rep => rep.ReportPackageId == reportPackageId);

                    reportPackage.RepudiationReviewDateTimeUtc = DateTimeOffset.Now;
                    reportPackage.RepudiationReviewerUserId = currentUserId;
                    reportPackage.RepudiationReviewerFirstName = user.FirstName;
                    reportPackage.RepudiationReviewerLastName = user.LastName;
                    reportPackage.RepudiationReviewerTitleRole = user.TitleRole;
                    reportPackage.RepudiationReviewComments = comments;

                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch (DbEntityValidationException ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        DbEntityEntry entry = item.Entry;
                        string entityTypeName = entry.Entity.GetType().Name;

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = $"Error '{subItem.ErrorMessage}' occurred in {entityTypeName} at {subItem.PropertyName}";
                            errors.Add(message);
                        }
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;

                }
                catch
                {
                    transaction.Rollback();

                    throw;
                }
            }

            _logger.Info($"Leave ReportPackageService.ReviewRepudiation. reportPackageId={reportPackageId}, comments={comments}, currentOrgRegProgramId={currentOrgRegProgramId}, currentUserId={currentUserId}");

        }

        /// <summary>
        /// Iterates through all required element types for a given report package where content is not provided and 
        /// ensures there is at least one "sample & results" or "file" associated with the report package
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <returns>True if there is an association for all required element types where content is not provided</returns>
        public bool IsRequiredReportPackageElementTypesIncluded(int reportPackageId)
        {
            _logger.Info($"Enter ReportPackageService.IsRequiredReportPackageElementTypesIncluded. reportPackageId={reportPackageId}");

            var requiredReportPackageElementTypes = _dbContext.ReportPackageElementTypes
                                                        .Include(rpet => rpet.ReportPackageElementCategory.ReportElementCategory)
                                                        .Where(rpet => rpet.ReportPackageElementCategory.ReportPackageId == reportPackageId
                                                            && !rpet.ReportElementTypeIsContentProvided
                                                            && rpet.IsRequired)
                                                        .ToList();

            bool isRequirementsMet = true;
            foreach (var requiredRPET in requiredReportPackageElementTypes)
            {
                if (requiredRPET.ReportSamples.Count() < 1 && requiredRPET.ReportFiles.Count() < 1)
                {
                    isRequirementsMet = false;
                    break;
                }
            }

            _logger.Info($"Leaving ReportPackageService.IsRequiredReportPackageElementTypesIncluded. reportPackageId={reportPackageId}, isIncluded={isRequirementsMet}");

            return isRequirementsMet;
        }

        /// <summary>
        /// Updates the LastSentDateTimeUtc value of a row in table tReportPackage.
        /// Also optionally updates the "last sender" details.
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <param name="sentDateTime">eg. DateTimeOffset.Now</param>
        /// <param name="lastSenderUserId">Optional: tUserProfile.UserProfileId</param>
        /// <param name="lastSenderFirstName">Optional: tUserProfile.FirstName</param>
        /// <param name="lastSenderLastName">Optional: tUserProfile.LastName</param>
        public void UpdateLastSentDateTime(int reportPackageId, DateTimeOffset sentDateTime, int? lastSenderUserId = null, string lastSenderFirstName = null, string lastSenderLastName = null)
        {
            _logger.Info($"Enter ReportPackageService.UpdateLastSentDateTime. reportPackageId={reportPackageId}");

            var reportPackage = _dbContext.ReportPackages
                .SingleOrDefault(rp => rp.ReportPackageId == reportPackageId);

            if (reportPackage == null)
            {
                throw new Exception($"ERROR: Could not find Report Package associated with reportPackageId={reportPackageId}");
            }

            reportPackage.LastSentDateTimeUtc = sentDateTime;

            if (lastSenderUserId.HasValue)
            {
                reportPackage.LastSenderUserId = lastSenderUserId;
            }

            if (!String.IsNullOrWhiteSpace(lastSenderFirstName))
            {
                reportPackage.LastSenderFirstName = lastSenderFirstName;
            }

            if (!String.IsNullOrWhiteSpace(lastSenderLastName))
            {
                reportPackage.LastSenderLastName = lastSenderLastName;
            }

            _dbContext.SaveChanges();

            _logger.Info($"Leaving ReportPackageService.UpdateLastSentDateTime. reportPackageId={reportPackageId}");
        }

        /// <summary>
        /// Identifies if a newer Report Package (same industry, template name and same Reporting Period)
        /// exists with a newer SubmissionDateTimeUtc.
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId of the ReportPackage we want to compare the submission date/time with</param>
        /// <returns>True if a Report Package with a later submission date/time exists</returns>
        public bool IsSimilarReportPackageSubmittedAfter(int reportPackageId)
        {
            bool isNewerReportPackageExist = false;

            _logger.Info($"Enter ReportPackageService.UpdateLastSentDateTime. reportPackageId={reportPackageId}");

            var reportPackageToCompare = _dbContext.ReportPackages
                .SingleOrDefault(rp => rp.ReportPackageId == reportPackageId);

            if (reportPackageToCompare == null)
            {
                throw new Exception($"ERROR: Could not find Report Package associated with reportPackageId={reportPackageId}");
            }

            var startDate = reportPackageToCompare.PeriodStartDateTimeUtc.Date;
            var endDate = reportPackageToCompare.PeriodEndDateTimeUtc.Date;

            isNewerReportPackageExist = _dbContext.ReportPackages
                .Any(rp => rp.OrganizationRegulatoryProgramId == reportPackageToCompare.OrganizationRegulatoryProgramId
                    && rp.ReportPackageTemplateId == reportPackageToCompare.ReportPackageTemplateId
                    && rp.PeriodStartDateTimeUtc.Year == startDate.Year && rp.PeriodStartDateTimeUtc.Month == startDate.Month && rp.PeriodStartDateTimeUtc.Day == startDate.Day
                    && rp.PeriodEndDateTimeUtc.Year == endDate.Year && rp.PeriodEndDateTimeUtc.Month == endDate.Month && rp.PeriodEndDateTimeUtc.Day == endDate.Day
                    && rp.SubmissionDateTimeUtc > reportPackageToCompare.SubmissionDateTimeUtc);

            _logger.Info($"Leaving ReportPackageService.UpdateLastSentDateTime. reportPackageId={reportPackageId}, isNewerReportPackageExist={isNewerReportPackageExist}");

            return isNewerReportPackageExist;
        }

        public bool CanRepudiateReportPackage(int reportPackageId)
        {
            bool canRepudiate;
            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            _logger.Info($"Enter ReportPackageService.CanRepudiateReportPackage. reportPackageId={reportPackageId}, currentOrgRegProgramId={currentOrgRegProgramId}");

            //Check ARP config "Max days after report period end date to repudiate" has not passed (UC-19 5.2.)
            var reportRepudiatedDays = Convert.ToInt32(_settingService.GetOrgRegProgramSettingValue(currentOrgRegProgramId, SettingType.ReportRepudiatedDays));

            var reportPackage = _dbContext.ReportPackages
                .Single(rep => rep.ReportPackageId == reportPackageId);

            if (reportPackage.PeriodEndDateTimeUtc < DateTime.UtcNow.AddDays(-reportRepudiatedDays))
            {
                canRepudiate = false;
            }
            else
            {
                canRepudiate = true;
            }

            _logger.Info($"Enter ReportPackageService.CanRepudiateReportPackage. reportPackageId={reportPackageId}, currentOrgRegProgramId={currentOrgRegProgramId}, canRepudiate={canRepudiate}");

            return canRepudiate;
        }


        private string GenerateXmlString(CopyOfRecordDataXml dataXmlObj, ReportPackageDto reportPackageDto)
        {
            var strWriter = new Utf8StringWriter();
            var xmlSerializer = new XmlSerializer(dataXmlObj.GetType());
            xmlSerializer.Serialize(strWriter, dataXmlObj);
            var xmlString = strWriter.ToString();

            var xmlDoc = XDocument.Parse(xmlString);
            var elements = xmlDoc.Root.Elements().ToList();

            var certifications = GetXElementNode(elements, ReportElementCategoryName.Certifications.ToString());
            var samples = GetXElementNode(elements, "Samples");
            var attachments = GetXElementNode(elements, "FileManifest");
            var comment = GetXElementNode(elements, "Comment");

            var certificationsCloned = TakeOffXElementNode(certifications);
            var samplesCloned = TakeOffXElementNode(samples);
            var attachmentsCloned = TakeOffXElementNode(attachments);
            var commentCloned = TakeOffXElementNode(comment);

            foreach (var categoryName in reportPackageDto.ReportPackageElementCategories)
            {
                switch (categoryName)
                {
                    case ReportElementCategoryName.Attachments:
                        if (attachments != null)
                        {
                            xmlDoc.Root.Add(attachmentsCloned);
                        }
                        break;
                    case ReportElementCategoryName.Certifications:
                        if (certifications != null)
                        {
                            xmlDoc.Root.Add(certificationsCloned);
                        }
                        break;
                    case ReportElementCategoryName.SamplesAndResults:
                        if(samplesCloned != null)
                        {
                            xmlDoc.Root.Add(samplesCloned);
                        }
                        if (comment != null)
                        {
                            xmlDoc.Root.Add(commentCloned);
                        }
                        break;
                }
            }

            return xmlDoc.ToString();
        }

        private XElement TakeOffXElementNode(XElement node)
        {
            if (node != null)
            {
                var nodeCloned = new XElement(node);
                node.Remove();
                return nodeCloned;
            }

            return null;
        }

        private XElement GetXElementNode(IEnumerable<XElement> corElements, string tagName)
        {
            if (corElements.Any(i => i.Name.LocalName == tagName))
            {
                return corElements.Single(i => i.Name.LocalName == tagName);
            }

            return null;
        }

        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        private ReportStatusName GetReportStatusName(ReportPackage reportPackage)
        {
            var reportStatusName = _dbContext.ReportStatuses
                .Single(rs => rs.ReportStatusId == reportPackage.ReportStatusId).Name;

            return (ReportStatusName)Enum.Parse(typeof(ReportStatusName), reportStatusName);
        }

        /// <summary>
        /// Helper function used by both "GetReportPackagesByStatusName" and "GetReportPackage" methods
        /// </summary>
        /// <param name="reportPackage"></param>
        /// <param name="timeZoneId">Authority's local timezone</param>
        /// <returns>Mapped ReportPackageDto without children element data</returns>
        private ReportPackageDto GetReportPackageDtoFromReportPackage(ReportPackage reportPackage, int timeZoneId)
        {
            var reportPackagegDto = _mapHelper.GetReportPackageDtoFromReportPackage(reportPackage);

            reportPackagegDto.ReportStatusName = GetReportStatusName(reportPackage);

            reportPackagegDto.OrganizationRegulatoryProgramDto =
                    _programService.GetOrganizationRegulatoryProgram(reportPackage.OrganizationRegulatoryProgramId);

            if (reportPackagegDto.OrganizationRegulatoryProgramDto.RegulatorOrganizationId.HasValue)
            {
                reportPackagegDto.RecipientOrganizationRegulatoryProgramId = _dbContext.OrganizationRegulatoryPrograms
                    .Single(orp => orp.OrganizationId == reportPackagegDto.OrganizationRegulatoryProgramDto.RegulatorOrganizationId
                        && orp.RegulatoryProgramId == reportPackagegDto.OrganizationRegulatoryProgramDto.RegulatoryProgramId).OrganizationRegulatoryProgramId;
            }

            if (reportPackage.SubmissionDateTimeUtc.HasValue)
            {
                reportPackagegDto.SubmissionDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.SubmissionDateTimeUtc.Value.UtcDateTime, timeZoneId);
            }

            if (reportPackage.SubmissionReviewDateTimeUtc.HasValue)
            {
                reportPackagegDto.SubmissionReviewDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.SubmissionReviewDateTimeUtc.Value.UtcDateTime, timeZoneId);
            }

            if (reportPackage.RepudiationDateTimeUtc.HasValue)
            {
                reportPackagegDto.RepudiationDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.RepudiationDateTimeUtc.Value.UtcDateTime, timeZoneId);
            }

            if (reportPackage.RepudiationReviewDateTimeUtc.HasValue)
            {
                reportPackagegDto.RepudiationReviewDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.RepudiationReviewDateTimeUtc.Value.UtcDateTime, timeZoneId);
            }

            if (reportPackage.LastSentDateTimeUtc.HasValue)
            {
                reportPackagegDto.LastSentDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.LastSentDateTimeUtc.Value.UtcDateTime, timeZoneId);
            }

            reportPackagegDto.PeriodEndDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.PeriodEndDateTimeUtc.UtcDateTime, timeZoneId);

            reportPackagegDto.PeriodStartDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.PeriodStartDateTimeUtc.UtcDateTime, timeZoneId);

            reportPackagegDto.CreationDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.CreationDateTimeUtc.UtcDateTime, timeZoneId);

            reportPackagegDto.LastModificationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId((reportPackage.LastModificationDateTimeUtc.HasValue ? reportPackage.LastModificationDateTimeUtc.Value.UtcDateTime
                                                                    : reportPackage.CreationDateTimeUtc.UtcDateTime), timeZoneId);
            if (reportPackage.LastModifierUserId != null)
            {
                var lastModifier = _dbContext.Users
                    .Single(u => u.UserProfileId == reportPackage.LastModifierUserId);
                reportPackagegDto.LastModifierFullName = $"{lastModifier.FirstName} {lastModifier.LastName}";
            }

            return reportPackagegDto;
        }

        /// <summary>
        /// Used to simplify and clean up methods where there are multiple validation tests.
        /// </summary>
        /// <param name="message">Rule violation message to use when throwing the exception.</param>
        private void ThrowSimpleException(string message, string propertyName = null)
        {
            _logger.Info($"Enter SampleService.ThrowSimpleException. message={message}");

            List<RuleViolation> validationIssues = new List<RuleViolation>();
            validationIssues.Add(new RuleViolation(propertyName ?? string.Empty, propertyValue: null, errorMessage: message));

            _logger.Info($"Leaving SampleService.ThrowSimpleException. message={message}");

            throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
        }
    }
}