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

namespace Linko.LinkoExchange.Services.Report
{
    public class ReportPackageService : IReportPackageService
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
            ICromerrAuditLogService crommerAuditLogService
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
        }


        /// <summary>
        /// Note:  before call this function,  make sure to update draf first.
        /// </summary>
        /// <param name="reportPackageId"></param>
        public void SignAndSubmitReportPackage(int reportPackageId)
        {
            _logger.Info("Enter ReportPackageService.SignAndSubmitReportPackage. reportPackageId={0}", reportPackageId);

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    List<RuleViolation> validationIssues = new List<RuleViolation>();

                    var reportPackage = _dbContext.ReportPackages.Include(i => i.ReportStatus)
                        .Single(i => i.ReportPackageId == reportPackageId);

                    if (reportPackage.ReportStatus.Name != ReportStatusName.ReadyToSubmit.ToString())
                    {
                        string message = "Report Package is not ready to submit.";
                        validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                        throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
                    }

                    var submitterUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
                    var submitterFirstName = _httpContextService.GetClaimValue(CacheKey.FirstName);
                    var submitterLastName = _httpContextService.GetClaimValue(CacheKey.LastName);
                    var submitterTitleRole = _httpContextService.GetClaimValue(CacheKey.UserRole);
                    var submitterIpAddress = _httpContextService.CurrentUserIPAddress();
                    var submitterUserName = _httpContextService.GetClaimValue(CacheKey.UserName);

                    reportPackage.SubmissionDateTimeUtc = DateTimeOffset.Now;
                    reportPackage.SubmitterUserId = submitterUserId;
                    reportPackage.SubmitterFirstName = submitterFirstName;
                    reportPackage.SubmitterLastName = submitterLastName;
                    reportPackage.SubmitterTitleRole = submitterTitleRole;
                    reportPackage.SubmitterIPAddress = submitterIpAddress;
                    reportPackage.SubmitterUserName = submitterUserName;

                    UpdateStatus(reportPackageId, ReportStatusName.Submitted, false);
                    _dbContext.SaveChanges();

                    var reportPackageDto = GetReportPackage(reportPackageId, true);
                    var copyOfRecordDto = CreateCopyOfRecordForReportPackage(reportPackageDto);

                    //// Send emails 
                    SendSignAndSubmitEmail(reportPackageDto, copyOfRecordDto);

                    // Add for crommer log
                    WriteCrommerrLog(reportPackageDto, submitterIpAddress, copyOfRecordDto);

                    _dbContext.SaveChanges();
                    transaction.Commit();
                    _logger.Info("Leave ReportPackageService.SignAndSubmitReportPackage. reportPackageId={0}", reportPackageId);
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
        }

        public CopyOfRecordPdfFileDto GetReportPackageCopyOfRecordPdfFile(ReportPackageDto reportPackageDto)
        {
            throw new NotImplementedException();
        }

        public CopyOfRecordDataXmlFileDto GetReportPackageCopyOfRecordDataXmlFile(ReportPackageDto reportPackageDto)
        {
            _logger.Info("Enter ReportPackageService.CopyOfRecordDataXmlFileDto. reportPackageId={0}", reportPackageDto.ReportPackageId);

            var dateTimeFormat = "MM/dd/yyyyThh:mm:ss zzzz";
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
                    Address1 = reportPackageDto.RecipientOrganizationAddressLine1,
                    Address2 = EmtpyStringIfNull(reportPackageDto.RecipientOrganizationAddressLine2),
                    City = reportPackageDto.RecipientOrganizationCityName,
                    State = reportPackageDto.RecipientOrganizationJurisdictionName,
                    ZipCode = reportPackageDto.RecipientOrganizationZipCode
                },
                SubmittedOnBehalfOf = new SubmittedOnBehalfOf
                {
                    OrganizationName = reportPackageDto.OrganizationName,
                    ReferenceNumber = reportPackageDto.OrganizationRegulatoryProgramDto.ReferenceNumber,
                    Address1 = reportPackageDto.RecipientOrganizationAddressLine1,
                    Address2 = EmtpyStringIfNull(reportPackageDto.RecipientOrganizationAddressLine2),
                    City = reportPackageDto.RecipientOrganizationCityName,
                    State = reportPackageDto.RecipientOrganizationJurisdictionName,
                    ZipCode = reportPackageDto.RecipientOrganizationZipCode
                },
                SubmittedBy = new SubmittedBy
                {
                    FirstName = EmtpyStringIfNull(reportPackageDto.SubmitterFirstName),
                    LastName = EmtpyStringIfNull(reportPackageDto.SubmitterLastName),
                    Title = EmtpyStringIfNull(reportPackageDto.SubmitterTitleRole),
                    UserName = EmtpyStringIfNull(reportPackageDto.SubmitterUserName),
                    ReportSubmissionFromIP = EmtpyStringIfNull(reportPackageDto.SubmitterIPAddress)
                },
                FileManifest = new FileManifest
                {
                    Files = reportPackageDto.AssociatedFiles.Select(af => af.FileStore).Select(i => new FileInfo
                    {
                        OriginalFileName = i.OriginalFileName,
                        SystemGeneratedUniqueFileName = i.Name,
                        AttachmentType = i.FileType
                    }).ToList()
                }
            };

            dataXmlObj.FileManifest.Files.Add(
                new FileInfo
                {
                    OriginalFileName = "Copy Of Record Data.xml",
                    SystemGeneratedUniqueFileName = "Copy Of Record Data.xml",
                    AttachmentType = "Xml Raw Data"
                });

            dataXmlObj.FileManifest.Files.Add(
                new FileInfo
                {
                    OriginalFileName = "Copy Of Record.pdf",
                    SystemGeneratedUniqueFileName = "Copy Of Record.pdf",
                    AttachmentType = "Copy Of Record PDF"
                });

            dataXmlObj.Certifications = new List<Certification>();
            dataXmlObj.Certifications = reportPackageDto.CertificationDtos.Select(i => new Certification
            {
                CertificationType = i.ReportElementTypeName,
                CertificationText = string.IsNullOrWhiteSpace(i.ReportElementTypeContent) ? "" : i.ReportElementTypeContent
            }).ToList();

            dataXmlObj.Comment = reportPackageDto.Comments;

            // SampleResults part 
            dataXmlObj.Samples = new List<SampleNode>();
            foreach (var sampleAssociation in reportPackageDto.AssociatedSamples)
            {
                var sampleDto = sampleAssociation.Sample;
                var sampleNode = new SampleNode
                {
                    SampleName = sampleDto.Name,
                    MonitoringPointName = sampleDto.MonitoringPointName,
                    CtsEventTypeCategoryName = sampleDto.CtsEventCategoryName,
                    CtsEventTypeName = sampleDto.CtsEventTypeName,
                    CollectionMethodName = sampleDto.CollectionMethodName,
                    LabSampleIdentifier = EmtpyStringIfNull(sampleDto.LabSampleIdentifier),

                    StartDateTimeUtc = sampleDto.StartDateTimeLocal.ToString(dateTimeFormat),
                    EndDateTimeUtc = sampleDto.EndDateTimeLocal.ToString(dateTimeFormat),

                    SampleFlowForMassCalcs = EmtpyStringIfNull(sampleDto.FlowValue),
                    SampleFlowForMassCalcsUnitName = EmtpyStringIfNull(sampleDto.FlowUnitName),

                    MassLoadingsConversionFactorPounds = sampleDto.MassLoadingConversionFactorPounds?.ToString(),
                    MassLoadingCalculationDecimalPlaces = sampleDto.MassLoadingCalculationDecimalPlaces?.ToString(),
                    IsMassLoadingResultToUseLessThanSign = sampleDto.IsMassLoadingResultToUseLessThanSign.ToString(),

                    SampledBy = sampleDto.ByOrganizationTypeName,
                    SampleResults = new List<SampleResultNode>(),
                };

                dataXmlObj.Samples.Add(sampleNode);

                foreach (var sampleResultDto in sampleDto.SampleResults)
                {
                    var sampleResultValue = "";
                    var limitBasisValue = "";

                    if (string.IsNullOrEmpty(sampleResultDto.MassLoadingValue))
                    {
                        limitBasisValue = LimitBasisName.Concentration.ToString();
                        sampleResultValue = sampleResultDto.Value;
                    }
                    else
                    {
                        limitBasisValue = LimitBasisName.MassLoading.ToString();
                        sampleResultValue = sampleResultDto.MassLoadingValue;
                    }

                    var analysisDateTime = "";
                    if (sampleResultDto.AnalysisDateTimeLocal.HasValue)
                    {
                        analysisDateTime = sampleResultDto.AnalysisDateTimeLocal.Value.ToString(dateTimeFormat);
                    }

                    var sampleResultNode = new SampleResultNode
                    {
                        ParameterName = sampleResultDto.ParameterName,
                        Qualifier = System.Net.WebUtility.HtmlEncode(sampleResultDto.Qualifier),
                        Value = sampleResultValue,
                        UnitName = sampleResultDto.UnitName,
                        EnteredMethodDetectionLimit = sampleResultDto.EnteredMethodDetectionLimit,
                        MethodDetectionLimit = sampleResultDto.MethodDetectionLimit.ToString(),
                        AnalysisMethod = sampleResultDto.AnalysisMethod,
                        AnalysisDateTimeUtc = analysisDateTime,
                        IsApprovedEPAMethod = sampleResultDto.IsApprovedEPAMethod.ToString(),
                        LimitBasis = limitBasisValue
                    };

                    sampleNode.SampleResults.Add(sampleResultNode);
                }
            }

            var strWriter = new Utf8StringWriter();
            var xmlSerializer = new XmlSerializer(dataXmlObj.GetType());
            xmlSerializer.Serialize(strWriter, dataXmlObj);
            var xmlString = strWriter.ToString();

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

        public ReportPackageDto GetReportPackage(int reportPackageId, bool isIncludeAssociatedElementData)
        {
            _logger.Info("Enter ReportPackageService.GetReportPackage. reportPackageId={0}", reportPackageId);

            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));

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

            if (samplesReportPackageElementCategory == null)
            {
                //throw Exception
                throw new Exception($"ERROR: Missing ReportPackageElementCategory associated with '{ReportElementCategoryName.SamplesAndResults}', reportPackageId={reportPackageId}");
            }

            reportPackagegDto.AssociatedSamples = new List<ReportSampleDto>();
            foreach (var existingSamplesReportPackageElementType in samplesReportPackageElementCategory.ReportPackageElementTypes)
            {
                //Should just be one iteration through this loop for the current phase, but in the future
                //we might have more than one "Sample and Results" section in a Report Package

                foreach (var reportSampleAssociated in existingSamplesReportPackageElementType.ReportSamples)
                {
                    var reportSampleDto = new ReportSampleDto()
                    {
                        ReportSampleId = reportSampleAssociated.ReportSampleId,
                        ReportPackageElementTypeId = reportSampleAssociated.ReportPackageElementTypeId,
                        SampleId = reportSampleAssociated.SampleId
                    };
                    if (isIncludeAssociatedElementData)
                    {
                        reportSampleDto.Sample = _sampleService.GetSampleDetails(reportSampleAssociated.SampleId);
                    }
                    reportPackagegDto.AssociatedSamples.Add(reportSampleDto);

                }
            }

            //
            //ADD FILE ASSOCIATIONS (AND OPTIONALLY FILE DATA)
            //

            //Find entry in tReportPackageElementType for this reportPackage associated with Samples category
            var filesReportPackageElementCategory = reportPackage.ReportPackageElementCategories
                .SingleOrDefault(rpet => rpet.ReportElementCategory.Name == ReportElementCategoryName.Attachments.ToString());

            if (filesReportPackageElementCategory == null)
            {
                //throw Exception
                throw new Exception($"ERROR: Missing ReportPackageElementCategory associated with '{ReportElementCategoryName.Attachments}', reportPackageId={reportPackageId}");
            }

            reportPackagegDto.AssociatedFiles = new List<ReportFileDto>();
            foreach (var existingFilesReportPackageElementType in filesReportPackageElementCategory.ReportPackageElementTypes)
            {
                foreach (var reportFileAssociated in existingFilesReportPackageElementType.ReportFiles)
                {
                    var reportFileDto = new ReportFileDto()
                    {
                        ReportFileId = reportFileAssociated.ReportFileId,
                        ReportPackageElementTypeId = reportFileAssociated.ReportPackageElementTypeId,
                        FileStoreId = reportFileAssociated.FileStoreId
                    };
                    if (isIncludeAssociatedElementData)
                    {
                        reportFileDto.FileStore = _mapHelper.GetFileStoreDtoFromFileStore(_dbContext.FileStores.Single(s => s.FileStoreId == reportFileAssociated.FileStoreId));
                    }
                    reportPackagegDto.AssociatedFiles.Add(reportFileDto);

                }
            }
            _logger.Info("Leave ReportPackageService.GetReportPackage. reportPackageId={0}", reportPackageId);
            return reportPackagegDto;
        }

        public CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId, ReportPackageDto reportPackageDto = null)
        {
            _logger.Info("Enter ReportPackageService.GetCopyOfRecordByReportPackageId. reportPackageId={0}", reportPackageId);

            if (reportPackageDto == null)
            {
                reportPackageDto = GetReportPackage(reportPackageId, false);
            }

            var copyOfRecordDto = _copyOfRecordService.GetCopyOfRecordByReportPackage(reportPackageDto);
            _logger.Info("Leave ReportPackageService.GetCopyOfRecordByReportPackageId. reportPackageId={0}", reportPackageId);

            return copyOfRecordDto;
        }

        public CopyOfRecordDto CreateCopyOfRecordForReportPackage(ReportPackageDto reportPackageDto)
        {
            _logger.Info("Enter ReportPackageService.CreateCopyOfRecordForReportPackage. reportPackageId={0}", reportPackageDto.ReportPackageId);
            int reportPackageId = reportPackageDto.ReportPackageId;
            var attachments = reportPackageDto.AssociatedFiles.Select(af => af.FileStore);
            var copyOfRecordPdfFile = GetReportPackageCopyOfRecordPdfFile(reportPackageDto);
            var copyOfRecordDataXmlFile = GetReportPackageCopyOfRecordDataXmlFile(reportPackageDto);
            var copyOfRecordDto = _copyOfRecordService.CreateCopyOfRecordForReportPackage(reportPackageId, attachments, copyOfRecordPdfFile, copyOfRecordDataXmlFile);

            _logger.Info("Leave ReportPackageService.CreateCopyOfRecordForReportPackage. reportPackageId={0}", reportPackageId);

            return copyOfRecordDto;
        }

        private void WriteCrommerrLog(ReportPackageDto reportPackageDto, string submitterIpAddress, CopyOfRecordDto copyOfRecordDto)
        {
            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
            cromerrAuditLogEntryDto.RegulatoryProgramId = reportPackageDto.OrganizationRegulatoryProgramId;
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

            var dateTimeFormat = "MM/dd/yyyyThh:mm:ss zzzz";
            crommerContentReplacements.Add("periodStart", reportPackageDto.PeriodStartDateTimeLocal.ToString(dateTimeFormat));
            crommerContentReplacements.Add("periodEnd", reportPackageDto.PeriodEndDateTimeLocal.ToString(dateTimeFormat));
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

            emailContentReplacements.Add("iuOrganizationName", reportPackage.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName);
            emailContentReplacements.Add("reportPackageName", reportPackage.Name);

            emailContentReplacements.Add("periodStartDate", reportPackage.PeriodStartDateTimeLocal.ToString("MMM dd, yyyy"));
            emailContentReplacements.Add("periodEndDate", reportPackage.PeriodEndDateTimeLocal.ToString("MMM dd, yyyy"));

            var timeZoneNameAbbreviation = _timeZoneService.GetTimeZoneNameUsingSettingForThisOrg(reportPackage.OrganizationRegulatoryProgramId, reportPackage.SubmissionDateTimeLocal.Value, true);
            var submissionDateTime =
                $"{reportPackage.SubmissionDateTimeLocal.Value.ToString("MMM dd, yyyy HHtt ")}{timeZoneNameAbbreviation}";

            emailContentReplacements.Add("submissionDateTime", submissionDateTime);
            emailContentReplacements.Add("corSignature", copyOfRecordDto.Signature);
            emailContentReplacements.Add("submitterFirstName", _httpContextService.GetClaimValue(CacheKey.FirstName));
            emailContentReplacements.Add("submitterLastName", _httpContextService.GetClaimValue(CacheKey.LastName));
            emailContentReplacements.Add("submitterTitle", _httpContextService.GetClaimValue(CacheKey.UserRole));

            emailContentReplacements.Add("permitNumber", reportPackage.OrganizationRegulatoryProgramDto.ReferenceNumber);

            emailContentReplacements.Add("organizationAddressLine1", reportPackage.OrganizationRegulatoryProgramDto.OrganizationDto.AddressLine1);
            var organizationAddressLine2 = "";
            if (!string.IsNullOrWhiteSpace(reportPackage.OrganizationRegulatoryProgramDto.OrganizationDto.AddressLine2))
            {
                organizationAddressLine2 = reportPackage.OrganizationRegulatoryProgramDto.OrganizationDto.AddressLine2;
            }

            emailContentReplacements.Add("organizationAddressLine2", organizationAddressLine2);

            emailContentReplacements.Add("organizationCityName", reportPackage.OrganizationRegulatoryProgramDto.OrganizationDto.CityName);
            emailContentReplacements.Add("organizationJurisdictionName", reportPackage.OrganizationRegulatoryProgramDto.OrganizationDto.State);
            emailContentReplacements.Add("organizationZipCode", reportPackage.OrganizationRegulatoryProgramDto.OrganizationDto.ZipCode);

            emailContentReplacements.Add("userName", _httpContextService.GetClaimValue(CacheKey.UserName));

            emailContentReplacements.Add("recipientOrganizationName", reportPackage.RecipientOrganizationName);
            emailContentReplacements.Add("recipientOrganizationAddressLine1", reportPackage.RecipientOrganizationAddressLine1);

            var recipientOrganizationAddressLine2 = "";
            if (!string.IsNullOrWhiteSpace(reportPackage.RecipientOrganizationAddressLine2))
            {
                recipientOrganizationAddressLine2 = reportPackage.RecipientOrganizationAddressLine2;
            }

            emailContentReplacements.Add("recipientOrganizationAddressLine2", recipientOrganizationAddressLine2);
            emailContentReplacements.Add("recipientOrganizationCityName", reportPackage.RecipientOrganizationCityName);
            emailContentReplacements.Add("recipientOrganizationJurisdictionName", reportPackage.RecipientOrganizationJurisdictionName);
            emailContentReplacements.Add("recipientOrganizationZipCode", reportPackage.RecipientOrganizationZipCode);

            var emailAddressOnEmail = _settingService.GetOrgRegProgramSettingValue(reportPackage.OrganizationRegulatoryProgramDto.RegulatoryProgramId, SettingType.EmailContactInfoEmailAddress);
            var phoneNumberOnEmail = _settingService.GetOrgRegProgramSettingValue(reportPackage.OrganizationRegulatoryProgramDto.RegulatoryProgramId, SettingType.EmailContactInfoPhone);

            emailContentReplacements.Add("authorityName", reportPackage.RecipientOrganizationName);
            emailContentReplacements.Add("supportEmail", emailAddressOnEmail);
            emailContentReplacements.Add("supportPhoneNumber", phoneNumberOnEmail);

            emailContentReplacements.Add("corViewLink", $"/reportPackage/{reportPackage.ReportPackageId}/cor");

            // Send emails to all IU signatories 
            var signatoriesEmails = _userService.GetOrgRegProgSignators(reportPackage.OrganizationRegulatoryProgramId).Select(i => i.Email).ToList();
            _emailService.SendEmail(signatoriesEmails, EmailType.Report_Submission_IU, emailContentReplacements, false);

            // Send emails to all Standard Users for the authority  
            var authorityOrganzationId = reportPackage.OrganizationRegulatoryProgramDto.OrganizationId;
            var authorityAdminAndStandardUsersEmails = _userService.GetAuthorityAdministratorAndStandardUsers(authorityOrganzationId).Select(i => i.Email).ToList();
            _emailService.SendEmail(authorityAdminAndStandardUsersEmails, EmailType.Report_Submission_AU, emailContentReplacements, false);

            _logger.Info("Leave ReportPackageService.SendSignAndSubmitEmail. reportPackageId={0}", reportPackage.ReportPackageId);
        }

        /// <summary>
        /// *WARNING: NO VALIDATION CHECK -- CASCADE DELETE*
        /// Hard delete of row from tReportPackage table associated with passed in parameter.
        /// Programatically cascade deletes rows in the following associated tables:
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
                    var authorityOrganization = _orgService.GetAuthority(currentOrgRegProgramId);
                    var currentUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
                    var timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));

                    //Step 1 - copy fields from template to new Report Package instance (tReportPackage)

                    //Get template
                    var reportPackageTemplate = _dbContext.ReportPackageTempates
                        .Include(rpt => rpt.CtsEventType)
                        .Include(rpt => rpt.OrganizationRegulatoryProgram)
                        .Include(rpt => rpt.OrganizationRegulatoryProgram.Organization)
                        .Include(rpt => rpt.OrganizationRegulatoryProgram.Organization.Jurisdiction)
                        .Include(rpt => rpt.OrganizationRegulatoryProgram.RegulatorOrganization)
                        .Include(rpt => rpt.OrganizationRegulatoryProgram.RegulatorOrganization.Jurisdiction)
                        .Single(rpt => rpt.ReportPackageTemplateId == reportPackageTemplateId);

                    var newReportPackage = _mapHelper.GetReportPackageFromReportPackageTemplate(reportPackageTemplate);

                    var startDateTimeUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(startDateTimeLocal, timeZoneId);
                    var endDateTimeUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(endDateTimeLocal, timeZoneId);
                    newReportPackage.PeriodStartDateTimeUtc = startDateTimeUtc;
                    newReportPackage.PeriodEndDateTimeUtc = endDateTimeUtc;
                    newReportPackage.ReportStatusId = _dbContext.ReportStatuses
                        .Single(rs => rs.Name == ReportStatusName.Draft.ToString()).ReportStatusId;
                    newReportPackage.CreationDateTimeUtc = DateTime.UtcNow;

                    //Need to populate with Authority fields
                    newReportPackage.RecipientOrganizationName = authorityOrganization.OrganizationDto.OrganizationName;
                    newReportPackage.RecipientOrganizationAddressLine1 = authorityOrganization.OrganizationDto.AddressLine1;
                    newReportPackage.RecipientOrganizationAddressLine2 = authorityOrganization.OrganizationDto.AddressLine2;
                    newReportPackage.RecipientOrganizationCityName = authorityOrganization.OrganizationDto.CityName;
                    newReportPackage.RecipientOrganizationJurisdictionName = _dbContext.Organizations
                        .Include(o => o.Jurisdiction)
                        .Single(o => o.OrganizationId == authorityOrganization.OrganizationDto.OrganizationId).Jurisdiction.Name;
                    newReportPackage.RecipientOrganizationZipCode = authorityOrganization.OrganizationDto.ZipCode;

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

                    //Associate all existing and eligible samples to this draft
                    //
                    //From UC-16.4: "System pulls all samples in status "Ready to Report" with 
                    //Sample Start date or Sample End Date on or between report period into Draft"
                    //

                    var existingEligibleSamples = _dbContext.Samples
                        .Where(s => s.ForOrganizationRegulatoryProgramId == currentOrgRegProgramId
                            && s.IsReadyToReport
                            && ((s.StartDateTimeUtc <= endDateTimeUtc && s.StartDateTimeUtc >= startDateTimeUtc) ||
                                (s.EndDateTimeUtc <= endDateTimeUtc && s.EndDateTimeUtc >= startDateTimeUtc)));

                    _dbContext.SaveChanges(); //Need to do this to get new Id for samplesAndResultsReportPackageElementCategory

                    var samplesAndResultsReportPackageElementCategory = _dbContext.ReportPackageElementCategories
                        .Include(rpec => rpec.ReportElementCategory)
                        .Include(rpec => rpec.ReportPackageElementTypes)
                        .SingleOrDefault(rpec => rpec.ReportPackageId == newReportPackage.ReportPackageId
                            && rpec.ReportElementCategory.Name == ReportElementCategoryName.SamplesAndResults.ToString());

                    if (samplesAndResultsReportPackageElementCategory == null)
                    {
                        //throw Exception
                        throw new Exception($"ERROR: Could not find a ReportPackageElementCategory associated with '{ReportElementCategoryName.SamplesAndResults}', reportPackageTemplateId={reportPackageTemplateId}");
                    }

                    var samplesAndResultsReportPackageElementType = samplesAndResultsReportPackageElementCategory.ReportPackageElementTypes.FirstOrDefault();
                    //Should be 1 and only 1 Report Package Element Type for Sample and Results 
                    //(but may be more than 1 in the future -- if so, which one is used to add samples??)
                    if (samplesAndResultsReportPackageElementType == null)
                    {
                        //throw Exception
                        throw new Exception($"ERROR: Missing ReportPackageElementCategory associated with '{ReportElementCategoryName.SamplesAndResults}', reportPackageTemplateId={reportPackageTemplateId}");
                    }

                    foreach (var sample in existingEligibleSamples)
                    {
                        var reportSampleAssociation = new ReportSample();
                        reportSampleAssociation.ReportPackageElementTypeId = samplesAndResultsReportPackageElementType.ReportPackageElementTypeId;
                        reportSampleAssociation.SampleId = sample.SampleId;
                        _dbContext.ReportSamples.Add(reportSampleAssociation);
                    }

                    _dbContext.SaveChanges();
                    transaction.Commit();

                    newReportPackageId = newReportPackage.ReportPackageId;
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

                if (samplesReportPackageElementCategory == null)
                {
                    //throw Exception
                    throw new Exception($"ERROR: Missing ReportPackageElementCategory associated with '{ReportElementCategoryName.SamplesAndResults}', reportPackageDto.ReportPackageId={reportPackageDto.ReportPackageId}");
                }

                //Handle deletions first
                // - Iterate through all SampleAndResult rows in ReportSample and delete ones that cannot be matched with an item in reportPackageDto.AssociatedSamples
                foreach (var existingSamplesReportPackageElementType in samplesReportPackageElementCategory.ReportPackageElementTypes)
                {
                    //Should just be one iteration through this loop for the current phase, but in the future
                    //we might have more than one "Sample and Results" section in a Report Package

                    var existingReportSamples = existingSamplesReportPackageElementType.ReportSamples.ToArray();
                    for (var i = 0; i < existingReportSamples.Length; i++)
                    {
                        var existingReportSample = existingReportSamples[i];
                        //Find match in dto samples
                        var matchedSampleAssociation = reportPackageDto.AssociatedSamples
                        .SingleOrDefault(sa => sa.ReportPackageElementTypeId == existingReportSample.ReportPackageElementTypeId
                            && sa.SampleId == existingReportSample.SampleId);

                        if (matchedSampleAssociation == null)
                        {
                            //existing association must have been deleted -- remove
                            _dbContext.ReportSamples.Remove(existingReportSample);
                        }
                    }
                }

                //Now handle additions
                // - Iteration through all requested sample associations (in dto) and add ones that do not already exist
                foreach (var requestedSampleAssociation in reportPackageDto.AssociatedSamples)
                {
                    var foundReportSample = _dbContext.ReportSamples
                        .SingleOrDefault(rs => rs.ReportPackageElementTypeId == requestedSampleAssociation.ReportPackageElementTypeId
                            && rs.SampleId == requestedSampleAssociation.SampleId);

                    if (foundReportSample == null)
                    {
                        //Need to add association
                        _dbContext.ReportSamples.Add(new ReportSample()
                        {
                            SampleId = requestedSampleAssociation.SampleId,
                            ReportPackageElementTypeId = requestedSampleAssociation.ReportPackageElementTypeId
                        });
                    }

                }

                //FILES
                //===================

                //Find entry in tReportPackageElementType for this reportPackage associated with Files category
                var filesReportPackageElementCategory = reportPackage.ReportPackageElementCategories
                    .SingleOrDefault(rpet => rpet.ReportElementCategory.Name == ReportElementCategoryName.Attachments.ToString());

                if (filesReportPackageElementCategory == null)
                {
                    //throw Exception
                    throw new Exception($"ERROR: Missing ReportPackageElementCategory associated with '{ReportElementCategoryName.Attachments}', reportPackageDto.ReportPackageId={reportPackageDto.ReportPackageId}");
                }

                //Handle deletions first
                // - Iterate through all Attachment rows in ReportFile and delete ones that cannot be matched with an item in reportPackageDto.AssociatedSamples
                foreach (var existingFilesReportPackageElementType in filesReportPackageElementCategory.ReportPackageElementTypes)
                {
                    var existingReportFiles = existingFilesReportPackageElementType.ReportFiles.ToArray();
                    for (var i = 0; i < existingReportFiles.Length; i++)
                    {
                        var existingReportFile = existingReportFiles[i];
                        //Find match in dto files
                        var matchedFileAssociation = reportPackageDto.AssociatedFiles
                        .SingleOrDefault(sa => sa.ReportPackageElementTypeId == existingReportFile.ReportPackageElementTypeId
                            && sa.FileStoreId == existingReportFile.FileStoreId);

                        if (matchedFileAssociation == null)
                        {
                            //existing association must have been deleted -- remove
                            _dbContext.ReportFiles.Remove(existingReportFile);
                        }
                    }

                }

                //Now handle additions
                // - Iteration through all requested file associations (in dto) and add ones that do not already exist
                foreach (var requestedFileAssociation in reportPackageDto.AssociatedFiles)
                {
                    var foundReportFile = _dbContext.ReportFiles
                        .SingleOrDefault(rs => rs.ReportPackageElementTypeId == requestedFileAssociation.ReportPackageElementTypeId
                            && rs.FileStoreId == requestedFileAssociation.FileStoreId);

                    if (foundReportFile == null)
                    {
                        //Need to add association
                        _dbContext.ReportFiles.Add(new ReportFile()
                        {
                            FileStoreId = requestedFileAssociation.FileStoreId,
                            ReportPackageElementTypeId = requestedFileAssociation.ReportPackageElementTypeId
                        });
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
                    //No transition -- allowed?
                }
                else if ((previousStatus == ReportStatusName.Draft.ToString() && reportStatus == ReportStatusName.ReadyToSubmit) ||
                    (previousStatus == ReportStatusName.ReadyToSubmit.ToString() && reportStatus == ReportStatusName.Draft))
                {
                    //allowed
                }
                else if (previousStatus == ReportStatusName.ReadyToSubmit.ToString() && reportStatus == ReportStatusName.Submitted)
                {
                    //allowed
                }
                else if (previousStatus == ReportStatusName.Submitted.ToString() && reportStatus == ReportStatusName.Repudiated)
                {
                    //allowed
                }
                else
                {
                    //not allowed
                    string message = $"Cannot change a Report Package status from '{previousStatus}' to '{reportStatus}'.";
                    List<RuleViolation> validationIssues = new List<RuleViolation>();
                    validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
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
        /// Gets a collection of FileStoreDto's that are eligible to be added this Report Package
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <returns>Collection of FileStoreDto objects</returns>
        public ICollection<FileStoreDto> GetFilesForSelection(int reportPackageId)
        {
            _logger.Info($"Enter ReportPackageService.GetFilesForSelection. reportPackageId={reportPackageId}");

            var fileStoreList = new List<FileStoreDto>();
            var ageInMonthsSinceFileUploaded = Int32.Parse(_settingService.GetGlobalSettings()[SystemSettingType.FileAvailableToAttachMaxAgeMonths]);
            var xMonthsAgo = DateTimeOffset.UtcNow.AddMonths(-ageInMonthsSinceFileUploaded);

            var reportPackage = _dbContext.ReportPackages
                .Include(rp => rp.ReportPackageElementCategories)
                .Include(rp => rp.ReportPackageElementCategories.Select(rc => rc.ReportElementCategory))
                .Include(rp => rp.ReportPackageElementCategories.Select(rc => rc.ReportPackageElementTypes))
                .Single(rp => rp.ReportPackageId == reportPackageId);

            var filesReportPackageElementCategory = reportPackage.ReportPackageElementCategories
               .SingleOrDefault(rpet => rpet.ReportElementCategory.Name == ReportElementCategoryName.Attachments.ToString());

            if (filesReportPackageElementCategory == null)
            {
                //throw Exception
                throw new Exception($"ERROR: Missing ReportPackageElementCategory associated with '{ReportElementCategoryName.Attachments}', reportPackageId={reportPackageId}");
            }

            foreach (var existingFilesReportPackageElementType in filesReportPackageElementCategory.ReportPackageElementTypes)
            {
                var filesOfThisReportElementType = _dbContext.FileStores
                    .Where(fs => fs.ReportElementTypeId == existingFilesReportPackageElementType.ReportElementTypeId
                        && fs.UploadDateTimeUtc >= xMonthsAgo)
                    .ToList();

                foreach (var eligibleFile in filesOfThisReportElementType)
                {
                    var fileStoreDto = _mapHelper.GetFileStoreDtoFromFileStore(eligibleFile);
                    fileStoreDto.ReportPackageElementTypeId = existingFilesReportPackageElementType.ReportPackageElementTypeId;
                    fileStoreList.Add(fileStoreDto);
                }
            }

            _logger.Info($"Leave ReportPackageService.GetFilesForSelection. reportPackageId={reportPackageId}, fileStoreList.Count={fileStoreList.Count}");

            return fileStoreList;
        }

        /// <summary>
        /// Gets a collection of SampleDto's that are eligible to be added this Report Package
        /// </summary>
        /// <param name="reportPackageId">tReportPackage.ReportPackageId</param>
        /// <returns>Collection of SampleDto objects</returns>
        public ICollection<SampleDto> GetSamplesForSelection(int reportPackageId)
        {
            _logger.Info($"Enter ReportPackageService.GetSamplesForSelection. reportPackageId={reportPackageId}");

            var reportPackage = _dbContext.ReportPackages
                .Single(rp => rp.ReportPackageId == reportPackageId);

            var eligibleSampleList = new List<SampleDto>();

            var existingEligibleSamples = _dbContext.Samples
                        .Include(s => s.ByOrganizationRegulatoryProgram.Organization.OrganizationType)
                        .Where(s => s.ForOrganizationRegulatoryProgramId == reportPackage.OrganizationRegulatoryProgramId
                            && s.IsReadyToReport
                            && ((s.StartDateTimeUtc <= reportPackage.PeriodEndDateTimeUtc && s.StartDateTimeUtc >= reportPackage.PeriodStartDateTimeUtc) ||
                                (s.EndDateTimeUtc <= reportPackage.PeriodEndDateTimeUtc && s.EndDateTimeUtc >= reportPackage.PeriodStartDateTimeUtc)));

            foreach (var existingEligibleSample in existingEligibleSamples)
            {
                var sampleDto = _mapHelper.GetSampleDtoFromSample(existingEligibleSample);
                eligibleSampleList.Add(sampleDto);
            }

            _logger.Info($"Leave ReportPackageService.GetSamplesForSelection. reportPackageId={reportPackageId}, eligibleSampleList.Count={eligibleSampleList.Count}");

            return eligibleSampleList;
        }

        /// <summary>
        /// Gets Report Package information (without children element data) for displaying in a grid.
        /// </summary>
        /// <param name="reportStatusName">Fetches report packages of this status only</param>
        /// <returns>Collection of ReportPackageDto objects (without children element data)</returns>
        public IEnumerable<ReportPackageDto> GetReportPackagesByStatusName(ReportStatusName reportStatusName)
        {
            _logger.Info($"Enter ReportPackageService.GetReportPackagesByStatusName. reportStatusName={reportStatusName}");

            var reportPackageDtoList = new List<ReportPackageDto>();

            var currentOrgRegProgramId = int.Parse(_httpContextService.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));

            var reportPackages = _dbContext.ReportPackages
                .Include(rp => rp.ReportStatus)
                .Where(rp => rp.OrganizationRegulatoryProgramId == currentOrgRegProgramId
                    && rp.ReportStatus.Name == reportStatusName.ToString())
                .ToList();

            foreach (var reportPackage in reportPackages)
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
                    .GetLocalizedDateTimeUsingThisTimeZoneId(repudiationReason.CreationDateTimeUtc.DateTime, timeZoneId);

                if (repudiationReason.LastModificationDateTimeUtc.HasValue)
                {
                    repudiationReasonDto.LastModificationLocalDateTime = _timeZoneService
                        .GetLocalizedDateTimeUsingThisTimeZoneId(repudiationReason.LastModificationDateTimeUtc.Value.DateTime, timeZoneId);
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
                    var authorityOrganization = _orgService.GetAuthority(currentOrgRegProgramId);
                    var timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(currentOrgRegProgramId, SettingType.TimeZone));
                    var timeZone = _dbContext.TimeZones.Single(tz => tz.TimeZoneId == timeZoneId);

                    //Check has Signatory Rights (UC-19 5.1.)
                    var orgRegProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                        .Single(orpu => orpu.UserProfileId == currentUserId
                            && orpu.OrganizationRegulatoryProgramId == currentOrgRegProgramId);

                    if (!orgRegProgramUser.IsSignatory)
                    {
                        ThrowSimpleException("User not authorized to repudiate report packages.");
                    }

                    //Check ARP config "Max days after report period end date to repudiate" has not passed (UC-19 5.2.)
                    var reportRepudiatedDays = Convert.ToInt32(_settingService.GetOrganizationSettingValue(authorityOrganization.OrganizationRegulatoryProgramId, SettingType.ReportRepudiatedDays));

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
                        ThrowSimpleException($"Reason is required.");
                    }

                    //=========
                    //Repudiate
                    //=========
                    var currentUser = _dbContext.Users
                        .Single(user => user.UserProfileId == currentUserId);

                    reportPackage.RepudiationDateTimeUtc = _timeZoneService.GetLocalizedDateTimeOffsetUsingThisTimeZoneId(DateTime.UtcNow, timeZoneId);
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
                    var industrySignatories = _dbContext.OrganizationRegulatoryProgramUsers
                        .Where(orpu => orpu.OrganizationRegulatoryProgramId == currentOrgRegProgramId
                            && orpu.IsSignatory
                            && !orpu.IsRemoved
                            && !orpu.IsRegistrationDenied
                            && orpu.IsRegistrationApproved)
                         .ToList();

                    var corHash = _dbContext.CopyOfRecords
                        .Single(cor => cor.ReportPackageId == reportPackageId);

                    //Use the same contentReplacement dictionary for both emails and Cromerr audit logging
                    var contentReplacements = new Dictionary<string, string>();

                    //Report Details:
                    contentReplacements.Add("reportPackageName", reportPackage.Name);
                    contentReplacements.Add("periodStartDate", reportPackage.PeriodStartDateTimeUtc.DateTime.ToString("MMM d, yyyy"));
                    contentReplacements.Add("periodEndDate", reportPackage.PeriodEndDateTimeUtc.DateTime.ToString("MMM d, yyyy"));
                    contentReplacements.Add("submissionDateTime", reportPackage.SubmissionDateTimeUtc.Value.DateTime.ToString("MMM d, yyyy h:mmtt") +
                        $" {_timeZoneService.GetTimeZoneNameUsingThisTimeZone(timeZone, reportPackage.SubmissionDateTimeUtc.Value.DateTime, true)}");
                    contentReplacements.Add("corSignature", corHash.Hash);
                    contentReplacements.Add("repudiatedDateTime", reportPackage.RepudiationDateTimeUtc.Value.DateTime.ToString("MMM d, yyyy h:mmtt") +
                        $" {_timeZoneService.GetTimeZoneNameUsingThisTimeZone(timeZone, reportPackage.RepudiationDateTimeUtc.Value.DateTime, true)}");
                    contentReplacements.Add("repudiationReason", repudiationReasonName);
                    contentReplacements.Add("repudiationReasonComments", comments ?? "");

                    //Repudiated to:
                    contentReplacements.Add("authOrganizationName", reportPackage.RecipientOrganizationName);
                    contentReplacements.Add("authOrganizationAddressLine1", reportPackage.RecipientOrganizationAddressLine1);
                    contentReplacements.Add("authOrganizationAddressLine2", reportPackage.RecipientOrganizationAddressLine2);
                    contentReplacements.Add("authOrganizationCityName", reportPackage.RecipientOrganizationCityName);
                    contentReplacements.Add("authOrganizationJurisdictionName", reportPackage.RecipientOrganizationJurisdictionName);
                    contentReplacements.Add("authOrganizationZipCode", reportPackage.RecipientOrganizationZipCode);

                    //Repudiated by:
                    contentReplacements.Add("submitterFirstName", currentUser.FirstName);
                    contentReplacements.Add("submitterLastName", currentUser.LastName);
                    contentReplacements.Add("submitterTitle", currentUser.TitleRole ?? "");
                    contentReplacements.Add("iuOrganizationName", reportPackage.OrganizationName);
                    contentReplacements.Add("permitNumber", reportPackage.OrganizationRegulatoryProgram.ReferenceNumber);
                    contentReplacements.Add("organizationAddressLine1", reportPackage.OrganizationAddressLine1);
                    contentReplacements.Add("organizationAddressLine2", reportPackage.OrganizationAddressLine2);
                    contentReplacements.Add("organizationCityName", reportPackage.OrganizationCityName);
                    contentReplacements.Add("organizationJurisdictionName", reportPackage.OrganizationJurisdictionName);
                    contentReplacements.Add("organizationZipCode", reportPackage.OrganizationZipCode);

                    contentReplacements.Add("userName", currentUser.UserName);
                    contentReplacements.Add("corViewLink", $"/reportPackage/{reportPackage.ReportPackageId}/cor");

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

                    foreach (var industrySignatory in industrySignatories)
                    {
                        var industrySignatoryUser = _dbContext.Users
                            .Single(user => user.UserProfileId == industrySignatory.UserProfileId);

                        _emailService.SendEmail(new[] { industrySignatoryUser.Email }, EmailType.Report_Repudiation_IU, contentReplacements);
                    }

                    //System sends Submission Receipt to all Standard Users for the Authority (UC-19 8.4.)
                    var standardUsersOfAuthority = _dbContext.OrganizationRegulatoryProgramUsers
                        .Include(orpu => orpu.PermissionGroup)
                        .Where(orpu => orpu.OrganizationRegulatoryProgramId == authorityOrganization.OrganizationRegulatoryProgramId
                            && !orpu.IsRemoved
                            && !orpu.IsRegistrationDenied
                            && orpu.IsRegistrationApproved
                            && orpu.PermissionGroup.Name == PermissionGroupName.Standard.ToString())
                        .ToList();

                    foreach (var standardUserOfAuthority in standardUsersOfAuthority)
                    {
                        var standardUserOfAuthorityUser = _dbContext.Users
                            .Single(user => user.UserProfileId == standardUserOfAuthority.UserProfileId);

                        _emailService.SendEmail(new[] { standardUserOfAuthorityUser.Email }, EmailType.Report_Repudiation_AU, contentReplacements);
                    }

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

                    _crommerAuditLogService.Log(CromerrEvent.Report_Repudiated, cromerrAuditLogEntryDto,
                                                contentReplacements);

                    _dbContext.SaveChanges();

                    transaction.Commit();
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

                    reportPackage.SubmissionReviewDateTimeUtc = DateTime.UtcNow;
                    reportPackage.SubmissionReviewerUserId = currentUserId;
                    reportPackage.SubmissionReviewerFirstName = user.FirstName;
                    reportPackage.SubmissionReviewerLastName = user.LastName;
                    reportPackage.SubmissionReviewerTitleRole = user.TitleRole;

                    if (!String.IsNullOrEmpty(comments))
                        reportPackage.SubmissionReviewComments = comments;

                    _dbContext.SaveChanges();

                    transaction.Commit();
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

                    reportPackage.RepudiationReviewDateTimeUtc = DateTime.UtcNow;
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

            _logger.Info($"Leave ReportPackageService.ReviewRepudiation. reportPackageId={reportPackageId}, comments={comments}, currentOrgRegProgramId={currentOrgRegProgramId}, currentUserId={currentUserId}");

        }

        private string EmtpyStringIfNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "" : value;
        }
        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
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

            reportPackagegDto.OrganizationRegulatoryProgramDto =
                    _programService.GetOrganizationRegulatoryProgram(reportPackage.OrganizationRegulatoryProgramId);

            if (reportPackage.SubmissionDateTimeUtc.HasValue)
            {
                reportPackagegDto.SubmissionDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.SubmissionDateTimeUtc.Value.DateTime, timeZoneId);
            }

            if (reportPackage.RepudiationDateTimeUtc.HasValue)
            {
                reportPackagegDto.RepudiationDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.RepudiationDateTimeUtc.Value.DateTime, timeZoneId);
            }

            if (reportPackage.RepudiationReviewDateTimeUtc.HasValue)
            {
                reportPackagegDto.RepudiationReviewDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.RepudiationReviewDateTimeUtc.Value.DateTime, timeZoneId);
            }

            reportPackagegDto.PeriodEndDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.PeriodEndDateTimeUtc.DateTime, timeZoneId);

            reportPackagegDto.PeriodStartDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.PeriodStartDateTimeUtc.DateTime, timeZoneId);

            reportPackagegDto.CreationDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingThisTimeZoneId(reportPackage.CreationDateTimeUtc.DateTime, timeZoneId);

            return reportPackagegDto;
        }

        /// <summary>
        /// Used to simplify and clean up methods where there are multiple validation tests.
        /// </summary>
        /// <param name="message">Rule violation message to use when throwing the exception.</param>
        private void ThrowSimpleException(string message)
        {
            _logger.Info($"Enter SampleService.ThrowSimpleException. message={message}");

            List<RuleViolation> validationIssues = new List<RuleViolation>();
            validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));

            _logger.Info($"Leaving SampleService.ThrowSimpleException. message={message}");

            throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
        }
    }
}