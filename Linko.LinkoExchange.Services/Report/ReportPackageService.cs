using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.CopyOfRecord;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Report.DataXML;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.User;
using NLog;
using FileInfo = Linko.LinkoExchange.Services.Report.DataXML.FileInfo;

namespace Linko.LinkoExchange.Services.Report
{
    public class ReportPackageService : BaseService, IReportPackageService
    {
        #region fields

        private readonly ICopyOfRecordService _copyOfRecordService;
        private readonly ICromerrAuditLogService _crommerAuditLogService;
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        private readonly ILinkoExchangeEmailService _linkoExchangeEmailService;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationService _orgService;
        private readonly IProgramService _programService;
        private readonly ISampleService _sampleService;
        private readonly ISettingService _settingService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly IUserService _userService;

        #endregion

        #region constructors and destructor

        public ReportPackageService(
            IProgramService programService,
            ICopyOfRecordService copyOfRecordService,
            ITimeZoneService timeZoneService,
            ILogger logger,
            LinkoExchangeContext linkoExchangeContext,
            IHttpContextService httpContextService,
            IUserService userService,
            ISettingService settingService,
            IOrganizationService orgService,
            ISampleService sampleService,
            IMapHelper mapHelper,
            ICromerrAuditLogService crommerAuditLogService,
            IOrganizationService organizationService,
            ILinkoExchangeEmailService linkoExchangeEmailService
        )
        {
            _programService = programService;
            _copyOfRecordService = copyOfRecordService;
            _timeZoneService = timeZoneService;
            _logger = logger;
            _dbContext = linkoExchangeContext;
            _httpContextService = httpContextService;
            _userService = userService;
            _settingService = settingService;
            _orgService = orgService;
            _sampleService = sampleService;
            _mapHelper = mapHelper;
            _crommerAuditLogService = crommerAuditLogService;
            _organizationService = organizationService;
            _linkoExchangeEmailService = linkoExchangeEmailService;
        }

        #endregion

