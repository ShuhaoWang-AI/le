using System.Collections.Generic;
using System.Configuration;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
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
using Linko.LinkoExchange.Services.Config;

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
        Mock<IConfigSettingService> _configService = new Mock<IConfigSettingService>();

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
            authorityOrgRegProgramDto.OrganizationDto.OrganizationId = 1000;
            authorityOrgRegProgramDto.OrganizationDto.OrganizationName = "Axys Chemicals";
            authorityOrgRegProgramDto.OrganizationDto.AddressLine1 = "1232 Johnson St.";
            authorityOrgRegProgramDto.OrganizationDto.AddressLine2 = "PO Box 1234";
            authorityOrgRegProgramDto.OrganizationDto.CityName = "Gotham";
            authorityOrgRegProgramDto.OrganizationDto.ZipCode = "90210";

            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>())).Returns(authorityOrgRegProgramDto);

            _timeZoneService.Setup(s => s.GetUTCDateTimeUsingThisTimeZoneId(It.IsAny<DateTime>(), It.IsAny<int>())).Returns(DateTimeOffset.UtcNow);

            _configService.Setup(s => s.GetConfigValue(It.IsAny<string>())).Returns("16");

            var actualUnitService = new UnitService(connection, new MapHelper(), _logger.Object, _httpContext.Object, actualTimeZoneService, _orgService.Object, actualSettingService);
            var actualSampleService = new SampleService(connection, _httpContext.Object, _orgService.Object, new MapHelper(), _logger.Object, actualTimeZoneService, actualSettingService, actualUnitService);

            _reportPackageService = new ReportPackageService(
                _programService.Object,
                _copyOfRecordService.Object,
                actualTimeZoneService,
                _logger.Object,
                connection,
                _httpContext.Object,
                _userService.Object,
                _emailService.Object,
                actualSettingService,
                _orgService.Object,
                actualSampleService,
                new MapHelper()
            );
        }

        [TestMethod]
        public void DeleteReportPackage()
        {
            var reportPackageId = 7;
            _reportPackageService.DeleteReportPackage(reportPackageId);
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
        public void GetReportPackage()
        {
            //Fetch existing
            var existingReportPackage = _reportPackageService.GetReportPackage(8, false);
        }

        [TestMethod]
        public void GetReportPackage_With_Associated_Element_Children()
        {
            //Fetch existing
            var existingReportPackage = _reportPackageService.GetReportPackage(8, true);
        }

        [TestMethod]
        public void SaveReportPackage_Add_Samples()
        {
            //Fetch existing
            var existingReportPackage = _reportPackageService.GetReportPackage(2, false);

            //Add sample associations
            existingReportPackage.AssociatedSamples = new List<ReportSampleDto>();
            existingReportPackage.AssociatedSamples.Add(new ReportSampleDto { SampleId = 35, ReportPackageElementTypeId = 1 });

            var existingId = _reportPackageService.SaveReportPackage(existingReportPackage);
        }

        [TestMethod]
        public void SaveReportPackage_Add_Files()
        {
            //Fetch existing
            var existingReportPackage = _reportPackageService.GetReportPackage(8, false);

            existingReportPackage.Comments = "TESTING COMMENTS!";

            //Add sample associations
            existingReportPackage.AssociatedSamples = new List<ReportSampleDto>();
            //existingReportPackage.AssociatedSamples.Add(new ReportSampleDto { SampleId = 36, ReportPackageElementTypeId = 13 });
            existingReportPackage.AssociatedSamples.Add(new ReportSampleDto { SampleId = 38, ReportPackageElementTypeId = 13 });

            //Add file associations
            existingReportPackage.AssociatedFiles = new List<ReportFileDto>();
            //existingReportPackage.AssociatedFiles.Add(new ReportFileDto { FileStoreId = 2, ReportPackageElementTypeId = 14 });
            //existingReportPackage.AssociatedFiles.Add(new ReportFileDto { FileStoreId = 4, ReportPackageElementTypeId = 14 });

            var existingId = _reportPackageService.SaveReportPackage(existingReportPackage);
        }


        [TestMethod]
        public void UpdateStatus()
        {
            //Change status
            _reportPackageService.UpdateStatus(2, ReportStatusName.Submitted, false);

        }

        [TestMethod]
        public void GetFilesForSelection()
        {
            var eligibleFiles = _reportPackageService.GetFilesForSelection(8);

        }

        [TestMethod]
        public void GetSamplesForSelection()
        {
            var eligibleSamples = _reportPackageService.GetSamplesForSelection(8);

        }

        //
        [TestMethod]
        public void GetReportPackagesByStatusName_Draft()
        {
            var filteredReportPackages = _reportPackageService.GetReportPackagesByStatusName(ReportStatusName.Draft);
        }

    }
}
