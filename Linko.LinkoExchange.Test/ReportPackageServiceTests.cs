using System.Collections.Generic;
using System.Configuration;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Organization;
using System;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services.CopyOfRecord;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.Unit;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Cache;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class ReportPackageServiceTests
    {
        ReportPackageService _reportPackageService;
        Mock<IProgramService> _programService = new Mock<IProgramService>();
        Mock<ICopyOfRecordService> _copyOfRecordService = new Mock<ICopyOfRecordService>();
        Mock<ITimeZoneService> _timeZoneService = new Mock<ITimeZoneService>();
        Mock<ILogger> _logger = new Mock<ILogger>();
        Mock<IHttpContextService> _httpContext = new Mock<IHttpContextService>();
        Mock<IUserService> _userService = new Mock<IUserService>();
        Mock<IEmailService> _emailService = new Mock<IEmailService>();
        Mock<ISettingService> _settingService = new Mock<ISettingService>();
        Mock<IOrganizationService> _orgService = new Mock<IOrganizationService>();
        Mock<ICromerrAuditLogService> _cromerrService = new Mock<ICromerrAuditLogService>();
        Mock<IRequestCache> _requestCache = new Mock<IRequestCache>();
        Mock<IAuditLogService> _auditLogService = new Mock<IAuditLogService>();

        public ReportPackageServiceTests()
        {
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            var connection = new LinkoExchangeContext(connectionString);
            _httpContext = new Mock<IHttpContextService>();
            _orgService = new Mock<IOrganizationService>();
            _logger = new Mock<ILogger>();
            _timeZoneService = new Mock<ITimeZoneService>();
            var actualSettingService = new SettingService(connection, _logger.Object, new MapHelper());
            var actualTimeZoneService = new TimeZoneService(connection, actualSettingService, new MapHelper());
            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns("1");

            var authorityOrgRegProgramDto = new OrganizationRegulatoryProgramDto();
            authorityOrgRegProgramDto.OrganizationDto = new OrganizationDto();
            authorityOrgRegProgramDto.OrganizationRegulatoryProgramId = 1;
            authorityOrgRegProgramDto.OrganizationDto.OrganizationId = 1000;
            authorityOrgRegProgramDto.OrganizationDto.OrganizationName = "Axys Chemicals";
            authorityOrgRegProgramDto.OrganizationDto.AddressLine1 = "1232 Johnson St.";
            authorityOrgRegProgramDto.OrganizationDto.AddressLine2 = "PO Box 1234";
            authorityOrgRegProgramDto.OrganizationDto.CityName = "Gotham";
            authorityOrgRegProgramDto.OrganizationDto.ZipCode = "90210";

            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>())).Returns(authorityOrgRegProgramDto);

            _settingService.Setup(s => s.GetOrganizationSettingValue(It.IsAny<int>(), SettingType.ReportRepudiatedDays)).Returns("180");
            _settingService.Setup(s => s.GetOrganizationSettingValue(It.IsAny<int>(), SettingType.TimeZone)).Returns("6");

            _settingService.Setup(s => s.GetOrgRegProgramSettingValue(It.IsAny<int>(), SettingType.EmailContactInfoName)).Returns("Email C Name");
            _settingService.Setup(s => s.GetOrgRegProgramSettingValue(It.IsAny<int>(), SettingType.EmailContactInfoEmailAddress)).Returns("contactemail@auth.com");
            _settingService.Setup(s => s.GetOrgRegProgramSettingValue(It.IsAny<int>(), SettingType.EmailContactInfoPhone)).Returns("(555) 555-5555");

            var systemSettingLookup = new Dictionary<SystemSettingType, string>();
            systemSettingLookup.Add(SystemSettingType.SystemEmailEmailAddress, "donteventrytoreply@linkotechnology.com");
            systemSettingLookup.Add(SystemSettingType.SystemEmailFirstName, "Adam");
            systemSettingLookup.Add(SystemSettingType.SystemEmailLastName, "Adminsky");
            //systemSettingLookup.Add(SystemSettingType.EmailServer, "wtraxadc2.watertrax.local");
            systemSettingLookup.Add(SystemSettingType.EmailServer, "192.168.5.51");
            systemSettingLookup.Add(SystemSettingType.FileAvailableToAttachMaxAgeMonths, "999");
            _settingService.Setup(s => s.GetGlobalSettings()).Returns(systemSettingLookup);

            _requestCache.Setup(s => s.GetValue(CacheKey.Token)).Returns("some_token_string");

            var actualUnitService = new UnitService(connection, new MapHelper(), _logger.Object, _httpContext.Object, actualTimeZoneService, _orgService.Object, actualSettingService);
            var actualSampleService = new SampleService(connection, _httpContext.Object, _orgService.Object, new MapHelper(), _logger.Object, actualTimeZoneService, actualSettingService, actualUnitService);

            var actualAuditLogService = new EmailAuditLogService(connection, _requestCache.Object, new MapHelper());
            var actualEmailService = new LinkoExchangeEmailService(connection, actualAuditLogService, _programService.Object, _settingService.Object, _requestCache.Object);
            var actualCromerrService = new CromerrAuditLogService(connection, _requestCache.Object, new MapHelper(), _httpContext.Object, _logger.Object);
            var actualProgramService = new ProgramService(connection, new MapHelper());

            _reportPackageService = new ReportPackageService(
                actualProgramService,
                _copyOfRecordService.Object,
                actualTimeZoneService,
                _logger.Object,
                connection,
                _httpContext.Object,
                _userService.Object,
                actualEmailService,
                _settingService.Object,
                _orgService.Object,
                actualSampleService,
                new MapHelper(),
                actualCromerrService
            );
        }

        [TestMethod]
        public void DeleteReportPackage()
        {
            _reportPackageService.DeleteReportPackage(14);
            //_reportPackageService.DeleteReportPackage(9);
            //_reportPackageService.DeleteReportPackage(10);
        }

        [TestMethod]
        public void CreateDraft()
        {
            var templateId = 1;
            var startDateTimeLocal = new DateTime(2017, 4, 20);
            var endDateTimeLocal = new DateTime(2017, 4, 28);
            var newId = _reportPackageService.CreateDraft(templateId, startDateTimeLocal, endDateTimeLocal);
        }

        [TestMethod]
        public void CreateDraft_Using_Date_Plus_Time()
        {
            var templateId = 1;
            var startDateTimeLocal = new DateTime(2017, 4, 20, 17, 30, 30, 30);
            var endDateTimeLocal = new DateTime(2017, 4, 20, 17, 30, 30, 30);
            var newId = _reportPackageService.CreateDraft(templateId, startDateTimeLocal, endDateTimeLocal);
        }

        [TestMethod]
        public void CreateDraft_Using_Empty_Template()
        {
            var emptyReportPackageTemplateId = 9;
            var startDateTimeLocal = new DateTime(2017, 4, 20);
            var endDateTimeLocal = new DateTime(2017, 4, 28);
            var newId = _reportPackageService.CreateDraft(emptyReportPackageTemplateId, startDateTimeLocal, endDateTimeLocal);
        }

        [TestMethod]
        public void CreateDraft_Using_Certification_Only__Template()
        {
            var emptyReportPackageTemplateId = 10;
            var startDateTimeLocal = new DateTime(2017, 4, 20);
            var endDateTimeLocal = new DateTime(2017, 4, 28);
            var newId = _reportPackageService.CreateDraft(emptyReportPackageTemplateId, startDateTimeLocal, endDateTimeLocal);
        }

        [TestMethod]
        public void GetReportPackage()
        {
            //Fetch existing
            var existingReportPackage = _reportPackageService.GetReportPackage(13, false);
        }

        [TestMethod]
        [ExpectedException(typeof(UnauthorizedAccessException))]
        public void GetReportPackage_UnauthorizedAccessException()
        {
            //Fetch existing
            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns("4");
            var existingReportPackage = _reportPackageService.GetReportPackage(8, false);
        }

        [TestMethod]
        public void GetReportPackage_With_Associated_Element_Children()
        {
            //Fetch existing
            var existingReportPackage = _reportPackageService.GetReportPackage(21, true);
        }

        [TestMethod]
        public void SaveReportPackage_Add_Samples()
        {
            //Fetch existing
            var existingReportPackage = _reportPackageService.GetReportPackage(13, false);

            //Add sample associations
            existingReportPackage.SamplesAndResultsTypes = new List<ReportPackageElementTypeDto>();
            var sampleReportPackageElementType = new ReportPackageElementTypeDto() { ReportPackageElementTypeId = 25 };
            sampleReportPackageElementType.Samples = new List<SampleDto>();
            sampleReportPackageElementType.Samples.Add(new SampleDto { SampleId = 52 });
            sampleReportPackageElementType.Samples.Add(new SampleDto { SampleId = 53 });
            existingReportPackage.SamplesAndResultsTypes.Add(sampleReportPackageElementType);
            var existingId = _reportPackageService.SaveReportPackage(existingReportPackage, true);
        }

        [TestMethod]
        public void SaveReportPackage_Add_Files()
        {
            //Fetch existing
            var existingReportPackage = _reportPackageService.GetReportPackage(15, true);

            existingReportPackage.Comments = "Comments .. test.";

            //Add sample associations
            existingReportPackage.SamplesAndResultsTypes = new List<ReportPackageElementTypeDto>();
            var sampleReportPackageElementType = new ReportPackageElementTypeDto() { ReportPackageElementTypeId = 25 };
            sampleReportPackageElementType.Samples = new List<SampleDto>();
            sampleReportPackageElementType.Samples.Add(new SampleDto { SampleId = 52 });
            //sampleReportPackageElementType.Samples.Add(new SampleDto { SampleId = 53 });
            existingReportPackage.SamplesAndResultsTypes.Add(sampleReportPackageElementType);

            //Add attachment associations
            existingReportPackage.AttachmentTypes = new List<ReportPackageElementTypeDto>();
            var attachmentReportPackageElementType = new ReportPackageElementTypeDto() { ReportPackageElementTypeId = 26 };
            attachmentReportPackageElementType.FileStores = new List<FileStoreDto>();
            attachmentReportPackageElementType.FileStores.Add(new FileStoreDto { FileStoreId = 2 });
            //filesReportPackageElementType.FileStores.Add(new FileStoreDto { FileStoreId = 4 });
            existingReportPackage.AttachmentTypes.Add(attachmentReportPackageElementType);

            //Add certification associations
            existingReportPackage.CertificationTypes = new List<ReportPackageElementTypeDto>();
            var certsReportPackageElementType = new ReportPackageElementTypeDto() { ReportPackageElementTypeId = 34 };
            certsReportPackageElementType.FileStores = new List<FileStoreDto>();
            certsReportPackageElementType.FileStores.Add(new FileStoreDto { FileStoreId = 2 });
            //filesReportPackageElementType.FileStores.Add(new FileStoreDto { FileStoreId = 4 });
            existingReportPackage.CertificationTypes.Add(certsReportPackageElementType);

            var existingId = _reportPackageService.SaveReportPackage(existingReportPackage, true);
        }


        [TestMethod]
        public void UpdateStatus()
        {
            //Change status
            _reportPackageService.UpdateStatus(8, ReportStatusName.Submitted, false);

        }

        [TestMethod]
        public void GetFilesForSelection()
        {
            var eligibleFiles = _reportPackageService.GetFilesForSelection(26);

        }

        [TestMethod]
        public void GetSamplesForSelection()
        {
            var eligibleSamples = _reportPackageService.GetSamplesForSelection(1);

        }

        [TestMethod]
        public void GetReportPackagesByStatusName_Draft()
        {
            var filteredReportPackages = _reportPackageService.GetReportPackagesByStatusName(ReportStatusName.RepudiatedPendingReview);
        }

        [TestMethod]
        public void RepudiateReport()
        {
            //RepudiationReasonId     Name
            //1                       I did not submit this report
            //2                       Report is missing a sample
            //3                       Report is missing a parameter
            //4                       Report has errors
            //5                       A hold time was exceeded
            //6                       Other(please comment)

            _reportPackageService.RepudiateReport(21, 6, "Other(please comment)", "Technical error");

        }

        [TestMethod]
        public void ReviewSubmission()
        {
            _reportPackageService.ReviewSubmission(8);
        }

        [TestMethod]
        public void ReviewRepudiation()
        {
            _reportPackageService.ReviewRepudiation(8, "This repudiation has been reviewed!");
        }

        [TestMethod]
        public void IsRequiredReportPackageElementTypesIncluded()
        {
            var isGood = _reportPackageService.IsRequiredReportPackageElementTypesIncluded(11);
        }

        [TestMethod]
        public void Test_CreatePDF()
        {
            var templateId = 1;
            var startDateTimeLocal = new DateTime(2017, 4, 20);
            var endDateTimeLocal = new DateTime(2018, 4, 28);
            var reportPackageId = _reportPackageService.CreateDraft(templateId, startDateTimeLocal, endDateTimeLocal);


        }

        [TestMethod]
        public void UpdateLastSentDateTime()
        {
            _reportPackageService.UpdateLastSentDateTime(1, DateTimeOffset.Now, 1, "Testing First Name");
        }

        [TestMethod]
        public void IsSimilarReportPackageSubmittedAfter()
        {
            var result = _reportPackageService.IsSimilarReportPackageSubmittedAfter(4);
        }

        [TestMethod]
        public void CanRepudiateReportPackage()
        {
            var result = _reportPackageService.CanRepudiateReportPackage(4);
        }

    }
}
