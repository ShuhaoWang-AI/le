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
            _mapHelper = mapHelper;
        }


        public void SignAndSubmitReportPackage(int reportPackageId)
        {

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
                    var reportPackage = _dbContext.ReportPackages.Single(i => i.ReportPackageId == reportPackageId);

                    var submitterUserId = int.Parse(_httpContextService.GetClaimValue(CacheKey.UserProfileId));
                    var submitterFirstName = _httpContextService.GetClaimValue(CacheKey.FirstName);
                    var submitterLastName = _httpContextService.GetClaimValue(CacheKey.LastName);
                    var submitterTitleRole = _httpContextService.GetClaimValue(CacheKey.UserRole);
                    var submitterIPAddress = _httpContextService.CurrentUserIPAddress();
                    var submitterUserName = _httpContextService.GetClaimValue(CacheKey.UserName);

                    reportPackage.SubmissionDateTimeUtc = DateTimeOffset.Now;
                    reportPackage.SubmissionReviewerUserId = submitterUserId;
                    reportPackage.SubmitterFirstName = submitterFirstName;
                    reportPackage.SubmitterLastName = submitterLastName;
                    reportPackage.SubmitterTitleRole = submitterTitleRole;
                    reportPackage.SubmitterIPAddress = submitterIPAddress;
                    reportPackage.SubmitterUserName = submitterUserName;

                    _dbContext.SaveChanges();

                    var reportPackageDto = GetReportPackage(reportPackageId);

                    //// TODO:
                    //// Comment out temporary, using hard code temporary 
                    //// var copyOfRecordDto = CreateCopyOfRecordForReportPackage(reportPackageId);  

                    //// TODO: to remove below line for hard code testing purpose 
                    var temp = reportPackageDto.ReportPackageId;
                    reportPackageDto.ReportPackageId = 1355344931;
                    var copyOfRecordDto = GetCopyOfRecordByReportPackageId(1355344931, reportPackageDto);
                    reportPackageDto.ReportPackageId = temp;
                    //// 
                    // Sending email.... 
                    SendSignAndSubmitEmail(reportPackageDto, copyOfRecordDto);
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
                        errors.Add(item: ex.Message);
                    }
                    _logger.Error(message: "Error happens {0} ", argument: string.Join(separator: "," + Environment.NewLine, values: errors));
                    throw;
                }
            }
        }

        /// <summary>
        ///  Prepare Mock data;
        /// </summary>
        /// <param name="reportPackageId"></param>
        /// <returns></returns>
        public IList<FileStoreDto> GetReportPackageAttachments(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }

        public IList<ReportPackageELementTypeDto> GetReportPackageCertifications(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }

        public CopyOfRecordPdfFileDto GetReportPackageCopyOfRecordPdfFile(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }

        public CopyOfRecordDataXmlFileDto GetReportPackageCopyOfRecordDataXmlFile(int reportPackageId)
        {
            throw new System.NotImplementedException();
        }

        public CopyOfRecordValidationResultDto VerififyCopyOfRecord(int reportPackageId)
        {
            _logger.Info("Enter ReportPackageService.VerififyCopyOfRecord. reportPackageId={0}", reportPackageId);

            var validationResult = _copyOfRecordService.ValidCopyOfRecordData(reportPackageId);

            _logger.Info("Enter ReportPackageService.VerififyCopyOfRecord. reportPackageId={0}", reportPackageId);

            return validationResult;
        }

        //TODO: to implement this!
        public ReportPackageDto GetReportPackage(int reportPackageId)
        {
            //var rptDto = new ReportPackageDto
            //{
            //    ReportPackageId = reportPackageId,
            //    Name = " 1st Quarter PCR",
            //    OrganizationRegulatoryProgramId = 3,
            //    SubMissionDateTime = DateTime.UtcNow,
            //};

            var rpt = _dbContext.ReportPackages.Single(i => i.ReportPackageId == reportPackageId);
            var rptDto = new ReportPackageDto();
            rptDto = _mapHelper.GetReportPackageDtoFromReportPackage(rpt);
            rptDto.OrganizationRegulatoryProgramDto =
                    _programService.GetOrganizationRegulatoryProgram(rpt.OrganizationRegulatoryProgramId);

            rptDto.SubMissionDateTimeLocal = _timeZoneService
                .GetLocalizedDateTimeUsingSettingForThisOrg(rptDto.SubMissionDateTime, rptDto.OrganizationRegulatoryProgramId);

            return rptDto;
        }

        public CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId, ReportPackageDto reportPackageDto = null)
        {
            _logger.Info("Enter ReportPackageService.GetCopyOfRecordByReportPackageId. reportPackageId={0}", reportPackageId);

            if (reportPackageDto == null)
            {
                reportPackageDto = GetReportPackage(reportPackageId);
            }

            var copyOfRecordDto = _copyOfRecordService.GetCopyOfRecordByReportPackage(reportPackageDto);
            _logger.Info("Enter ReportPackageService.GetCopyOfRecordByReportPackageId. reportPackageId={0}", reportPackageId);

            return copyOfRecordDto;
        }

        public CopyOfRecordDto CreateCopyOfRecordForReportPackage(int reportPackageId)
        {
            _logger.Info("Enter ReportPackageService.CreateCopyOfRecordForReportPackage. reportPackageId={0}", reportPackageId);

            var attachments = GetReportPackageAttachments(reportPackageId);
            var copyOfRecordPdfFile = GetReportPackageCopyOfRecordPdfFile(reportPackageId);
            var copyOfRecordDataXmlFile = GetReportPackageCopyOfRecordDataXmlFile(reportPackageId);
            var copyOfRecordDto = _copyOfRecordService.CreateCopyOfRecordForReportPackage(reportPackageId, attachments, copyOfRecordPdfFile, copyOfRecordDataXmlFile);

            _logger.Info("Enter ReportPackageService.CreateCopyOfRecordForReportPackage. reportPackageId={0}", reportPackageId);

            return copyOfRecordDto;
        }

        private void SendSignAndSubmitEmail(ReportPackageDto reportPackage, CopyOfRecordDto copyOfRecordDto)
        {
            var emailContentReplacements = new Dictionary<string, string>();

            emailContentReplacements.Add("iuOrganizationName", reportPackage.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName);
            emailContentReplacements.Add("reportPackageName", reportPackage.Name);

            emailContentReplacements.Add("periodStartDate", reportPackage.PeriodStartDateTime.ToString("MMM dd, yyyy"));
            emailContentReplacements.Add("periodEndDate", reportPackage.PeriodEndDateTime.ToString("MMM dd, yyyy"));

            var submissionDateTime =
                $"{reportPackage.SubMissionDateTimeLocal.ToString("MMM dd, yyyy HHtt ")}{_timeZoneService.GetAbbreviationTimeZoneNameUsingSettingForThisOrg(reportPackage.OrganizationRegulatoryProgramId)}";

            emailContentReplacements.Add("submissionDateTime", submissionDateTime);
            emailContentReplacements.Add("corSignature", copyOfRecordDto.Signature);
            emailContentReplacements.Add("submitterFirstName", _httpContextService.GetClaimValue(CacheKey.FirstName));
            emailContentReplacements.Add("submitterLastName", _httpContextService.GetClaimValue(CacheKey.LastName));
            emailContentReplacements.Add("submitterTitle", _httpContextService.GetClaimValue(CacheKey.UserRole));

            emailContentReplacements.Add("permitNumber", reportPackage.OrganizationRegulatoryProgramDto.OrganizationDto.PermitNumber);

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

            // Send emails  
            // Get all signators for the program 
            var signatorsEmails = _userService.GetOrgRegProgSignators(reportPackage.OrganizationRegulatoryProgramId).Select(i => i.Email).ToList();
            signatorsEmails.Add("shuhao.wang@watertrax.com");
            _emailService.SendEmail(signatorsEmails, Core.Enum.EmailType.Report_Submission_IU, emailContentReplacements, false);

            // Get all Standard Users for the autority  
            var authorityOrganzationId = reportPackage.OrganizationRegulatoryProgramDto.OrganizationId;
            var authorityAdminAndStandardUsersEmails = _userService.GetAuthorityAdministratorAndStandardUsers(authorityOrganzationId).Select(i => i.Email).ToList();
            authorityAdminAndStandardUsersEmails.Add("shuhao.wang@watertrax.com");
            _emailService.SendEmail(authorityAdminAndStandardUsersEmails, Core.Enum.EmailType.Report_Submission_AU, emailContentReplacements, false);
        }
    }
}