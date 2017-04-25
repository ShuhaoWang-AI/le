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
using Linko.LinkoExchange.Services.Report.DataXML;
using FileInfo = Linko.LinkoExchange.Services.Report.DataXML.FileInfo;
using Linko.LinkoExchange.Services.Organization;

namespace Linko.LinkoExchange.Services.Report
{
    // TODO: To implement more 

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
        private readonly IMapHelper _mapHelper;

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
            IMapHelper mapHelper
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
            _mapHelper = mapHelper;
        }


        /// <summary>
        /// Note:  before call this function,  make sure to update draf first.
        /// </summary>
        /// <param name="reportPackageId"></param>
        public void SignAndSubmitReportPackage(int reportPackageId)
        {
            _logger.Info("Enter ReportPackageService.SignAndSubmitReportPackage. reportPackageId={0}", reportPackageId);

            //TODO to add reportPakcage data temporary
            // To be removed once saving reportPackage is done 
            var reportPackageTemp = new ReportPackage();
            reportPackageTemp.Name = "1st Quarter PCR";
            reportPackageTemp.PeriodStartDateTimeUtc = DateTime.UtcNow;
            reportPackageTemp.PeriodStartDateTimeUtc = DateTime.UtcNow.AddMonths(10);
            reportPackageTemp.IsSubmissionBySignatoryRequired = true;
            reportPackageTemp.ReportStatusId = 1;

            // organizationid = 1012, organizationTypeId =2 
            // name:Valley City Plating
            // OrganizationRegulatoryProgramId = 11  
            // It has two users. 
            // userProfileId:4  David Pelletier  //signatory
            //               7  Jon Rasche 

            reportPackageTemp.OrganizationRegulatoryProgramId = 11;
            reportPackageTemp.OrganizationName = "Valley City Plating";
            reportPackageTemp.OrganizationAddressLine1 = "3353 Eastern, S.E.";
            reportPackageTemp.OrganizationAddressLine2 = "";
            reportPackageTemp.OrganizationCityName = "Grand Rapids";
            reportPackageTemp.OrganizationJurisdictionName = "Michigan";
            reportPackageTemp.OrganizationZipCode = "49508";

            reportPackageTemp.RecipientOrganizationName = "City of Grand Rapids";
            reportPackageTemp.RecipientOrganizationAddressLine1 = "1300 Market Ave., S.W.";
            reportPackageTemp.RecipientOrganizationAddressLine2 = "";
            reportPackageTemp.RecipientOrganizationCityName = "Grand Rapids";
            reportPackageTemp.RecipientOrganizationJurisdictionName = "Michigan";
            reportPackageTemp.RecipientOrganizationZipCode = "49503";

            reportPackageTemp.CreationDateTimeUtc = DateTime.UtcNow;

            _dbContext.ReportPackages.Add(reportPackageTemp);
            reportPackageId = _dbContext.SaveChanges();

            //TODO: end of temporary code

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    List<RuleViolation> validationIssues = new List<RuleViolation>();

                    var reportPackage = _dbContext.ReportPackages.Single(i => i.ReportPackageId == reportPackageId);
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
                    reportPackage.SubmissionReviewerUserId = submitterUserId;
                    reportPackage.SubmitterFirstName = submitterFirstName;
                    reportPackage.SubmitterLastName = submitterLastName;
                    reportPackage.SubmitterTitleRole = submitterTitleRole;
                    reportPackage.SubmitterIPAddress = submitterIpAddress;
                    reportPackage.SubmitterUserName = submitterUserName;

                    var reportStatus = _dbContext.ReportStatuses.Single(i => i.Name.Equals(ReportStatusName.Submitted.ToString()));
                    reportPackage.ReportStatusId = reportStatus.ReportStatusId;

                    _dbContext.SaveChanges();

                    var reportPackageDto = GetReportPackage(reportPackageId, true);

                    //// TODO:
                    //// Comment out temporary, using hard code temporary 
                    //// var copyOfRecordDto = CreateCopyOfRecordForReportPackage(reportPackageDto);

                    //// TODO: to remove below line for hard code testing purpose 
                    var temp = reportPackageDto.ReportPackageId;
                    reportPackageDto.ReportPackageId = 1355344931;
                    var copyOfRecordDto = GetCopyOfRecordByReportPackageId(1355344931, reportPackageDto);
                    reportPackageDto.ReportPackageId = temp;

                    //// Send emails 
                    SendSignAndSubmitEmail(reportPackageDto, copyOfRecordDto);
                    _dbContext.SaveChanges();
                    transaction.Commit();
                    _logger.Info("Enter ReportPackageService.SignAndSubmitReportPackage. reportPackageId={0}", reportPackageId);
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
            var dataXmlObj = new CopyOfRecordDataXml
            {
                XmlFileVersion = new XmlFileVersion
                {
                    VersionNumber = "1.0"
                },
                ReportHeader = new ReportHeader
                {
                    ReportName = reportPackageDto.Name,
                    //TODO:  UTC or local time?  
                    ReportPeriodStartDateTimeUtc = reportPackageDto.PeriodStartDateTimeLocal.ToString(format: "yyyy-MM-dd"),
                    ReportPeriodEndDateTimeUtc = reportPackageDto.PeriodEndDateTimeLocal.ToString(format: "yyyy-MM-dd"),
                    ReportSubmissionDateUtc = $"{reportPackageDto.SubmissionDateTimeLocal:MMM dd, yyyy HHtt}",
                    CurrentTimeZoneName = "Place_holder"
                },
                SubmittedTo = new SubmittedTo
                {
                    OrganizationName = reportPackageDto.RecipientOrganizationName,
                    Address1 = reportPackageDto.RecipientOrganizationAddressLine1,
                    Address2 = string.IsNullOrEmpty(reportPackageDto.RecipientOrganizationAddressLine2) == true ? "" : reportPackageDto.RecipientOrganizationAddressLine2,
                    City = reportPackageDto.RecipientOrganizationCityName,
                    State = reportPackageDto.RecipientOrganizationJurisdictionName,
                    ZipCode = reportPackageDto.RecipientOrganizationZipCode
                },
                SubmittedOnBehalfOf = new SubmittedOnBehalfOf
                {
                    OrganizationName = reportPackageDto.RecipientOrganizationName,
                    ReferenceNumber = reportPackageDto.OrganizationRegulatoryProgramDto.ReferenceNumber,
                    Address1 = reportPackageDto.RecipientOrganizationAddressLine1,
                    Address2 = string.IsNullOrEmpty(reportPackageDto.RecipientOrganizationAddressLine2) == true ? "" : reportPackageDto.RecipientOrganizationAddressLine2,
                    City = reportPackageDto.RecipientOrganizationCityName,
                    State = reportPackageDto.RecipientOrganizationJurisdictionName,
                    ZipCode = reportPackageDto.RecipientOrganizationZipCode
                },
                SubmittedBy = new SubmittedBy
                {
                    FirstName = reportPackageDto.SubmitterFirstName,
                    LastName = reportPackageDto.SubmitterLastName,
                    Title = reportPackageDto.SubmitterTitleRole,
                    UserName = reportPackageDto.SubmitterUserName,
                    ReportSubmissionFromIP = reportPackageDto.SubmitterIPAddress
                },
                FileManifest = new FileManifest
                {
                    Files = reportPackageDto.AttachmentDtos.Select(i => new FileInfo
                    {
                        OriginalFileName = i.OriginalFileName,
                        SystemGeneratedUnqiueFileName = i.Name,
                        AttachmentType = i.FileType
                    }).ToList()
                }
            };

            dataXmlObj.FileManifest.Files.Add(
                new FileInfo
                {
                    OriginalFileName = "Copy Of Record Data.xml",
                    SystemGeneratedUnqiueFileName = "Copy Of Record Data.xml",
                    AttachmentType = "Xml Raw Data"
                });

            dataXmlObj.FileManifest.Files.Add(
                new FileInfo
                {
                    OriginalFileName = "Copy Of Record.pdf",
                    SystemGeneratedUnqiueFileName = "Copy Of Record.pdf",
                    AttachmentType = "Copy Of Record PDF"
                });


            dataXmlObj.Certifications = new List<Certification>();
            dataXmlObj.Certifications = reportPackageDto.CertificationDtos.Select(i => new Certification
            {
                CertificationType = i.ReportElementTypeName,
                CertificationText = i.ReportElementTypeContent
            }).ToList();

            dataXmlObj.Comment = reportPackageDto.Comments;

            // SampleResults part 
            dataXmlObj.SampleResults = new List<SampleResultNode>();

            foreach (var sampleDto in reportPackageDto.SamplesDtos)
            {
                foreach (var sampleResultDto in sampleDto.SampleResults)
                {
                    var sampleResultNode = new SampleResultNode
                    {
                        SampleName = sampleDto.Name,
                        MonitoringPointName = sampleDto.MonitoringPointName,
                        CtsEventTypeCategoryName = sampleDto.CtsEventCategoryName,
                        CtsEventTypeName = sampleDto.CtsEventTypeName,
                        CollectionMethodName = sampleDto.CollectionMethodName,
                        LabSampleIdentifier = sampleDto.LabSampleIdentifier,

                        //TODO to covert the date time here 
                        StartDateTimeUtc = sampleDto.StartDateTimeLocal.ToString(),
                        EndDateTimeUtc = sampleDto.EndDateTimeLocal.ToString(),

                        MassLoadingsConversionFactorPounds = sampleDto.MassLoadingConversionFactorPounds?.ToString(),
                        MassLoadingCalculationDecimalPlaces = sampleDto.MassLoadingCalculationDecimalPlaces?.ToString(),
                        IsMassLoadingResultToUseLessThanSign = sampleDto.IsMassLoadingResultToUseLessThanSign.ToString(),

                        SampledBy = sampleDto.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName,
                        ParameterName = sampleResultDto.ParameterName,
                        Qualifier = System.Net.WebUtility.HtmlEncode(sampleResultDto.Qualifier),
                        Value = sampleResultDto.Value,
                        UnitName = sampleResultDto.UnitName,
                        EnteredMethodDetectionLimit = sampleResultDto.EnteredMethodDetectionLimit,
                        MethodDetectionLimit = sampleResultDto.MethodDetectionLimit.ToString(),
                        AnalysisMethod = sampleResultDto.AnalysisMethod,

                        //TODO handle the datatime here
                        AnalysisDateTimUtc = sampleResultDto.AnalysisDateTimeLocal.ToString(),
                        IsApprovedEPAMethod = sampleResultDto.IsApprovedEPAMethod.ToString(),

                        LimitBasis = LimitBasisName.Concentration.ToString()
                    };

                    dataXmlObj.SampleResults.Add(sampleResultNode);
                }
            }

            // Convert to xml string
            var strWriter = new Utf8StringWriter();

            var xmlSerializer = new XmlSerializer(dataXmlObj.GetType());
            xmlSerializer.Serialize(strWriter, dataXmlObj);
            var xmlString = strWriter.ToString();

            var xmlData = new CopyOfRecordDataXmlFileDto
            {
                FileData = Encoding.UTF8.GetBytes(s: xmlString),
                FileName = "Copy Of Record Data.xml"
            };
            return xmlData;
        }

        public CopyOfRecordValidationResultDto VerififyCopyOfRecord(int reportPackageId)
        {
            _logger.Info("Enter ReportPackageService.VerififyCopyOfRecord. reportPackageId={0}", reportPackageId);

            var validationResult = _copyOfRecordService.ValidCopyOfRecordData(reportPackageId);

            _logger.Info("Enter ReportPackageService.VerififyCopyOfRecord. reportPackageId={0}", reportPackageId);

            return validationResult;
        }

        //TODO: to implement this!
        public ReportPackageDto GetReportPackage(int reportPackageId, bool incldingAttachmentFileData)
        {
            _logger.Info("Enter ReportPackageService.GetReportPackage. reportPackageId={0}", reportPackageId);

            var rpt = _dbContext.ReportPackages.Single(i => i.ReportPackageId == reportPackageId);
            var rptDto = _mapHelper.GetReportPackageDtoFromReportPackage(rpt);
            rptDto.OrganizationRegulatoryProgramDto =
                    _programService.GetOrganizationRegulatoryProgram(rpt.OrganizationRegulatoryProgramId);

            rptDto.SubmissionDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingSettingForThisOrg(rpt.CreationDateTimeUtc.DateTime, rpt.OrganizationRegulatoryProgramId);

            rptDto.PeriodEndDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingSettingForThisOrg(rpt.PeriodEndDateTimeUtc.DateTime, rpt.OrganizationRegulatoryProgramId);

            rptDto.PeriodStartDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingSettingForThisOrg(rpt.PeriodStartDateTimeUtc.DateTime, rpt.OrganizationRegulatoryProgramId);

            rptDto.CreationDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingSettingForThisOrg(rpt.CreationDateTimeUtc.DateTime, rpt.OrganizationRegulatoryProgramId);

            _logger.Info("Leave ReportPackageService.GetReportPackage. reportPackageId={0}", reportPackageId);
            return rptDto;
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
            var attachments = reportPackageDto.AttachmentDtos;
            var copyOfRecordPdfFile = GetReportPackageCopyOfRecordPdfFile(reportPackageDto);
            var copyOfRecordDataXmlFile = GetReportPackageCopyOfRecordDataXmlFile(reportPackageDto);
            var copyOfRecordDto = _copyOfRecordService.CreateCopyOfRecordForReportPackage(reportPackageId, attachments, copyOfRecordPdfFile, copyOfRecordDataXmlFile);

            _logger.Info("Leave ReportPackageService.CreateCopyOfRecordForReportPackage. reportPackageId={0}", reportPackageId);

            return copyOfRecordDto;
        }

        private void SendSignAndSubmitEmail(ReportPackageDto reportPackage, CopyOfRecordDto copyOfRecordDto)
        {
            _logger.Info("Enter ReportPackageService.SendSignAndSubmitEmail. reportPackageId={0}", reportPackage.ReportPackageId);

            var emailContentReplacements = new Dictionary<string, string>();

            emailContentReplacements.Add("iuOrganizationName", reportPackage.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName);
            emailContentReplacements.Add("reportPackageName", reportPackage.Name);

            emailContentReplacements.Add("periodStartDate", reportPackage.PeriodStartDateTimeLocal.ToString("MMM dd, yyyy"));
            emailContentReplacements.Add("periodEndDate", reportPackage.PeriodEndDateTimeLocal.ToString("MMM dd, yyyy"));

            var submissionDateTime =
                $"{reportPackage.SubmissionDateTimeLocal.ToString("MMM dd, yyyy HHtt ")}{_timeZoneService.GetAbbreviationTimeZoneNameUsingSettingForThisOrg(reportPackage.OrganizationRegulatoryProgramId)}";

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

            //TODO: use the real cor view path
            emailContentReplacements.Add("corViewLink", $"/reportPackage/cor/{reportPackage.ReportPackageId}");

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
            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var reportPackageElementCategories = _dbContext.ReportPackageElementCategories
                    .Include(rpec => rpec.ReportPackageElementTypes)
                    .Where(rpec => rpec.ReportPackageId == reportPackageId);

                    foreach (var rpec in reportPackageElementCategories)
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
                    _dbContext.ReportPackageElementCategories.RemoveRange(reportPackageElementCategories);

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
            newReportPackage.PeriodStartDateTimeUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(startDateTimeLocal, timeZoneId);
            newReportPackage.PeriodEndDateTimeUtc = _timeZoneService.GetUTCDateTimeUsingThisTimeZoneId(endDateTimeLocal, timeZoneId);
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
                .Where(rptec => rptec.ReportPackageTemplateId == reportPackageTemplateId);

            foreach (var rptec in reportPackageTemplateElementCategories)
            {
                //Create a row in tReportPackageElementCategory
                var newReportPackageElementCategory = new ReportPackageElementCategory()
                {
                    ReportElementCategoryId = rptec.ReportElementCategoryId,
                    SortOrder = rptec.SortOrder
                };
                newReportPackage.ReportPackageElementCategories.Add(newReportPackageElementCategory); //handles setting ReportPackageId

                foreach (var rptet in rptec.ReportPackageTemplateElementTypes)
                {
                    var reportElementType = _dbContext.ReportElementTypes
                        .Include(ret => ret.CtsEventType)
                        .Single(ret => ret.ReportElementTypeId == rptet.ReportElementTypeId);

                    //Create a row in tReportPackageElementType
                    var newReportPackageElementType = new ReportPackageElementType() {
                        ReportElementTypeId = rptet.ReportElementTypeId,
                        ReportElementTypeName = reportElementType.Name,
                        ReportElementTypeContent = reportElementType.Content,
                        ReportElementTypeIsContentProvided = reportElementType.IsContentProvided,
                        CtsEventTypeId = reportElementType.CtsEventTypeId,
                        CtsEventTypeName = reportElementType.CtsEventType.Name,
                        CtsEventCategoryName = reportElementType.CtsEventType.CtsEventCategoryName,
                        IsRequired = rptet.IsRequired,
                        SortOrder = rptet.SortOrder
                    };
                    newReportPackageElementType.ReportPackageElementCategory = newReportPackageElementCategory; //handles setting ReportPackageElementCategoryId
                }
            }

            _dbContext.ReportPackages.Add(newReportPackage);
            return newReportPackage.ReportPackageId;
        }

        /// <summary>
        /// Cannot be used to CREATE, only to UPDATE. Use "CreateDraft" to create.
        /// reportPackageDto.ReportPackageId must exist or exception thrown
        /// </summary>
        /// <param name="reportPackageDto">Existing Report Package to update</param>
        /// <returns>Existing ReportPackage.ReportPackageId</returns>
        public int SaveReportPackage(ReportPackageDto reportPackageDto)
        {
            //Report Package will have already been saved before this call and therefore exists
            if (reportPackageDto.ReportPackageId < 1)
            {
                throw new Exception("ERROR: Cannot call 'SaveReportPackage' without setting reportPackageDto.ReportPackageId.");
            }

            var reportPackage = _dbContext.ReportPackages
                .Single(rp => rp.ReportPackageId == reportPackageDto.ReportPackageId);

            //
            //TO-DO: Add/remove report package elements (samples and files)
            //

            _dbContext.SaveChanges();

            return reportPackage.ReportPackageId;
        }

        /// <summary>
        /// Performs validation to ensure only allowed state transitions are occur,
        /// throw RuleViolationException otherwise
        /// </summary>
        /// <param name="reportStatus">Intended target state</param>
        public void UpdateStatus(int reportPackageId, ReportStatusName reportStatus)
        {
            using (var transaction = _dbContext.BeginTransaction())
            {
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

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;
                }

            }


        }

        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
    }
}