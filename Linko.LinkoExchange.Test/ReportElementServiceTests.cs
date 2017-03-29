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

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class ReportElementServiceTests
    {
        ReportElementService _reportElementService;
        Mock<IHttpContextService> _httpContext;
        Mock<IOrganizationService> _orgService;
        Mock<ILogger> _logger;
        Mock<ITimeZoneService> _timeZoneService;

        public ReportElementServiceTests()
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
            var actualTimeZoneService = new TimeZoneService(connection, new SettingService(connection, _logger.Object, new MapHelper()), new MapHelper());

            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns("1");
            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>())).Returns(new OrganizationRegulatoryProgramDto() { OrganizationRegulatoryProgramId = 1 });

            _reportElementService = new ReportElementService(connection, _httpContext.Object, _orgService.Object, new MapHelper(), _logger.Object, actualTimeZoneService);
        }


        /// <summary>
        /// Must throw RuleViolationException if missing Name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RuleViolationException))]
        public void SaveReportElementType_Test_Missing_Required_Name()
        {
            var reportElementTypeDto = new ReportElementTypeDto();
            _reportElementService.SaveReportElementType(reportElementTypeDto);
        }

        /// <summary>
        /// Must throw RuleViolationException if Certification and missing content
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RuleViolationException))]
        public void SaveReportElementType_Test_Missing_CertificationContent()
        {
            var reportElementTypeDto = new ReportElementTypeDto();
            reportElementTypeDto.Name = "Has Required Name";
            reportElementTypeDto.ReportElementCategory = ReportElementCategoryName.Certifications; //Certifications;
            //reportElementTypeDto.Content = NOT SET!
            _reportElementService.SaveReportElementType(reportElementTypeDto);
        }

        [TestMethod]
        public void SaveReportElementType_Test_Valid_Missing_Content_Attachment_CreateNew()
        {
            var reportElementTypeDto = new ReportElementTypeDto();

            reportElementTypeDto.Name = "Attachment Name";
            reportElementTypeDto.Description = "Attachment Description";
            //reportElementTypeDto.Content = MISSING
            reportElementTypeDto.IsContentProvided = false;
            reportElementTypeDto.CtsEventType = new CtsEventTypeDto() { CtsEventTypeId = 1 };
            reportElementTypeDto.ReportElementCategory = ReportElementCategoryName.Attachments; //Attachment;
            reportElementTypeDto.OrganizationRegulatoryProgramId = 1;

            _reportElementService.SaveReportElementType(reportElementTypeDto);
        }

        [TestMethod]
        public void SaveReportElementType_Test_Valid_CreateNew()
        {
            var reportElementTypeDto = new ReportElementTypeDto();

            reportElementTypeDto.Name = "Name";
            reportElementTypeDto.Description = "Description";
            reportElementTypeDto.Content = "This is sample certification text";
            reportElementTypeDto.IsContentProvided = true;
            reportElementTypeDto.CtsEventType = new CtsEventTypeDto() { CtsEventTypeId = 1 };
            reportElementTypeDto.ReportElementCategory = ReportElementCategoryName.Certifications;; //Certifications;
            reportElementTypeDto.OrganizationRegulatoryProgramId = 1;

            _reportElementService.SaveReportElementType(reportElementTypeDto);
        }

        [TestMethod]
        [ExpectedException(typeof(RuleViolationException))]
        public void SaveReportElementType_Test_DuplicateName_CreateNew()
        {
            var reportElementTypeDto = new ReportElementTypeDto();

            reportElementTypeDto.Name = "Name";
            reportElementTypeDto.Description = "Description";
            reportElementTypeDto.Content = "This is sample certification text";
            reportElementTypeDto.IsContentProvided = true;
            reportElementTypeDto.CtsEventType = new CtsEventTypeDto() { CtsEventTypeId = 1 };
            reportElementTypeDto.ReportElementCategory = ReportElementCategoryName.Certifications;; //Certifications;
            reportElementTypeDto.OrganizationRegulatoryProgramId = 1;

            _reportElementService.SaveReportElementType(reportElementTypeDto);
        }

        [TestMethod]
        public void SaveReportElementType_Test_UpdateExisting()
        {
            var reportElementTypeDto = new ReportElementTypeDto();

            reportElementTypeDto.ReportElementTypeId = 1; //Existing Id
            reportElementTypeDto.Name = "Name";
            reportElementTypeDto.Description = "Description";
            reportElementTypeDto.Content = "This is different sample certification text";
            reportElementTypeDto.IsContentProvided = true;
            reportElementTypeDto.CtsEventType = new CtsEventTypeDto() { CtsEventTypeId = 1 };
            reportElementTypeDto.ReportElementCategory = ReportElementCategoryName.Certifications; //Certifications;
            reportElementTypeDto.OrganizationRegulatoryProgramId = 1;

            _reportElementService.SaveReportElementType(reportElementTypeDto);
        }

        [TestMethod]
        public void GetReportElementTypes_Certifications_Test()
        {
            var certificationReportElementTypes = _reportElementService.GetReportElementTypes(ReportElementCategoryName.Certifications);
        }

        [TestMethod]
        public void GetReportElementTypes_Attachments_Test()
        {
            var attachmentReportElementTypes = _reportElementService.GetReportElementTypes(ReportElementCategoryName.Attachments);
        }

        [TestMethod]
        public void IsReportElementTypeInUse_Test_NotInUse()
        {
            var isInUse = _reportElementService.IsReportElementTypeInUse(1);

            Assert.IsFalse(isInUse);
        }

        [TestMethod]
        public void IsReportElementTypeInUse_Test_IsInUse()
        {
            var isInUse = _reportElementService.IsReportElementTypeInUse(2);

            Assert.IsTrue(isInUse);
        }

        [TestMethod]
        [ExpectedException(typeof(RuleViolationException))]
        public void DeleteReportElementType_Test_IsInUse()
        {
            _reportElementService.DeleteReportElementType(2);
        }

        [TestMethod]
        public void DeleteReportElementType_Test_IsNotInUse()
        {
            _reportElementService.DeleteReportElementType(1);
        }

        [TestMethod]
        public void GetReportElementType_Test_Valid()
        {
            var dto = _reportElementService.GetReportElementType(3);
        }
    }
}