        #region interface implementations

        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            var retVal = false;

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));
            var currentPortalName = _httpContextService.GetClaimValue(claimType:CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value:currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            var reportPackageId = id[0];
            var reportPackage = _dbContext.ReportPackages
                .SingleOrDefault(rp => rp.ReportPackageId == reportPackageId);

            if (reportPackage == null)
            {
                return false;
            }

            switch (apiName)
            {
                case "GetReportPackage":
                {

                    //Check authorized access as either one of:
                    //1) Industry - currentOrgRegProgramId == reportPackage.OrganizationRegulatoryProgramId
                    //2) Authority - currentOrgRegProgramId == Id of authority of reportPackage.OrganizationRegulatoryProgram
                    if (currentPortalName.Equals(value:OrganizationTypeName.Authority.ToString(), comparisonType: StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentOrgRegProgramId == _orgService.GetAuthority(orgRegProgramId:reportPackage.OrganizationRegulatoryProgramId).OrganizationRegulatoryProgramId)
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

                    if (reportPackage.OrganizationRegulatoryProgramId == currentOrgRegProgramId)
                    {
                        //Get current user
                        var orgRegProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                                          .SingleOrDefault(orpu => orpu.UserProfileId == currentUserId
                                                                                   && orpu.OrganizationRegulatoryProgramId == currentOrgRegProgramId);

                        //Does this user belong to this org reg program?
                        if (orgRegProgramUser == null)
                        {
                            return false;
                        }

                        //Check if user is removed or disabled
                        if (orgRegProgramUser.IsRemoved || !orgRegProgramUser.IsEnabled)
                        {
                            return false;
                        }

                        //Check if user signatory rights are required and current user "is signatory".
                        var rpTemplate = _dbContext.ReportPackageTempates
                                                   .SingleOrDefault(rpt => rpt.ReportPackageTemplateId == reportPackage.ReportPackageTemplateId);

                        if (rpTemplate == null || (rpTemplate.IsSubmissionBySignatoryRequired && !orgRegProgramUser.IsSignatory))
                        {
                            return false;
                        }

                        retVal = true;
                    }
                }

                    break;

                default: throw new Exception(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
            }

            return retVal;
        }

        /// <summary>
        ///     Note:  before call this function,  make sure to update draft first.
        /// </summary>
        /// <param name="reportPackageId"> </param>
        public void SignAndSubmitReportPackage(int reportPackageId)
        {
            var submitterUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));
            var submitterFirstName = _httpContextService.GetClaimValue(claimType:CacheKey.FirstName);
            var submitterLastName = _httpContextService.GetClaimValue(claimType:CacheKey.LastName);
            var submitterUserName = _httpContextService.GetClaimValue(claimType:CacheKey.UserName);

            _logger.Info(message:
                         $"Start: ReportPackageService.SignAndSubmitReportPackage. reportPackageId={reportPackageId}, submitterUserId={submitterUserId}, submitterUserName={submitterUserName}");

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    if (!CanUserExecuteApi(id:reportPackageId))
                    {
                        throw new UnauthorizedAccessException();
                    }

                    var reportPackage = _dbContext.ReportPackages.Include(i => i.ReportStatus)
                                                  .Single(i => i.ReportPackageId == reportPackageId);

                    if (reportPackage.ReportStatus.Name != ReportStatusName.ReadyToSubmit.ToString())
                    {
                        ThrowSimpleException(message:"Report Package is not ready to submit.");
                    }

                    var submitterTitleRole = _userService.GetUserProfileById(userProfileId:submitterUserId).TitleRole;
                    var submitterIpAddress = _httpContextService.CurrentUserIPAddress();

                    reportPackage.SubmissionDateTimeUtc = DateTimeOffset.Now;
                    reportPackage.SubmitterUserId = submitterUserId;
                    reportPackage.SubmitterFirstName = submitterFirstName;
                    reportPackage.SubmitterLastName = submitterLastName;
                    reportPackage.SubmitterTitleRole = submitterTitleRole;
                    reportPackage.SubmitterIPAddress = submitterIpAddress;
                    reportPackage.SubmitterUserName = submitterUserName;

                    UpdateStatus(reportPackageId:reportPackageId, reportStatus:ReportStatusName.Submitted, isUseTransaction:false);
                    _dbContext.SaveChanges();

                    var reportPackageDto = GetReportPackage(reportPackageId:reportPackageId, isIncludeAssociatedElementData:true);
                    var copyOfRecordDto = CreateCopyOfRecordForReportPackage(reportPackageDto:reportPackageDto);

                    // Add for crommer log
                    WriteCrommerrLog(reportPackageDto:reportPackageDto, submitterIpAddress:submitterIpAddress, copyOfRecordDto:copyOfRecordDto);

                    _dbContext.SaveChanges();
                    var emailEntries = new List<EmailEntry>();
                    if (reportPackageDto != null && copyOfRecordDto != null)
                    {
                        emailEntries = GetSignAndSubmitEmailEntries(reportPackage:reportPackageDto, copyOfRecordDto:copyOfRecordDto);
                
                        // Do email audit log.
                        _linkoExchangeEmailService.WriteEmailAuditLogs(emailEntries:emailEntries);
                    }

                    transaction.Commit();

                    if (reportPackageDto != null && copyOfRecordDto != null)
                    {
                        // Send emails.
                        _linkoExchangeEmailService.SendEmails(emailEntries:emailEntries);
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            _logger.Info(message:"End: ReportPackageService.SignAndSubmitReportPackage.");
        }

        public CopyOfRecordPdfFileDto GetReportPackageCopyOfRecordPdfFile(int reportPackageId)
        {
            var reportPackageDto = GetReportPackage(reportPackageId:reportPackageId, isIncludeAssociatedElementData:true, isAuthorizationRequired:true);
            return GetReportPackageCopyOfRecordPdfFile(reportPackageDto:reportPackageDto);
        }

        public CopyOfRecordPdfFileDto GetReportPackageCopyOfRecordPdfFile(ReportPackageDto reportPackageDto)
        {
            var pdfGenerator = new PdfGenerator(reportPackage:reportPackageDto);
            var draftMode = reportPackageDto.ReportStatusName == ReportStatusName.Draft || reportPackageDto.ReportStatusName == ReportStatusName.ReadyToSubmit;
            var pdfData = pdfGenerator.CreateCopyOfRecordPdf(draftMode:draftMode);

            return new CopyOfRecordPdfFileDto
                   {
                       FileData = pdfData,
                       FileName = "Copy Of Record.pdf"
                   };
        }

        public CopyOfRecordDataXmlFileDto GetReportPackageCopyOfRecordDataXmlFile(ReportPackageDto reportPackageDto)
        {
            _logger.Info(message:$"Start: ReportPackageService.GetReportPackageCopyOfRecordDataXmlFile. reportPackageId={reportPackageDto.ReportPackageId}");

            var dateTimeFormat = "MM/dd/yyyyThh:mm tt zzzz";
            var timeZoneName = _timeZoneService.GetTimeZoneNameUsingSettingForThisOrg(orgRegProgramId:reportPackageDto.OrganizationRegulatoryProgramId,
                                                                                      dateTime:reportPackageDto.SubmissionDateTimeLocal.Value, abbreviationName:false);

            var reportHeader = new ReportHeader
                               {
                                   ReportName = reportPackageDto.Name,
                                   ReportPeriodStartDateTimeUtc = reportPackageDto.PeriodStartDateTimeLocal.ToString(format:dateTimeFormat),
                                   ReportPeriodEndDateTimeUtc = reportPackageDto.PeriodEndDateTimeLocal.ToString(format:dateTimeFormat),
                                   ReportSubmissionDateUtc = reportPackageDto.SubmissionDateTimeLocal.Value.ToString(format:dateTimeFormat),
                                   AuthorityTimeZone = timeZoneName
                               };

            var dataXmlObj = new CopyOfRecordDataXml
                             {
                                 XmlFileVersion = new XmlFileVersion
                                                  {
                                                      VersionNumber = "1.0.1"
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
                    dataXmlObj.FileManifest.Files.Add(item:new FileInfo
                                                           {
                                                               OriginalFileName = fileStore.OriginalFileName,
                                                               SystemGeneratedUniqueFileName = fileStore.Name,
                                                               AttachmentType = fileStore.ReportElementTypeName
                                                           });
                }
            }

            dataXmlObj.FileManifest.Files.Add(
                                              item:new FileInfo
                                                   {
                                                       OriginalFileName = "Copy Of Record Data.xml",
                                                       SystemGeneratedUniqueFileName = "Copy Of Record Data.xml",
                                                       AttachmentType = "XML Raw Data"
                                                   });

            dataXmlObj.FileManifest.Files.Add(
                                              item:new FileInfo
                                                   {
                                                       OriginalFileName = "Copy Of Record.pdf",
                                                       SystemGeneratedUniqueFileName = "Copy Of Record.pdf",
                                                       AttachmentType = "Copy Of Record PDF"
                                                   });

            dataXmlObj.Certifications = new List<Certification>();
            dataXmlObj.Certifications = reportPackageDto.CertificationTypes.Where(cert => cert.IsIncluded)
                                                                            .Select(i => new Certification
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

                                         StartDateTimeUtc = sampleDto.StartDateTimeLocal.ToString(format:dateTimeFormat),
                                         EndDateTimeUtc = sampleDto.EndDateTimeLocal.ToString(format:dateTimeFormat),

                                         SampleFlowForMassCalcs = sampleDto.FlowEnteredValue.GetValueOrEmptyString(),
                                         SampleFlowForMassCalcsUnitName = sampleDto.FlowUnitName.GetValueOrEmptyString(),

                                         MassLoadingsConversionFactorPounds = sampleDto.MassLoadingConversionFactorPounds?.ToString(),
                                         MassLoadingCalculationDecimalPlaces = sampleDto.MassLoadingCalculationDecimalPlaces?.ToString(),
                                         IsMassLoadingResultToUseLessThanSign = sampleDto.IsMassLoadingResultToUseLessThanSign.ToString(),

                                         SampledBy = sampleDto.ByOrganizationTypeName.GetValueOrEmptyString(),
                                         SampleResults = new List<SampleResultNode>()
                                     };

                    dataXmlObj.Samples.Add(item:sampleNode);

                    foreach (var sampleResultDto in sampleDto.SampleResults)
                    {
                        var analysisDateTime = "";
                        if (sampleResultDto.AnalysisDateTimeLocal.HasValue)
                        {
                            analysisDateTime = sampleResultDto.AnalysisDateTimeLocal.Value.ToString(format:dateTimeFormat);
                        }

                        var sampleResultNode = new SampleResultNode
                                               {
                                                   ParameterName = sampleResultDto.ParameterName,
                                                   Qualifier = sampleResultDto.Qualifier.GetValueOrEmptyString(),
                                                   EnteredValue = sampleResultDto.EnteredValue.GetValueOrEmptyString(),
                                                   Value = sampleResultDto.Value?.ToString(provider:CultureInfo.InvariantCulture) ?? "",
                                                   UnitName = sampleResultDto.UnitName.GetValueOrEmptyString(),
                                                   EnteredMethodDetectionLimit = sampleResultDto.EnteredMethodDetectionLimit.GetValueOrEmptyString(),
                                                   MethodDetectionLimit = sampleResultDto.MethodDetectionLimit?.ToString() ?? "",
                                                   AnalysisMethod = sampleResultDto.AnalysisMethod.GetValueOrEmptyString(),
                                                   AnalysisDateTimeUtc = analysisDateTime,
                                                   LimitBasis = LimitBasisName.Concentration.ToString()
                                               };

                        sampleNode.SampleResults.Add(item:sampleResultNode);

                        if (!string.IsNullOrWhiteSpace(value:sampleResultDto.MassLoadingValue))
                        {
                            sampleResultNode = new SampleResultNode
                                               {
                                                   ParameterName = sampleResultDto.ParameterName,
                                                   Qualifier = sampleResultDto.Qualifier.GetValueOrEmptyString(),
                                                   Value = sampleResultDto.MassLoadingValue.GetValueOrEmptyString(),
                                                   UnitName = sampleResultDto.MassLoadingUnitName.GetValueOrEmptyString(),
                                                   LimitBasis = LimitBasisName.MassLoading.ToString()
                                               };

                            sampleNode.SampleResults.Add(item:sampleResultNode);
                        }
                    }
                }
            }

            var xmlString = GenerateXmlString(dataXmlObj:dataXmlObj, reportPackageDto:reportPackageDto);
            var xmlData = new CopyOfRecordDataXmlFileDto
                          {
                              FileData = Encoding.UTF8.GetBytes(s:xmlString),
                              FileName = "Copy Of Record Data.xml"
                          };

            _logger.Info(message:"End: ReportPackageService.GetReportPackageCopyOfRecordDataXmlFile.");

            return xmlData;
        }

        public CopyOfRecordValidationResultDto VerifyCopyOfRecord(int reportPackageId)
        {
            _logger.Info(message:"Start: ReportPackageService.VerififyCopyOfRecord. reportPackageId={0}", argument:reportPackageId);

            var validationResult = _copyOfRecordService.ValidCopyOfRecordData(reportPackageId:reportPackageId);

            _logger.Info(message:"End: ReportPackageService.VerififyCopyOfRecord.");

            return validationResult;
        }

        public ReportPackageDto GetReportPackage(int reportPackageId, bool isIncludeAssociatedElementData, bool isAuthorizationRequired = false)
        {
            _logger.Info(message:$"Start: ReportPackageService.GetReportPackage. reportPackageId={reportPackageId}");

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(value:_settingService.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));

            if (isAuthorizationRequired && !CanUserExecuteApi(id:reportPackageId))
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
                throw new Exception(message:$"ERROR: Could not find Report Package associated with reportPackageId={reportPackageId}");
            }

            var reportPackagegDto = GetReportPackageDtoFromReportPackage(reportPackage:reportPackage, timeZoneId:timeZoneId);

            //
            //ADD SAMPLE ASSOCIATIONS (AND OPTIONALLY SAMPLE DATA)
            //

            //Find entry in tReportPackageElementType for this reportPackage associated with Samples category
            var samplesReportPackageElementCategory = reportPackage.ReportPackageElementCategories
                                                                   .SingleOrDefault(rpet => rpet.ReportElementCategory.Name
                                                                                            == ReportElementCategoryName.SamplesAndResults.ToString());

            var sortedSamplesAndResultsTypes = new List<ReportPackageElementTypeDto>();
            if (samplesReportPackageElementCategory != null)
            {
                foreach (var existingSamplesReportPackageElementType in samplesReportPackageElementCategory.ReportPackageElementTypes)
                {
                    var reportPackageElementTypeDto =
                        _mapHelper.GetReportPackageElementTypeDtoFromReportPackageElementType(reportPackageElementType:existingSamplesReportPackageElementType);
                    reportPackageElementTypeDto.Samples = new List<SampleDto>();

                    //Should just be one iteration through this loop for the current phase, but in the future
                    //we might have more than one "Sample and Results" section in a Report Package

                    foreach (var reportSampleAssociated in existingSamplesReportPackageElementType.ReportSamples)
                    {
                        var sampleDto = new SampleDto
                                        {
                                            SampleId = reportSampleAssociated.SampleId
                                        };
                        if (isIncludeAssociatedElementData)
                        {
                            sampleDto = _sampleService.GetSampleDetails(sampleId:reportSampleAssociated.SampleId);
                        }
                        reportPackageElementTypeDto.Samples.Add(item:sampleDto);
                    }

                    sortedSamplesAndResultsTypes.Add(item:reportPackageElementTypeDto);
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
                    var reportPackageElementTypeDto =
                        _mapHelper.GetReportPackageElementTypeDtoFromReportPackageElementType(reportPackageElementType:existingFilesReportPackageElementType);
                    reportPackageElementTypeDto.FileStores = new List<FileStoreDto>();

                    foreach (var reportFileAssociated in existingFilesReportPackageElementType.ReportFiles)
                    {
                        var fileStoreDto = new FileStoreDto
                                           {
                                               FileStoreId = reportFileAssociated.FileStoreId
                                           };
                        if (isIncludeAssociatedElementData)
                        {
                            var fileStore = _dbContext.FileStores.Single(s => s.FileStoreId == reportFileAssociated.FileStoreId);
                            fileStoreDto = _mapHelper.GetFileStoreDtoFromFileStore(fileStore:fileStore);
                            fileStoreDto.Data = fileStore.FileStoreData.Data;
                            fileStoreDto.UploadDateTimeLocal =
                                _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:fileStore.UploadDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);
                        }
                        reportPackageElementTypeDto.FileStores.Add(item:fileStoreDto);
                    }

                    sortedAttachmentTypes.Add(item:reportPackageElementTypeDto);
                }

                sortedAttachmentTypes = sortedAttachmentTypes.OrderBy(item => item.SortOrder).ToList();
            }

            reportPackagegDto.AttachmentTypes = sortedAttachmentTypes;

            //
            //ADD CERTIFICATIONS
            //
            var certificationReportPackageElementCategory = reportPackage.ReportPackageElementCategories
                                                                         .SingleOrDefault(rpet => rpet.ReportElementCategory.Name
                                                                                                  == ReportElementCategoryName.Certifications.ToString());

            var sortedCertificationTypes = new List<ReportPackageElementTypeDto>();
            if (certificationReportPackageElementCategory != null)
            {
                foreach (var existingCertsReportPackageElementType in certificationReportPackageElementCategory.ReportPackageElementTypes)
                {
                    var reportPackageElementTypeDto =
                        _mapHelper.GetReportPackageElementTypeDtoFromReportPackageElementType(reportPackageElementType:existingCertsReportPackageElementType);
                    reportPackageElementTypeDto.FileStores = new List<FileStoreDto>();

                    foreach (var reportFileAssociated in existingCertsReportPackageElementType.ReportFiles)
                    {
                        var fileStoreDto = new FileStoreDto
                                           {
                                               FileStoreId = reportFileAssociated.FileStoreId
                                           };
                        if (isIncludeAssociatedElementData)
                        {
                            var fileStore = _dbContext.FileStores.Single(s => s.FileStoreId == reportFileAssociated.FileStoreId);
                            fileStoreDto = _mapHelper.GetFileStoreDtoFromFileStore(fileStore:fileStore);
                            fileStoreDto.UploadDateTimeLocal =
                                _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:fileStore.UploadDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);
                        }
                        reportPackageElementTypeDto.FileStores.Add(item:fileStoreDto);
                    }

                    sortedCertificationTypes.Add(item:reportPackageElementTypeDto);
                }

                sortedCertificationTypes = sortedCertificationTypes.OrderBy(item => item.SortOrder).ToList();
            }

            reportPackagegDto.CertificationTypes = sortedCertificationTypes;

            _logger.Info(message:"End: ReportPackageService.GetReportPackage.");
            return reportPackagegDto;
        }

        public CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId, ReportPackageDto reportPackageDto = null)
        {
            _logger.Info(message:$"Start: ReportPackageService.GetCopyOfRecordByReportPackageId. reportPackageId={reportPackageId}");

            if (reportPackageDto == null)
            {
                reportPackageDto = GetReportPackage(reportPackageId:reportPackageId, isIncludeAssociatedElementData:false, isAuthorizationRequired:true);
            }

            var copyOfRecordDto = _copyOfRecordService.GetCopyOfRecordByReportPackage(reportPackage:reportPackageDto);
            _logger.Info(message:"End: ReportPackageService.GetCopyOfRecordByReportPackageId.");

            return copyOfRecordDto;
        }

        /// <summary>
        ///     *WARNING: NO VALIDATION CHECK -- CASCADE DELETE*
        ///     Hard delete of row from tReportPackage table associated with passed in parameter.
        ///     Programatically cascade deletes rows in the following associated tables:
        ///     - tReportPackageElementCategory (via ReportPackageId)
        ///     - tReportPackageElementType (via ReportPackageElementCategoryId)
        ///     - tReportSample (via ReportPackageElementTypeId)
        ///     - tReportFile (via ReportPackageElementTypeId)
        ///     - tCopyofRecord (via ReportPackageId)
        /// </summary>
        /// <param name="reportPackageId"> tReportPackage.ReportPackageId </param>
        public ReportPackageDto DeleteReportPackage(int reportPackageId)
        {
            _logger.Info(message:$"Start: ReportPackageService.DeleteReportPackage. reportPackageId={reportPackageId}");

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
                            if (reportSamples.Any())
                            {
                                _dbContext.ReportSamples.RemoveRange(entities:reportSamples);
                            }

                            var reportFiles = _dbContext.ReportFiles
                                                        .Where(rf => rf.ReportPackageElementTypeId == rpet.ReportPackageElementTypeId);
                            if (reportFiles.Any())
                            {
                                _dbContext.ReportFiles.RemoveRange(entities:reportFiles);
                            }
                        }

                        _dbContext.ReportPackageElementTypes.RemoveRange(entities:rpec.ReportPackageElementTypes);
                    }

                    _dbContext.ReportPackageElementCategories.RemoveRange(entities:reportPackage.ReportPackageElementCategories);

                    _dbContext.ReportPackages.Remove(entity:reportPackage);

                    _dbContext.SaveChanges();

                    transaction.Commit();
                    var reportPackagegDto = _mapHelper.GetReportPackageDtoFromReportPackage(rpt:reportPackage);
                    reportPackagegDto.ReportStatusName = GetReportStatusName(reportPackage:reportPackage);
                    _logger.Info(message:$"End: ReportPackageService.DeleteReportPackage.");
                    return reportPackagegDto;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        ///     To be called after a User selects a template and date range but before
        ///     the User clicks the "Save Draft" button (no reportPackageDto to save yet)
        /// </summary>
        /// <param name="reportPackageTemplateId"> </param>
        /// <param name="startDateTimeLocal"> </param>
        /// <param name="endDateTimeLocal"> </param>
        /// <returns> The newly created tReportPackage.ReportPackageId </returns>
        public int CreateDraft(int reportPackageTemplateId, DateTime startDateTimeLocal, DateTime endDateTimeLocal)
        {
            _logger.Info(message:$"Start: ReportPackageService.CreateDraft. reportPackageTemplateId={reportPackageTemplateId}, "
                                 + $"startDateTimeLocal={startDateTimeLocal}, endDateTimeLocal={endDateTimeLocal}");

            int newReportPackageId;

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                    var currentOrgRegProgram = _dbContext.OrganizationRegulatoryPrograms
                                                         .Include(orp => orp.Organization)
                                                         .Include(orp => orp.Organization.Jurisdiction)
                                                         .Single(orp => orp.OrganizationRegulatoryProgramId == currentOrgRegProgramId);
                    var currentUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));
                    var timeZoneId = Convert.ToInt32(value:_settingService.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));

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
                        throw new Exception(message:$"The current Org Reg Program Id (= {currentOrgRegProgram}) is not assigned to Report Template Id = {reportPackageTemplateId}");
                    }

                    var newReportPackage = _mapHelper.GetReportPackageFromReportPackageTemplate(rpt:reportPackageTemplate);

                    //Need to ensure these range dates are set to the beginning of the day (12am) for the start date 
                    //and end of the day (11:59:59pm) for the end date
                    //
                    startDateTimeLocal = startDateTimeLocal.Date; //12am
                    endDateTimeLocal = endDateTimeLocal.Date.AddDays(value:1).AddSeconds(value:-1); //11:59:59
                    var startDateTimeUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:startDateTimeLocal, timeZoneId:timeZoneId);
                    var endDateTimeUtc = _timeZoneService.GetDateTimeOffsetFromLocalUsingThisTimeZoneId(localDateTime:endDateTimeLocal, timeZoneId:timeZoneId);

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
                        var newReportPackageElementCategory = new ReportPackageElementCategory
                                                              {
                                                                  ReportElementCategoryId = rptec.ReportElementCategoryId,
                                                                  SortOrder = rptec.SortOrder,
                                                                  ReportPackageElementTypes = new List<ReportPackageElementType>()
                                                              };
                        newReportPackage.ReportPackageElementCategories.Add(item:newReportPackageElementCategory); //handles setting ReportPackageId

                        foreach (var rptet in rptec.ReportPackageTemplateElementTypes)
                        {
                            var reportElementType = _dbContext.ReportElementTypes
                                                              .Include(ret => ret.CtsEventType)
                                                              .Single(ret => ret.ReportElementTypeId == rptet.ReportElementTypeId);

                            //Create a row in tReportPackageElementType
                            var newReportPackageElementType = new ReportPackageElementType
                                                              {
                                                                  ReportElementTypeId = rptet.ReportElementTypeId,
                                                                  ReportElementTypeName = reportElementType.Name,
                                                                  ReportElementTypeContent = reportElementType.Content,
                                                                  ReportElementTypeIsContentProvided = reportElementType.IsContentProvided
                                                              };

                            if (reportElementType.CtsEventType != null)
                            {
                                newReportPackageElementType.CtsEventTypeId = reportElementType.CtsEventType.CtsEventTypeId;
                                newReportPackageElementType.CtsEventTypeName = reportElementType.CtsEventType.Name;
                                newReportPackageElementType.CtsEventCategoryName = reportElementType.CtsEventType.CtsEventCategoryName;
                            }

                            newReportPackageElementType.IsRequired = rptet.IsRequired;

                            // NOTE:     
                            //          "IsIncluded" is used for certifications only.
                            //          Default should match the IsRequired value in RPT.  
                            //          If IsRequired, then Cert is automatically checked to be included, otherwise not checked
                            newReportPackageElementType.IsIncluded = rptet.IsRequired;

                            newReportPackageElementType.SortOrder = rptet.SortOrder;
                            newReportPackageElementType.ReportPackageElementCategory = newReportPackageElementCategory;

                            //handles setting ReportPackageElementCategoryId
                            newReportPackageElementCategory.ReportPackageElementTypes.Add(item:newReportPackageElementType);
                        }
                    }

                    _dbContext.ReportPackages.Add(entity:newReportPackage);

                    _dbContext.SaveChanges(); //Need to do this to get new Id for samplesAndResultsReportPackageElementCategory

                    var samplesAndResultsReportPackageElementCategory = _dbContext.ReportPackageElementCategories
                                                                                  .Include(rpec => rpec.ReportElementCategory)
                                                                                  .Include(rpec => rpec.ReportPackageElementTypes)
                                                                                  .SingleOrDefault(rpec => rpec.ReportPackageId == newReportPackage.ReportPackageId
                                                                                                           && rpec.ReportElementCategory.Name
                                                                                                           == ReportElementCategoryName.SamplesAndResults.ToString());

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
                                                                                && (s.StartDateTimeUtc <= endDateTimeUtc && s.StartDateTimeUtc >= startDateTimeUtc
                                                                                    || s.EndDateTimeUtc <= endDateTimeUtc && s.EndDateTimeUtc >= startDateTimeUtc));

                            foreach (var sample in existingEligibleSamples)
                            {
                                var reportSampleAssociation = new ReportSample
                                                              {
                                                                  ReportPackageElementTypeId = samplesAndResultsReportPackageElementType.ReportPackageElementTypeId,
                                                                  SampleId = sample.SampleId
                                                              };
                                _dbContext.ReportSamples.Add(entity:reportSampleAssociation);
                            }

                            _dbContext.SaveChanges();
                        }
                        else
                        {
                            //Log -- there is no Sample & Results element type for this Report Package
                            //  therefore Samples & Results could not be automatically added.
                            _logger.Info(message:
                                         $"WARNING: Missing 'Samples & Results' element type for this Report Package Template (reportPackageTemplateId={reportPackageTemplateId}). Samples could not be added during 'CreateDraft'.");
                        }
                    }
                    else
                    {
                        //Log -- there is no Sample & Results category for this Report Package
                        //  therefore Samples & Results could not be automatically added.
                        _logger.Info(message:
                                     $"WARNING: Missing 'Samples & Results' element category for this Report Package Template (reportPackageTemplateId={reportPackageTemplateId}). Samples could not be added during 'CreateDraft'.");
                    }

                    transaction.Commit();

                    newReportPackageId = newReportPackage.ReportPackageId;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            _logger.Info(message:
                         $"End: ReportPackageService.CreateDraft. newReportPackageId={newReportPackageId}, reportPackageTemplateId={reportPackageTemplateId}, startDateTimeLocal={startDateTimeLocal}, endDateTimeLocal={endDateTimeLocal}");

            return newReportPackageId;
        }

        /// <summary>
        ///     Cannot be used to CREATE, only to UPDATE. Use "CreateDraft" to create.
        ///     reportPackageDto.ReportPackageId must exist or exception thrown
        /// </summary>
        /// <param name="reportPackageDto"> Existing Report Package to update </param>
        /// <param name="isUseTransaction"> If true, runs within transaction object </param>
        /// <returns> Existing ReportPackage.ReportPackageId </returns>
        public int SaveReportPackage(ReportPackageDto reportPackageDto, bool isUseTransaction)
        {
            //Report Package will have already been saved before this call and therefore exists
            if (reportPackageDto.ReportPackageId < 1)
            {
                throw new Exception(message:"ERROR: Cannot call 'SaveReportPackage' without setting reportPackageDto.ReportPackageId.");
            }

            _logger.Info(message:$"Start: ReportPackageService.SaveReportPackage. ReportPackageId={reportPackageDto.ReportPackageId}");

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
                                                                       .SingleOrDefault(rpet => rpet.ReportElementCategory.Name
                                                                                                == ReportElementCategoryName.SamplesAndResults.ToString());

                if (samplesReportPackageElementCategory != null)
                {
                    //Handle deletions first
                    // - Iterate through all SampleAndResult rows in ReportSample and delete ones that cannot be matched with an item in reportPackageDto.SamplesAndResultsTypes
                    foreach (var existingSamplesReportPackageElementType in samplesReportPackageElementCategory.ReportPackageElementTypes)
                    {
                        //Should just be one iteration through this loop for the current phase, but in the future
                        //we might have more than one "Sample and Results" section in a Report Package

                        var existingReportSamples = existingSamplesReportPackageElementType.ReportSamples.ToArray();
                        foreach (var existingReportSample in existingReportSamples) {
                            //Find match in dto samples
                            var matchedSampleAssociation =
                                reportPackageDto.SamplesAndResultsTypes
                                                .SingleOrDefault(rpet => rpet.ReportPackageElementTypeId == existingReportSample.ReportPackageElementTypeId)
                                                ?.Samples.SingleOrDefault(s => s.SampleId == existingReportSample.SampleId);

                            if (matchedSampleAssociation == null)
                            {
                                //existing association must have been deleted -- remove
                                _dbContext.ReportSamples.Remove(entity:existingReportSample);
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
                                _dbContext.ReportSamples.Add(entity:new ReportSample
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
                        _logger.Info(message:
                                     $"WARNING: Missing 'Samples & Results' element category for this Report Package (ReportPackageId={reportPackageDto.ReportPackageId}). Samples could not be added.");
                    }
                }

                //ATTACHMENTS
                //===================

                //Find entry in tReportPackageElementType for this reportPackage associated with Attachments category
                var attachmentsReportPackageElementCategory = reportPackage.ReportPackageElementCategories
                                                                           .SingleOrDefault(rpet => rpet.ReportElementCategory.Name
                                                                                                    == ReportElementCategoryName.Attachments.ToString());

                if (attachmentsReportPackageElementCategory != null)
                {
                    //Handle deletions first
                    // - Iterate through all Attachment rows in ReportFile and delete ones that cannot be matched with an item in reportPackageDto.AttachmentTypes
                    foreach (var existingAttachmentsReportPackageElementType in attachmentsReportPackageElementCategory.ReportPackageElementTypes)
                    {
                        var existingReportFiles = existingAttachmentsReportPackageElementType.ReportFiles.ToArray();
                        
                        foreach (var existingReportFile in existingReportFiles) {
                            //Find match in dto files
                            var matchedFileAssociation = reportPackageDto
                                .AttachmentTypes.SingleOrDefault(rpet => rpet.ReportPackageElementTypeId == existingReportFile.ReportPackageElementTypeId)?.FileStores
                                .SingleOrDefault(fs => fs.FileStoreId == existingReportFile.FileStoreId);

                            if (matchedFileAssociation == null)
                            {
                                //existing association must have been deleted -- remove
                                _dbContext.ReportFiles.Remove(entity:existingReportFile);
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
                                _dbContext.ReportFiles.Add(entity:new ReportFile
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
                        _logger.Info(message:
                                     $"WARNING: Missing 'Attachments' element category for this Report Package (ReportPackageId={reportPackageDto.ReportPackageId}). Attachments could not be added.");
                    }
                }

                //CERTIFICATIONS
                //===================
                
                //Can only update "Is Accepted" or not

                var certsReportPackageElementCategory = reportPackage.ReportPackageElementCategories
                                                                     .SingleOrDefault(rpet => rpet.ReportElementCategory.Name
                                                                                              == ReportElementCategoryName.Certifications.ToString());
                if (certsReportPackageElementCategory != null)
                {
                    foreach (var certificationElementType in reportPackageDto.CertificationTypes)
                    {
                        var certificationElementTypeToUpdate = certsReportPackageElementCategory.ReportPackageElementTypes
                                                                                                .Single(cert => cert.ReportPackageElementTypeId
                                                                                                                == certificationElementType.ReportPackageElementTypeId);
                        certificationElementTypeToUpdate.IsIncluded = certificationElementType.IsIncluded;
                    }

                }
                else
                {
                    if (reportPackageDto.CertificationTypes != null && reportPackageDto.CertificationTypes.Count > 0)
                    {
                        _logger.Info(message:
                                     $"WARNING: Missing 'Certifications' element category for this Report Package (ReportPackageId={reportPackageDto.ReportPackageId}). Certifications could not be added.");
                    }
                }

                _dbContext.SaveChanges();

                if (isUseTransaction)
                {
                    transaction.Commit();
                }

                _logger.Info(message:$"End: ReportPackageService.SaveReportPackage.");

                return reportPackage.ReportPackageId;
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
        ///     Performs validation to ensure only allowed state transitions are occur,
        ///     throw RuleViolationException otherwise. Does NOT enter any corresponding values into the Report Package row.
        /// </summary>
        /// <param name="reportPackageId"> </param>
        /// <param name="reportStatus"> Intended target state </param>
        /// <param name="isUseTransaction"> If true, runs within transaction object </param>
        public void UpdateStatus(int reportPackageId, ReportStatusName reportStatus, bool isUseTransaction)
        {
            _logger.Info(message:$"Start: ReportPackageService.UpdateStatus. reportPackageId={reportPackageId}, reportStatus={reportStatus}");

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

                    //...but check to see if required Report Package Element Types are included.
                    CheckRequiredReportPackageElementTypesIncluded(reportPackageId: reportPackageId);
                }
                else if (previousStatus == ReportStatusName.ReadyToSubmit.ToString() && reportStatus == ReportStatusName.Draft)
                {
                    //allowed
                }
                else if (previousStatus == ReportStatusName.ReadyToSubmit.ToString() && reportStatus == ReportStatusName.Submitted)
                {
                    //allowed

                    //...but check to see if required Report Package Element Types are included.
                    //(should already be OK since these were checked when putting report package into ReadyToSubmit state)
                    CheckRequiredReportPackageElementTypesIncluded(reportPackageId: reportPackageId);
                }
                else if (previousStatus == ReportStatusName.Submitted.ToString() && reportStatus == ReportStatusName.Repudiated)
                {
                    //allowed
                }
                else
                {
                    //not allowed
                    ThrowSimpleException(message:$"Cannot change a Report Package status from '{previousStatus}' to '{reportStatus}'.");
                }

                var targetReportStatusName = reportStatus.ToString();
                var targetReportStatusId = _dbContext.ReportStatuses
                                                     .Single(rs => rs.Name == targetReportStatusName).ReportStatusId;
                reportPackage.ReportStatusId = targetReportStatusId;

                _dbContext.SaveChanges();

                if (isUseTransaction)
                {
                    transaction.Commit();
                }
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

            _logger.Info(message:$"End: ReportPackageService.UpdateStatus.");
        }

        /// <summary>
        ///     Gets a collection of FileStoreDto's that are eligible to be added this Report Package -- also indicate which are already associated.
        /// </summary>
        /// <param name="reportPackageElementTypeId"> Report Package Element Type identifier </param>
        /// <returns> Collection of FileStoreDto objects </returns>
        public ICollection<FileStoreDto> GetFilesForSelection(int reportPackageElementTypeId)
        {
            _logger.Info(message:$"Start: ReportPackageService.GetFilesForSelection. reportPackageElementTypeId={reportPackageElementTypeId}");

            var existingFilesReportPackageElementType = _dbContext.ReportPackageElementTypes
                                                                  .Include(rp => rp.ReportPackageElementCategory.ReportPackage)
                                                                  .Include(rp => rp.ReportPackageElementCategory.ReportElementCategory)
                                                                  .Include(rp => rp.ReportFiles)
                                                                  .Single(rp => rp.ReportPackageElementTypeId == reportPackageElementTypeId);

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(value:_settingService.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));

            var reportPackage = existingFilesReportPackageElementType.ReportPackageElementCategory.ReportPackage;
            
            var fileStoreList = new List<FileStoreDto>();
            var ageInMonthsSinceFileUploaded = int.Parse(s:_settingService.GetGlobalSettings()[key:SystemSettingType.FileAvailableToAttachMaxAgeMonths]);

            var xMonthsAgo = reportPackage.CreationDateTimeUtc.AddMonths(months:-ageInMonthsSinceFileUploaded);

            if (existingFilesReportPackageElementType.ReportPackageElementCategory.ReportElementCategory.Name != ReportElementCategoryName.Attachments.ToString())
            {
                //throw Exception
                throw new Exception(message:$"ERROR: Passed in reportPackageElementTypeId={reportPackageElementTypeId} does not correspond with a Attachments.");
            }

            var filesOfThisReportElementType = _dbContext.FileStores
                                                         .Where(fs => fs.OrganizationRegulatoryProgramId == reportPackage.OrganizationRegulatoryProgramId
                                                                      && fs.ReportElementTypeId == existingFilesReportPackageElementType.ReportElementTypeId
                                                                      && fs.UploadDateTimeUtc >= xMonthsAgo)
                                                         .ToList();

            foreach (var eligibleFile in filesOfThisReportElementType)
            {
                var fileStoreDto = _mapHelper.GetFileStoreDtoFromFileStore(fileStore:eligibleFile);
                fileStoreDto.ReportPackageElementTypeId = existingFilesReportPackageElementType.ReportPackageElementTypeId;
                fileStoreDto.IsAssociatedWithReportPackage = existingFilesReportPackageElementType
                    .ReportFiles.Any(rf => rf.FileStoreId == eligibleFile.FileStoreId);
                fileStoreDto.UploadDateTimeLocal =
                    _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:eligibleFile.UploadDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);
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
                    fileStoreDto.LastSubmissionDateTimeLocal =
                        _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:lastSubmittedReportPackage.SubmissionDateTimeUtc.Value.UtcDateTime,
                                                                                 timeZoneId:timeZoneId);
                }

                fileStoreList.Add(item:fileStoreDto);
            }

            _logger.Info(message: $"End: ReportPackageService.GetFilesForSelection. Count={fileStoreList.Count}");

            return fileStoreList;
        }

        /// <summary>
        ///     Gets a collection of SampleDto's that are eligible to be added this Report Package -- also indicate which are already associated.
        /// </summary>
        /// <param name="reportPackageElementTypeId"> tReportPackage.reportPackageElementTypeId </param>
        /// <returns> Collection of SampleDto objects </returns>
        public ICollection<SampleDto> GetSamplesForSelection(int reportPackageElementTypeId)
        {
            _logger.Info(message:$"Start: ReportPackageService.GetSamplesForSelection. reportPackageElementTypeId={reportPackageElementTypeId}");

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(value:_settingService.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));

            var existingSamplesReportPackageElementType = _dbContext.ReportPackageElementTypes
                                                                    .Include(rp => rp.ReportPackageElementCategory.ReportPackage)
                                                                    .Include(rp => rp.ReportPackageElementCategory.ReportElementCategory)
                                                                    .Include(rp => rp.ReportSamples)
                                                                    .Single(rp => rp.ReportPackageElementTypeId == reportPackageElementTypeId);

            if (existingSamplesReportPackageElementType.ReportPackageElementCategory.ReportElementCategory.Name != ReportElementCategoryName.SamplesAndResults.ToString())
            {
                throw new Exception(message:$"ERROR: Passed in reportPackageElementTypeId={reportPackageElementTypeId} does not correspond with a Samples and Results.");
            }

            var reportPackage = existingSamplesReportPackageElementType.ReportPackageElementCategory.ReportPackage;

            var eligibleSampleList = new List<SampleDto>();

            var existingEligibleSamples = _dbContext.Samples
                                                    .Include(s => s.SampleResults)
                                                    .Where(s => s.ForOrganizationRegulatoryProgramId == reportPackage.OrganizationRegulatoryProgramId
                                                                && s.IsReadyToReport
                                                                && (s.StartDateTimeUtc <= reportPackage.PeriodEndDateTimeUtc
                                                                    && s.StartDateTimeUtc >= reportPackage.PeriodStartDateTimeUtc
                                                                    || s.EndDateTimeUtc <= reportPackage.PeriodEndDateTimeUtc
                                                                    && s.EndDateTimeUtc >= reportPackage.PeriodStartDateTimeUtc))
                                                    .ToList();

            foreach (var existingEligibleSample in existingEligibleSamples)
            {
                var sampleDto = _sampleService.GetSampleDetails(sample:existingEligibleSample, isLoggingEnabled:false);

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
                    sampleDto.LastSubmissionDateTimeLocal =
                        _timeZoneService.GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:lastSubmittedReportPackage.SubmissionDateTimeUtc.Value.UtcDateTime,
                                                                                 timeZoneId:timeZoneId);
                }

                eligibleSampleList.Add(item:sampleDto);
            }

            _logger.Info(message: $"End: ReportPackageService.GetSamplesForSelection. Count={eligibleSampleList.Count}");

            return eligibleSampleList;
        }

        public ReportPackageElementTypeDto GetReportReportPackageElementType(int reportPackageElementTypeId)
        {
            var reportPackageElementType = _dbContext.ReportPackageElementTypes
                                                     .Single(rpet => rpet.ReportPackageElementTypeId == reportPackageElementTypeId);

            var dto = _mapHelper.GetReportPackageElementTypeDtoFromReportPackageElementType(reportPackageElementType:reportPackageElementType);

            return dto;
        }

        public List<ReportPackageStatusCount> GetReportPackageStatusCounts()
        {
            _logger.Info(message:"Start: ReportPackageService.GetReportPackageStatusCounts.");

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentOrganizationId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationId));
            var currentRegulatoryProgramId = _dbContext.OrganizationRegulatoryPrograms
                                                       .Single(orp => orp.OrganizationRegulatoryProgramId == currentOrgRegProgramId)
                                                       .RegulatoryProgramId;

            var rptStatusCounts = new List<ReportPackageStatusCount>();

            var isAuthorityViewing = _httpContextService.GetClaimValue(claimType:CacheKey.PortalName).ToLower().Equals(value:"authority");

            var reportPackages = _dbContext.ReportPackages
                                           .Include(rp => rp.ReportStatus)
                                           .Include(rp => rp.OrganizationRegulatoryProgram);

            if (isAuthorityViewing)
            {
                // For authority, return submitted pending review count and repudiation pending review count
                var submittedPendingReview = new ReportPackageStatusCount {Status = ReportStatusName.SubmittedPendingReview};
                var repudiationRendingReview = new ReportPackageStatusCount {Status = ReportStatusName.RepudiatedPendingReview};

                reportPackages = reportPackages
                    .Where(rp => rp.OrganizationRegulatoryProgram.RegulatorOrganizationId == currentOrganizationId
                                 && rp.OrganizationRegulatoryProgram.RegulatoryProgramId == currentRegulatoryProgramId);

                submittedPendingReview.Count = reportPackages.Count(rp => rp.ReportStatus.Name == ReportStatusName.Submitted.ToString()
                                                                          && rp.SubmissionReviewDateTimeUtc == null);

                repudiationRendingReview.Count = reportPackages
                    .Count(rp => rp.ReportStatus.Name == ReportStatusName.Repudiated.ToString()
                                 && rp.RepudiationReviewDateTimeUtc == null);

                rptStatusCounts.Add(item:submittedPendingReview);
                rptStatusCounts.Add(item:repudiationRendingReview);
            }
            else
            {
                // For industry portal, return daft count and ready to submit count
                var draftCount = new ReportPackageStatusCount {Status = ReportStatusName.Draft};
                var readyToSubmitCount = new ReportPackageStatusCount {Status = ReportStatusName.ReadyToSubmit};

                reportPackages = reportPackages
                    .Where(rp => rp.OrganizationRegulatoryProgramId == currentOrgRegProgramId);

                draftCount.Count = reportPackages.Count(i => i.ReportStatus.Name == ReportStatusName.Draft.ToString());
                readyToSubmitCount.Count = reportPackages.Count(i => i.ReportStatus.Name == ReportStatusName.ReadyToSubmit.ToString());

                rptStatusCounts.Add(item:draftCount);
                rptStatusCounts.Add(item:readyToSubmitCount);
            }

            _logger.Info(message:"End: ReportPackageService.GetReportPackageStatusCounts.");
            return rptStatusCounts;
        }

        /// <summary>
        ///     Gets Report Package information (without children element data) for displaying in a grid.
        /// </summary>
        /// <param name="reportStatusName"> Fetches report packages of this status only </param>
        /// <returns> Collection of ReportPackageDto objects (without children element data) </returns>
        public IEnumerable<ReportPackageDto> GetReportPackagesByStatusName(ReportStatusName reportStatusName)
        {
            _logger.Info(message:$"Start: ReportPackageService.GetReportPackagesByStatusName. reportStatusName={reportStatusName}");
            var isAuthorityViewing = _httpContextService.GetClaimValue(claimType:CacheKey.PortalName).ToLower()
                                                        .Equals(value:OrganizationTypeName.Authority.ToString(), comparisonType:StringComparison.OrdinalIgnoreCase);
            var reportPackageDtoList = new List<ReportPackageDto>();

            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentOrganizationId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationId));
            var currentRegulatoryProgramId = _dbContext.OrganizationRegulatoryPrograms
                                                       .Single(orp => orp.OrganizationRegulatoryProgramId == currentOrgRegProgramId)
                                                       .RegulatoryProgramId;
            var timeZoneId = Convert.ToInt32(value:_settingService.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));

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
                var reportPackagegDto = GetReportPackageDtoFromReportPackage(reportPackage:reportPackage, timeZoneId:timeZoneId);
                reportPackageDtoList.Add(item:reportPackagegDto);
            }

            _logger.Info(message:$"End: ReportPackageService.GetReportPackagesByStatusName.");

            return reportPackageDtoList;
        }

        /// <summary>
        ///     Gets items to populate a dropdown list of reasons to repudiate a report package (for a specific Org Reg Program Id)
        /// </summary>
        /// <returns> </returns>
        public IEnumerable<RepudiationReasonDto> GetRepudiationReasons()
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authorityOrganization = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId);
            var currentUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));
            var timeZoneId = Convert.ToInt32(value:_settingService.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));

            _logger.Info(message:$"Start: ReportPackageService.GetRepudiationReasons. currentOrgRegProgramId={currentOrgRegProgramId}, currentUserId={currentUserId}");

            var repudiationReasonDtos = new List<RepudiationReasonDto>();

            var repudiationReasons = _dbContext.RepudiationReasons
                                               .Where(rr => rr.OrganizationRegulatoryProgramId == authorityOrganization.OrganizationRegulatoryProgramId);

            foreach (var repudiationReason in repudiationReasons)
            {
                var repudiationReasonDto = _mapHelper.GetRepudiationReasonDtoFromRepudiationReason(repudiationReason:repudiationReason);
                repudiationReasonDto.CreationLocalDateTime = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:repudiationReason.CreationDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);

                if (repudiationReason.LastModificationDateTimeUtc.HasValue)
                {
                    repudiationReasonDto.LastModificationLocalDateTime = _timeZoneService
                        .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:repudiationReason.LastModificationDateTimeUtc.Value.UtcDateTime, timeZoneId:timeZoneId);
                }

                repudiationReasonDtos.Add(item:repudiationReasonDto);
            }

            _logger.Info(message:$"End: ReportPackageService.GetRepudiationReasons.");

            return repudiationReasonDtos;
        }

        /// <summary>
        ///     Performs various validation checks before putting a report package into "Repudiated" status.
        ///     Also logs action and emails stakeholders.
        /// </summary>
        /// <param name="reportPackageId"> tReportPackage.ReportPackageId </param>
        /// <param name="repudiationReasonId"> tRepudiationReason.RepudiationReasonId </param>
        /// <param name="repudiationReasonName"> Snapshot of tRepudiationReason.Name </param>
        /// <param name="comments"> Optional field to accompany "other reason" </param>
        public void RepudiateReport(int reportPackageId, int repudiationReasonId, string repudiationReasonName, string comments = null)
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));

            _logger.Info(message:$"Start: ReportPackageService.RepudiateReport. reportPackageId={reportPackageId}, "
                                 + $"repudiationReasonId={repudiationReasonId}, currentOrgRegProgramId={currentOrgRegProgramId}, "
                                 + $"currentUserId={currentUserId}");

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    if (!CanUserExecuteApi(id:reportPackageId))
                    {
                        throw new UnauthorizedAccessException();
                    }

                    var authorityOrganizationRegulatoryProgramDto = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId);
                    var timeZoneId = Convert.ToInt32(value:_settingService.GetOrganizationSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.TimeZone));
                    var timeZone = _dbContext.TimeZones.Single(tz => tz.TimeZoneId == timeZoneId);

                    //Check ARP config "Max days after report period end date to repudiate" has not passed (UC-19 5.2.)
                    var reportRepudiatedDays =
                        Convert.ToInt32(value:
                                        _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authorityOrganizationRegulatoryProgramDto.OrganizationRegulatoryProgramId,
                                                                                     settingType:SettingType.ReportRepudiatedDays));

                    var reportPackage = _dbContext.ReportPackages
                                                  .Include(rep => rep.OrganizationRegulatoryProgram)
                                                  .Single(rep => rep.ReportPackageId == reportPackageId);

                    if (reportPackage.PeriodEndDateTimeUtc < DateTime.UtcNow.AddDays(value:-reportRepudiatedDays))
                    {
                        ThrowSimpleException(message:$"Report repudiation time period of {reportRepudiatedDays} days has expired.");
                    }

                    //Check valid reason (UC-19 7.a.)
                    if (repudiationReasonId < 1)
                    {
                        ThrowSimpleException(message:"Reason is required.", propertyName:"RepudiationReasonId");
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
                    if (!string.IsNullOrEmpty(value:comments))
                    {
                        reportPackage.RepudiationComments = comments;
                    }

                    reportPackage.LastModificationDateTimeUtc = reportPackage.RepudiationDateTimeUtc;
                    reportPackage.LastModifierUserId = currentUserId;

                    //Change status
                    UpdateStatus(reportPackageId:reportPackageId, reportStatus:ReportStatusName.Repudiated, isUseTransaction:false);

                    //===========
                    //Send emails
                    //===========

                    //System sends Repudiation Receipt email to all Signatories for Industry (UC-19 8.3.)

                    var corHash = _dbContext.CopyOfRecords.SingleOrDefault(cor => cor.ReportPackageId == reportPackageId);

                    if (corHash == null)
                    {
                        ThrowSimpleException(message:$"ERROR: Could not find COR associated with ReportPackageId={reportPackageId}");
                    }

                    //Use the same contentReplacement dictionary for both emails and Cromerr audit logging
                    var contentReplacements = new Dictionary<string, string>
                                              {
                                                  {"reportPackageName", reportPackage.Name},
                                                  {
                                                      "periodStartDate",
                                                      _timeZoneService
                                                          .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.PeriodStartDateTimeUtc.UtcDateTime,
                                                                                                   timeZoneId:timeZoneId).ToString(format:"MMM d, yyyy")
                                                  },
                                                  {
                                                      "periodEndDate",
                                                      _timeZoneService
                                                          .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.PeriodEndDateTimeUtc.UtcDateTime,
                                                                                                   timeZoneId:timeZoneId).ToString(format:"MMM d, yyyy")
                                                  },
                                                  {
                                                      "submissionDateTime",
                                                      _timeZoneService
                                                          .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.SubmissionDateTimeUtc.Value.UtcDateTime,
                                                                                                   timeZoneId:timeZoneId).ToString(format:"MMM d, yyyy h:mmtt")
                                                      + $" {_timeZoneService.GetTimeZoneNameUsingThisTimeZone(leTimeZone:timeZone, dateTime:reportPackage.SubmissionDateTimeUtc.Value.UtcDateTime, abbreviationName:true)}"
                                                  },
                                                  {"corSignature", corHash.Signature},
                                                  {
                                                      "repudiatedDateTime",
                                                      _timeZoneService
                                                          .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.RepudiationDateTimeUtc.Value.UtcDateTime,
                                                                                                   timeZoneId:timeZoneId).ToString(format:"MMM d, yyyy h:mmtt")
                                                      + $" {_timeZoneService.GetTimeZoneNameUsingThisTimeZone(leTimeZone:timeZone, dateTime:reportPackage.RepudiationDateTimeUtc.Value.UtcDateTime, abbreviationName:true)}"
                                                  },
                                                  {"repudiationReason", repudiationReasonName},
                                                  {"repudiationReasonComments", comments ?? ""},
                                                  {"authOrganizationName", reportPackage.RecipientOrganizationName},
                                                  {"authOrganizationAddressLine1", reportPackage.RecipientOrganizationAddressLine1}
                                              };

                    //Report Details:

                    //Repudiated to:

                    var authOrganizationAddressLine2 = "";
                    if (!string.IsNullOrWhiteSpace(value:reportPackage.RecipientOrganizationAddressLine2))
                    {
                        authOrganizationAddressLine2 = $"{Environment.NewLine}\t{reportPackage.RecipientOrganizationAddressLine2}";
                    }
                    contentReplacements.Add(key:"authOrganizationAddressLine2", value:authOrganizationAddressLine2);

                    contentReplacements.Add(key:"authOrganizationCityName", value:reportPackage.RecipientOrganizationCityName);
                    contentReplacements.Add(key:"authOrganizationJurisdictionName", value:reportPackage.RecipientOrganizationJurisdictionName);
                    contentReplacements.Add(key:"authOrganizationZipCode", value:reportPackage.RecipientOrganizationZipCode);

                    //Repudiated by:
                    contentReplacements.Add(key:"submitterFirstName", value:currentUser.FirstName);
                    contentReplacements.Add(key:"submitterLastName", value:currentUser.LastName);
                    contentReplacements.Add(key:"submitterTitle", value:currentUser.TitleRole);
                    contentReplacements.Add(key:"iuOrganizationName", value:reportPackage.OrganizationName);
                    contentReplacements.Add(key:"permitNumber", value:reportPackage.OrganizationRegulatoryProgram.ReferenceNumber);
                    contentReplacements.Add(key:"organizationAddressLine1", value:reportPackage.OrganizationAddressLine1);

                    var organizationAddressLine2 = "";
                    if (!string.IsNullOrWhiteSpace(value:reportPackage.OrganizationAddressLine2))
                    {
                        organizationAddressLine2 = $"{Environment.NewLine}\t{reportPackage.OrganizationAddressLine2}";
                    }
                    contentReplacements.Add(key:"organizationAddressLine2", value:organizationAddressLine2);

                    contentReplacements.Add(key:"organizationCityName", value:reportPackage.OrganizationCityName);
                    contentReplacements.Add(key:"organizationJurisdictionName", value:reportPackage.OrganizationJurisdictionName);
                    contentReplacements.Add(key:"organizationZipCode", value:reportPackage.OrganizationZipCode);

                    contentReplacements.Add(key:"userName", value:currentUser.UserName);
                    contentReplacements.Add(key:"corViewLink", value:$"{_httpContextService.GetRequestBaseUrl()}reportPackage/{reportPackage.ReportPackageId}/Details");

                    var authorityName = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authorityOrganizationRegulatoryProgramDto.OrganizationRegulatoryProgramId,
                                                                                     settingType:SettingType.EmailContactInfoName);
                    var authorityEmail = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authorityOrganizationRegulatoryProgramDto.OrganizationRegulatoryProgramId,
                                                                                      settingType:SettingType.EmailContactInfoEmailAddress);
                    var authorityPhone = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authorityOrganizationRegulatoryProgramDto.OrganizationRegulatoryProgramId,
                                                                                      settingType:SettingType.EmailContactInfoPhone);

                    contentReplacements.Add(key:"authorityName", value:authorityName);
                    contentReplacements.Add(key:"authoritySupportEmail", value:authorityEmail);
                    contentReplacements.Add(key:"authoritySupportPhoneNumber", value:authorityPhone);

                    //For Cromerr
                    contentReplacements.Add(key:"organizationName", value:reportPackage.OrganizationName);
                    contentReplacements.Add(key:"firstName", value:currentUser.FirstName);
                    contentReplacements.Add(key:"lastName", value:currentUser.LastName);
                    contentReplacements.Add(key:"emailAddress", value:currentUser.Email);

                    //Cromerr Log (UC-19 8.6.)
                    var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                                  {
                                                      RegulatoryProgramId = reportPackage.OrganizationRegulatoryProgram.RegulatoryProgramId,
                                                      OrganizationId = reportPackage.OrganizationRegulatoryProgram.OrganizationId
                                                  };
                    cromerrAuditLogEntryDto.RegulatorOrganizationId = reportPackage.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                    cromerrAuditLogEntryDto.UserProfileId = currentUser.UserProfileId;
                    cromerrAuditLogEntryDto.UserName = currentUser.UserName;
                    cromerrAuditLogEntryDto.UserFirstName = currentUser.FirstName;
                    cromerrAuditLogEntryDto.UserLastName = currentUser.LastName;
                    cromerrAuditLogEntryDto.UserEmailAddress = currentUser.Email;
                    cromerrAuditLogEntryDto.IPAddress = _httpContextService.CurrentUserIPAddress();
                    cromerrAuditLogEntryDto.HostName = _httpContextService.CurrentUserHostName();

                    _crommerAuditLogService.Log(eventType:CromerrEvent.Report_Repudiated, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);

                    _dbContext.SaveChanges();

                    // Send emails to all IU signatories
                    var iuOrganizationRegulatoryProgram = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:reportPackage.OrganizationRegulatoryProgramId);
                    var signatories = _userService.GetOrgRegProgSignators(orgRegProgId:reportPackage.OrganizationRegulatoryProgramId).ToList();
                    var emailEntries = signatories
                        .Select(user => _linkoExchangeEmailService.GetEmailEntryForUser(user:user, emailType:EmailType.Report_Repudiation_IU,
                                                                                        contentReplacements:contentReplacements, orgRegProg:iuOrganizationRegulatoryProgram))
                        .ToList();

                    //System sends Report Repudiated Receipt to all Admin and Standard Users for the Authority (UC-19 8.4.) 
                    var authorityAdminAndStandardUsers = _userService
                        .GetAuthorityAdministratorAndStandardUsers(authorityOrganizationId:authorityOrganizationRegulatoryProgramDto.OrganizationId).ToList();
                    emailEntries.AddRange(collection:authorityAdminAndStandardUsers
                                              .Select(user => _linkoExchangeEmailService.GetEmailEntryForUser(user:user, emailType:EmailType.Report_Repudiation_AU,
                                                                                                              contentReplacements:contentReplacements,
                                                                                                              orgRegProg:authorityOrganizationRegulatoryProgramDto)));

                    // Do email audit log.
                    _linkoExchangeEmailService.WriteEmailAuditLogs(emailEntries:emailEntries);

                    transaction.Commit();

                    // Send emails.
                    _linkoExchangeEmailService.SendEmails(emailEntries:emailEntries);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            _logger.Info(message:$"End: ReportPackageService.RepudiateReport.");
        }

        /// <summary>
        ///     Meant to be called when user has reviewed a report submission. Updates the corresponding fields in the Report Package row.
        /// </summary>
        /// <param name="reportPackageId"> tReportPackage.ReportPackageId </param>
        /// <param name="comments"> Optional field </param>
        public void ReviewSubmission(int reportPackageId, string comments = null)
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));
            _logger.Info(message: $"Start: ReportPackageService.ReviewSubmission. reportPackageId={reportPackageId}, comments={comments}, "
                                  + $"currentOrgRegProgramId={currentOrgRegProgramId}, currentUserId={currentUserId}");

            //Comments are optional here (UC-10 6.)

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var user = _dbContext.Users.Single(u => u.UserProfileId == currentUserId);

                    var reportPackage = _dbContext.ReportPackages.Single(rep => rep.ReportPackageId == reportPackageId);

                    reportPackage.SubmissionReviewDateTimeUtc = DateTimeOffset.Now;
                    reportPackage.SubmissionReviewerUserId = currentUserId;
                    reportPackage.SubmissionReviewerFirstName = user.FirstName;
                    reportPackage.SubmissionReviewerLastName = user.LastName;
                    reportPackage.SubmissionReviewerTitleRole = user.TitleRole;

                    if (!string.IsNullOrEmpty(value:comments))
                    {
                        reportPackage.SubmissionReviewComments = comments;
                    }

                    _dbContext.SaveChanges();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            _logger.Info(message:$"End: ReportPackageService.ReviewSubmission.");
        }

        /// <summary>
        ///     Meant to be called when user has reviewed a report repudiation. Updates the corresponding fields in the Report Package row.
        /// </summary>
        /// <param name="reportPackageId"> tReportPackage.ReportPackageId </param>
        /// <param name="comments"> Required field </param>
        public void ReviewRepudiation(int reportPackageId, string comments)
        {
            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentUserId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.UserProfileId));
            _logger.Info(message: $"Start: ReportPackageService.ReviewRepudiation. reportPackageId={reportPackageId}, comments={comments}, "
                                  + $"currentOrgRegProgramId={currentOrgRegProgramId}, currentUserId={currentUserId}");

            if (string.IsNullOrEmpty(value:comments))
            {
                //UC-56 (6.a.) "Required"
                ThrowSimpleException(message:"Comments are required.");
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
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            _logger.Info(message:$"End: ReportPackageService.ReviewRepudiation.");
        }

        /// <summary>
        ///     Iterates through all required element types for a given report package where content is not provided and
        ///     ensures there is at least one "sample & results" or "file" associated with the report package
        ///     OF in the case where content is provided (ie. Certifications), we check that they have been "included".
        /// </summary>
        /// <param name="reportPackageId"> tReportPackage.ReportPackageId </param>
        private void CheckRequiredReportPackageElementTypesIncluded(int reportPackageId)
        {
            _logger.Info(message: $"Start: ReportPackageService.CheckRequiredReportPackageElementTypesIncluded. reportPackageId={reportPackageId}");

            var requiredReportPackageElementTypes = _dbContext.ReportPackageElementTypes
                                                              .Include(rpet => rpet.ReportPackageElementCategory.ReportElementCategory)
                                                              .Where(rpet => rpet.ReportPackageElementCategory.ReportPackageId == reportPackageId
                                                                             //&& !rpet.ReportElementTypeIsContentProvided
                                                                             && rpet.IsRequired)
                                                              .ToList();

            var isRequirementsMet = true;
            var failedRequirements = new List<RuleViolation>();
            foreach (var requiredRPET in requiredReportPackageElementTypes)
            {
                //LOGIC: if it's a certification (i.e. "content provided") and not accepted => VIOLATION
                //      OR it's not a certification and there are no samples or attachments
                if ((requiredRPET.ReportElementTypeIsContentProvided && !requiredRPET.IsIncluded)
                    || (!requiredRPET.ReportElementTypeIsContentProvided && !requiredRPET.ReportSamples.Any() && !requiredRPET.ReportFiles.Any()))
                {
                    failedRequirements.Add(new RuleViolation("", null, $"{requiredRPET.ReportElementTypeName} is required."));
                    isRequirementsMet = false;
                }
            }

            if (!isRequirementsMet)
            {
                throw new RuleViolationException(message: "Validation errors", validationIssues: failedRequirements);
            }

            _logger.Info(message: $"End: ReportPackageService.CheckRequiredReportPackageElementTypesIncluded. isIncluded={isRequirementsMet}");

        }

        /// <summary>
        ///     Updates the LastSentDateTimeUtc value of a row in table tReportPackage.
        ///     Also optionally updates the "last sender" details.
        /// </summary>
        /// <param name="reportPackageId"> tReportPackage.ReportPackageId </param>
        /// <param name="sentDateTime"> eg. DateTimeOffset.Now </param>
        /// <param name="lastSenderUserId"> Optional: tUserProfile.UserProfileId </param>
        /// <param name="lastSenderFirstName"> Optional: tUserProfile.FirstName </param>
        /// <param name="lastSenderLastName"> Optional: tUserProfile.LastName </param>
        public void UpdateLastSentDateTime(int reportPackageId, DateTimeOffset sentDateTime, int? lastSenderUserId = null, string lastSenderFirstName = null,
                                           string lastSenderLastName = null)
        {
            _logger.Info(message: $"Start: ReportPackageService.UpdateLastSentDateTime. reportPackageId={reportPackageId}, "
                                  + $"lastSenderUserId={lastSenderUserId?.ToString() ?? "null"}");

            var reportPackage = _dbContext.ReportPackages
                                          .SingleOrDefault(rp => rp.ReportPackageId == reportPackageId);

            if (reportPackage == null)
            {
                throw new Exception(message:$"ERROR: Could not find Report Package associated with reportPackageId={reportPackageId}");
            }

            reportPackage.LastSentDateTimeUtc = sentDateTime;

            if (lastSenderUserId.HasValue)
            {
                reportPackage.LastSenderUserId = lastSenderUserId;
            }

            if (!string.IsNullOrWhiteSpace(value:lastSenderFirstName))
            {
                reportPackage.LastSenderFirstName = lastSenderFirstName;
            }

            if (!string.IsNullOrWhiteSpace(value:lastSenderLastName))
            {
                reportPackage.LastSenderLastName = lastSenderLastName;
            }

            _dbContext.SaveChanges();

            _logger.Info(message:$"End: ReportPackageService.UpdateLastSentDateTime.");
        }

        /// <summary>
        ///     Identifies if a newer Report Package (same industry, template name and same Reporting Period)
        ///     exists with a newer SubmissionDateTimeUtc.
        /// </summary>
        /// <param name="reportPackageId"> tReportPackage.ReportPackageId of the ReportPackage we want to compare the submission date/time with </param>
        /// <returns> True if a Report Package with a later submission date/time exists </returns>
        public bool IsSimilarReportPackageSubmittedAfter(int reportPackageId)
        {
            _logger.Info(message:$"Start: ReportPackageService.IsSimilarReportPackageSubmittedAfter. reportPackageId={reportPackageId}");

            var reportPackageToCompare = _dbContext.ReportPackages
                                                   .SingleOrDefault(rp => rp.ReportPackageId == reportPackageId);

            if (reportPackageToCompare == null)
            {
                throw new Exception(message:$"ERROR: Could not find Report Package associated with reportPackageId={reportPackageId}");
            }

            var startDate = reportPackageToCompare.PeriodStartDateTimeUtc.Date;
            var endDate = reportPackageToCompare.PeriodEndDateTimeUtc.Date;

            var isNewerReportPackageExist = _dbContext.ReportPackages
                                                       .Any(rp => rp.OrganizationRegulatoryProgramId == reportPackageToCompare.OrganizationRegulatoryProgramId
                                                                  && rp.ReportPackageTemplateId == reportPackageToCompare.ReportPackageTemplateId
                                                                  && rp.PeriodStartDateTimeUtc.Year == startDate.Year
                                                                  && rp.PeriodStartDateTimeUtc.Month == startDate.Month
                                                                  && rp.PeriodStartDateTimeUtc.Day == startDate.Day
                                                                  && rp.PeriodEndDateTimeUtc.Year == endDate.Year
                                                                  && rp.PeriodEndDateTimeUtc.Month == endDate.Month
                                                                  && rp.PeriodEndDateTimeUtc.Day == endDate.Day
                                                                  && rp.SubmissionDateTimeUtc > reportPackageToCompare.SubmissionDateTimeUtc);

            _logger.Info(message:$"End: ReportPackageService.IsSimilarReportPackageSubmittedAfter. isNewerReportPackageExist={isNewerReportPackageExist}");

            return isNewerReportPackageExist;
        }

        public bool CanRepudiateReportPackage(int reportPackageId)
        {
            bool canRepudiate;
            var currentOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            _logger.Info(message:$"Start: ReportPackageService.CanRepudiateReportPackage. reportPackageId={reportPackageId}, currentOrgRegProgramId={currentOrgRegProgramId}");

            //Check ARP config "Max days after report period end date to repudiate" has not passed (UC-19 5.2.)
            var reportRepudiatedDays =
                Convert.ToInt32(value:_settingService.GetOrgRegProgramSettingValue(orgRegProgramId:currentOrgRegProgramId, settingType:SettingType.ReportRepudiatedDays));

            var reportPackage = _dbContext.ReportPackages.Single(rep => rep.ReportPackageId == reportPackageId);

            if (reportPackage.PeriodEndDateTimeUtc.UtcDateTime < DateTime.UtcNow.AddDays(value:-reportRepudiatedDays))
            {
                canRepudiate = false;
            }
            else
            {
                canRepudiate = true;
            }

            _logger.Info(message: $"End: ReportPackageService.CanRepudiateReportPackage. canRepudiate={canRepudiate}");

            return canRepudiate;
        }

        #endregion

        private CopyOfRecordDto CreateCopyOfRecordForReportPackage(ReportPackageDto reportPackageDto)
        {
            _logger.Info(message:$"Start: ReportPackageService.CreateCopyOfRecordForReportPackage. reportPackageId={reportPackageDto.ReportPackageId}");
            var reportPackageId = reportPackageDto.ReportPackageId;

            var attachments = new List<FileStoreDto>();
            foreach (var attachmentReportPackageElementType in reportPackageDto.AttachmentTypes)
            {
                foreach (var fileStore in attachmentReportPackageElementType.FileStores)
                {
                    attachments.Add(item:fileStore);
                }
            }

            var copyOfRecordPdfFile = GetReportPackageCopyOfRecordPdfFile(reportPackageDto:reportPackageDto);
            var copyOfRecordDataXmlFile = GetReportPackageCopyOfRecordDataXmlFile(reportPackageDto:reportPackageDto);
            var copyOfRecordDto = _copyOfRecordService.CreateCopyOfRecordForReportPackage(reportPackageId:reportPackageId, attachments:attachments,
                                                                                          copyOfRecordPdfFileDto:copyOfRecordPdfFile,
                                                                                          copyOfRecordDataXmlFileDto:copyOfRecordDataXmlFile);

            _logger.Info(message:"End: ReportPackageService.CreateCopyOfRecordForReportPackage.");

            return copyOfRecordDto;
        }

        private void WriteCrommerrLog(ReportPackageDto reportPackageDto, string submitterIpAddress, CopyOfRecordDto copyOfRecordDto)
        {
            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                          {
                                              RegulatoryProgramId = reportPackageDto.OrganizationRegulatoryProgramDto.RegulatoryProgramId,
                                              OrganizationId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationId))
                                          };
            var autorityOrg = _orgService.GetAuthority(orgRegProgramId:reportPackageDto.OrganizationRegulatoryProgramId);
            cromerrAuditLogEntryDto.RegulatorOrganizationId = autorityOrg.OrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = reportPackageDto.SubmitterUserId;
            cromerrAuditLogEntryDto.UserName = reportPackageDto.SubmitterUserName;
            cromerrAuditLogEntryDto.UserFirstName = reportPackageDto.SubmitterFirstName;
            cromerrAuditLogEntryDto.UserLastName = reportPackageDto.SubmitterLastName;
            cromerrAuditLogEntryDto.UserEmailAddress = _httpContextService.GetClaimValue(claimType:CacheKey.Email);

            cromerrAuditLogEntryDto.IPAddress = submitterIpAddress;
            cromerrAuditLogEntryDto.HostName = _httpContextService.CurrentUserHostName();

            var crommerContentReplacements = new Dictionary<string, string>
                                             {
                                                 {"organizationName", reportPackageDto.OrganizationName},
                                                 {"reportPackageName", reportPackageDto.Name}
                                             };

            var dateTimeFormat = "MM/dd/yyyy";
            crommerContentReplacements.Add(key:"periodStartDate", value:reportPackageDto.PeriodStartDateTimeLocal.ToString(format:dateTimeFormat));
            crommerContentReplacements.Add(key:"periodEndDate", value:reportPackageDto.PeriodEndDateTimeLocal.ToString(format:dateTimeFormat));
            crommerContentReplacements.Add(key:"corSignature", value:copyOfRecordDto.Signature);

            crommerContentReplacements.Add(key:"firstName", value:reportPackageDto.SubmitterFirstName);
            crommerContentReplacements.Add(key:"lastName", value:reportPackageDto.SubmitterLastName);
            crommerContentReplacements.Add(key:"userName", value:reportPackageDto.SubmitterUserName);
            crommerContentReplacements.Add(key:"emailAddress", value:cromerrAuditLogEntryDto.UserEmailAddress);

            _crommerAuditLogService.Log(eventType:CromerrEvent.Report_Submitted, dto:cromerrAuditLogEntryDto, contentReplacements:crommerContentReplacements);
        }

        private List<EmailEntry> GetSignAndSubmitEmailEntries(ReportPackageDto reportPackage, CopyOfRecordDto copyOfRecordDto)
        {
            _logger.Info(message:$"Start: ReportPackageService.GetSignAndSubmitEmailEntries. reportPackageId={reportPackage.ReportPackageId}");

            var emailContentReplacements = new Dictionary<string, string>
                                           {
                                               {"iuOrganizationName", reportPackage.OrganizationName},
                                               {"reportPackageName", reportPackage.Name},
                                               {"periodStartDate", reportPackage.PeriodStartDateTimeLocal.ToString(format:"MMM dd, yyyy")},
                                               {"periodEndDate", reportPackage.PeriodEndDateTimeLocal.ToString(format:"MMM dd, yyyy")}
                                           };



            var timeZoneNameAbbreviation =
                _timeZoneService.GetTimeZoneNameUsingSettingForThisOrg(orgRegProgramId:reportPackage.OrganizationRegulatoryProgramId,
                                                                       dateTime:reportPackage.SubmissionDateTimeLocal.Value, abbreviationName:true);
            var submissionDateTime =
                $"{reportPackage.SubmissionDateTimeLocal.Value.ToString(format:"MMM dd, yyyy h:mmtt ")}{timeZoneNameAbbreviation}";

            emailContentReplacements.Add(key:"submissionDateTime", value:submissionDateTime);
            emailContentReplacements.Add(key:"corSignature", value:copyOfRecordDto.Signature);
            emailContentReplacements.Add(key:"submitterFirstName", value:_httpContextService.GetClaimValue(claimType:CacheKey.FirstName));
            emailContentReplacements.Add(key:"submitterLastName", value:_httpContextService.GetClaimValue(claimType:CacheKey.LastName));
            emailContentReplacements.Add(key:"submitterTitle", value:reportPackage.SubmitterTitleRole.GetValueOrEmptyString());

            emailContentReplacements.Add(key:"permitNumber", value:reportPackage.OrganizationReferenceNumber);

            emailContentReplacements.Add(key:"organizationAddressLine1", value:reportPackage.OrganizationAddressLine1);
            var organizationAddressLine2 = "";
            if (!string.IsNullOrWhiteSpace(value:reportPackage.OrganizationAddressLine2))
            {
                organizationAddressLine2 = $"{Environment.NewLine}\t{reportPackage.OrganizationAddressLine2}";
            }

            emailContentReplacements.Add(key:"organizationAddressLine2", value:organizationAddressLine2);

            emailContentReplacements.Add(key:"organizationCityName", value:reportPackage.OrganizationCityName);

            var juridicationName = "";
            if (!string.IsNullOrWhiteSpace(value:reportPackage.OrganizationJurisdictionName))
            {
                juridicationName = reportPackage.OrganizationJurisdictionName;
            }
            emailContentReplacements.Add(key:"organizationJurisdictionName", value:juridicationName);

            var zipCode = "";
            if (!string.IsNullOrWhiteSpace(value:reportPackage.OrganizationZipCode))
            {
                zipCode = reportPackage.OrganizationZipCode;
            }
            emailContentReplacements.Add(key:"organizationZipCode", value:zipCode);

            emailContentReplacements.Add(key:"userName", value:_httpContextService.GetClaimValue(claimType:CacheKey.UserName));

            emailContentReplacements.Add(key:"recipientOrganizationName", value:reportPackage.RecipientOrganizationName);
            emailContentReplacements.Add(key:"recipientOrganizationAddressLine1", value:reportPackage.RecipientOrganizationAddressLine1);

            var recipientOrganizationAddressLine2 = "";
            if (!string.IsNullOrWhiteSpace(value:reportPackage.RecipientOrganizationAddressLine2))
            {
                recipientOrganizationAddressLine2 = $"{Environment.NewLine}\t{reportPackage.RecipientOrganizationAddressLine2}";
            }

            emailContentReplacements.Add(key:"recipientOrganizationAddressLine2", value:recipientOrganizationAddressLine2);
            emailContentReplacements.Add(key:"recipientOrganizationCityName", value:reportPackage.RecipientOrganizationCityName);
            emailContentReplacements.Add(key:"recipientOrganizationJurisdictionName", value:reportPackage.RecipientOrganizationJurisdictionName);
            emailContentReplacements.Add(key:"recipientOrganizationZipCode", value:reportPackage.RecipientOrganizationZipCode);

            var contactUserNameOnEmail = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:reportPackage.OrganizationRegulatoryProgramDto.OrganizationRegulatoryProgramId,
                                                                                      settingType:SettingType.EmailContactInfoName);
            var emailAddressOnEmail = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:reportPackage.OrganizationRegulatoryProgramDto.OrganizationRegulatoryProgramId,
                                                                                   settingType:SettingType.EmailContactInfoEmailAddress);
            var phoneNumberOnEmail = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:reportPackage.OrganizationRegulatoryProgramDto.OrganizationRegulatoryProgramId,
                                                                                  settingType:SettingType.EmailContactInfoPhone);

            emailContentReplacements.Add(key:"authorityName", value:contactUserNameOnEmail);
            emailContentReplacements.Add(key:"authoritySupportEmail", value:emailAddressOnEmail);
            emailContentReplacements.Add(key:"authoritySupportPhoneNumber", value:phoneNumberOnEmail);

            emailContentReplacements.Add(key:"corViewLink", value:$"{_httpContextService.GetRequestBaseUrl()}reportPackage/{reportPackage.ReportPackageId}/Details");

            // Send emails to all IU signatories 
            var iuOrganizationRegulatoryProgram = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:reportPackage.OrganizationRegulatoryProgramId);
            var signatories = _userService.GetOrgRegProgSignators(orgRegProgId:reportPackage.OrganizationRegulatoryProgramId).ToList();
            var emailEntries = signatories
                .Select(user => _linkoExchangeEmailService.GetEmailEntryForUser(user:user, emailType:EmailType.Report_Submission_IU, contentReplacements:emailContentReplacements,
                                                                                orgRegProg:iuOrganizationRegulatoryProgram))
                .ToList();

            // Send emails to all Standard Users for the authority
            var authorityOrganizationRegulatoryProgramDto = _organizationService.GetAuthority(orgRegProgramId:reportPackage.OrganizationRegulatoryProgramId);
            var authorityAdminAndStandardUsers = _userService
                .GetAuthorityAdministratorAndStandardUsers(authorityOrganizationId:authorityOrganizationRegulatoryProgramDto.OrganizationId).ToList();

            emailEntries.AddRange(collection:authorityAdminAndStandardUsers
                                      .Select(user => _linkoExchangeEmailService.GetEmailEntryForUser(user:user, emailType:EmailType.Report_Submission_AU,
                                                                                                      contentReplacements:emailContentReplacements,
                                                                                                      orgRegProg:authorityOrganizationRegulatoryProgramDto)));
            
            _logger.Info(message:"End: ReportPackageService.GetSignAndSubmitEmailEntries.");
            return emailEntries;
        }

        private string GenerateXmlString(CopyOfRecordDataXml dataXmlObj, ReportPackageDto reportPackageDto)
        {
            var strWriter = new Utf8StringWriter();
            var xmlSerializer = new XmlSerializer(type:dataXmlObj.GetType());
            xmlSerializer.Serialize(textWriter:strWriter, o:dataXmlObj);
            var xmlString = strWriter.ToString();

            var xmlDoc = XDocument.Parse(text:xmlString);
            var elements = xmlDoc.Root.Elements().ToList();

            var certifications = GetXElementNode(corElements:elements, tagName:ReportElementCategoryName.Certifications.ToString());
            var samples = GetXElementNode(corElements:elements, tagName:"Samples");
            var attachments = GetXElementNode(corElements:elements, tagName:"FileManifest");
            var comment = GetXElementNode(corElements:elements, tagName:"Comment");

            var certificationsCloned = TakeOffXElementNode(node:certifications);
            var samplesCloned = TakeOffXElementNode(node:samples);
            var attachmentsCloned = TakeOffXElementNode(node:attachments);
            var commentCloned = TakeOffXElementNode(node:comment);

            if(comment != null && !reportPackageDto.ReportPackageElementCategories.Contains(item:ReportElementCategoryName.SamplesAndResults))
            {
                xmlDoc.Root.Add(content:commentCloned);
            }

            foreach (var categoryName in reportPackageDto.ReportPackageElementCategories)
            {
                switch (categoryName)
                {
                    case ReportElementCategoryName.Attachments:
                        if (attachments != null)
                        {
                            xmlDoc.Root.Add(content:attachmentsCloned);
                        }
                        break;
                    case ReportElementCategoryName.Certifications:
                        if (certifications != null)
                        {
                            xmlDoc.Root.Add(content:certificationsCloned);
                        }
                        break;
                    case ReportElementCategoryName.SamplesAndResults:
                        if (samplesCloned != null)
                        {
                            xmlDoc.Root.Add(content:samplesCloned);
                        }
                        if (comment != null)
                        {
                            xmlDoc.Root.Add(content:commentCloned);
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
                var nodeCloned = new XElement(other:node);
                node.Remove();
                return nodeCloned;
            }

            return null;
        }

        private XElement GetXElementNode(IEnumerable<XElement> corElements, string tagName)
        {
            var xElements = corElements as XElement[] ?? corElements.ToArray();
            if (xElements.Any(i => i.Name.LocalName == tagName))
            {
                return xElements.Single(i => i.Name.LocalName == tagName);
            }

            return null;
        }

        private ReportStatusName GetReportStatusName(ReportPackage reportPackage)
        {
            var reportStatusName = _dbContext.ReportStatuses
                                             .Single(rs => rs.ReportStatusId == reportPackage.ReportStatusId).Name;

            return (ReportStatusName) Enum.Parse(enumType:typeof(ReportStatusName), value:reportStatusName);
        }

        /// <summary>
        ///     Helper function used by both "GetReportPackagesByStatusName" and "GetReportPackage" methods
        /// </summary>
        /// <param name="reportPackage"> </param>
        /// <param name="timeZoneId"> Authority's local timezone </param>
        /// <returns> Mapped ReportPackageDto without children element data </returns>
        private ReportPackageDto GetReportPackageDtoFromReportPackage(ReportPackage reportPackage, int timeZoneId)
        {
            var reportPackagegDto = _mapHelper.GetReportPackageDtoFromReportPackage(rpt:reportPackage);

            reportPackagegDto.ReportStatusName = GetReportStatusName(reportPackage:reportPackage);

            reportPackagegDto.OrganizationRegulatoryProgramDto =
                _programService.GetOrganizationRegulatoryProgram(organizationRegulatoryProgramId:reportPackage.OrganizationRegulatoryProgramId);

            if (reportPackagegDto.OrganizationRegulatoryProgramDto.RegulatorOrganizationId.HasValue)
            {
                reportPackagegDto.RecipientOrganizationRegulatoryProgramId = _dbContext.OrganizationRegulatoryPrograms
                                                                                       .Single(orp => orp.OrganizationId
                                                                                                      == reportPackagegDto.OrganizationRegulatoryProgramDto.RegulatorOrganizationId
                                                                                                      && orp.RegulatoryProgramId
                                                                                                      == reportPackagegDto.OrganizationRegulatoryProgramDto.RegulatoryProgramId)
                                                                                       .OrganizationRegulatoryProgramId;
            }

            if (reportPackage.SubmissionDateTimeUtc.HasValue)
            {
                reportPackagegDto.SubmissionDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.SubmissionDateTimeUtc.Value.UtcDateTime, timeZoneId:timeZoneId);
            }

            if (reportPackage.SubmissionReviewDateTimeUtc.HasValue)
            {
                reportPackagegDto.SubmissionReviewDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.SubmissionReviewDateTimeUtc.Value.UtcDateTime, timeZoneId:timeZoneId);
            }

            if (reportPackage.RepudiationDateTimeUtc.HasValue)
            {
                reportPackagegDto.RepudiationDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.RepudiationDateTimeUtc.Value.UtcDateTime, timeZoneId:timeZoneId);
            }

            if (reportPackage.RepudiationReviewDateTimeUtc.HasValue)
            {
                reportPackagegDto.RepudiationReviewDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.RepudiationReviewDateTimeUtc.Value.UtcDateTime, timeZoneId:timeZoneId);
            }

            if (reportPackage.LastSentDateTimeUtc.HasValue)
            {
                reportPackagegDto.LastSentDateTimeLocal = _timeZoneService
                    .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.LastSentDateTimeUtc.Value.UtcDateTime, timeZoneId:timeZoneId);
            }

            reportPackagegDto.PeriodEndDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.PeriodEndDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);

            reportPackagegDto.PeriodStartDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.PeriodStartDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);

            reportPackagegDto.CreationDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.CreationDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);

            reportPackagegDto.LastModificationDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(utcDateTime:reportPackage.LastModificationDateTimeUtc.HasValue
                                                                         ? reportPackage.LastModificationDateTimeUtc.Value.UtcDateTime
                                                                         : reportPackage.CreationDateTimeUtc.UtcDateTime, timeZoneId:timeZoneId);
            if (reportPackage.LastModifierUserId != null)
            {
                var lastModifier = _dbContext.Users
                                             .Single(u => u.UserProfileId == reportPackage.LastModifierUserId);
                reportPackagegDto.LastModifierFullName = $"{lastModifier.FirstName} {lastModifier.LastName}";
            }

            return reportPackagegDto;
        }

        /// <summary>
        ///     Used to simplify and clean up methods where there are multiple validation tests.
        /// </summary>
        /// <param name="message"> Rule violation message to use when throwing the exception. </param>
        /// <param name="propertyName"> </param>
        private void ThrowSimpleException(string message, string propertyName = null)
        {
            _logger.Info(message:$"Start: ReportPackageService.ThrowSimpleException. message={message}");

            var validationIssues = new List<RuleViolation>
                                   {
                                       new RuleViolation(propertyName:propertyName ?? string.Empty, propertyValue:null, errorMessage:message)
                                   };

            _logger.Info(message:"End: ReportPackageService.ThrowSimpleException.");

            throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
        }

        private class Utf8StringWriter : StringWriter
        {
            #region public properties

            public override Encoding Encoding => Encoding.UTF8;

            #endregion
        }
    }
}