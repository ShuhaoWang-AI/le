using System.Configuration;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleStringLiteral

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class ReportElementServiceTests
    {
        #region fields

        private Mock<IHttpContextService> _httpContext;
        private Mock<ILogger> _logger;
        private Mock<IOrganizationService> _orgService;
        private ReportElementService _reportElementService;
        private Mock<ITimeZoneService> _timeZoneService;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            var connection = new LinkoExchangeContext(nameOrConnectionString:connectionString);
            _httpContext = new Mock<IHttpContextService>();
            _orgService = new Mock<IOrganizationService>();
            _logger = new Mock<ILogger>();
            _timeZoneService = new Mock<ITimeZoneService>();
            var actualTimeZoneService = new TimeZoneService(dbContext:connection,
                                                            settings:new SettingService(
                                                                                        dbContext:connection,
                                                                                        logger:_logger.Object,
                                                                                        mapHelper:new MapHelper(),
                                                                                        cache:new Mock<IRequestCache>().Object,
                                                                                        globalSettings:new Mock<IGlobalSettings>().Object
                                                                                       ),
                                                            mapHelper:new MapHelper(),
                                                            appCache:new Mock<IApplicationCache>().Object);

            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns(value:"1");
            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>())).Returns(value:new OrganizationRegulatoryProgramDto {OrganizationRegulatoryProgramId = 1});

            _reportElementService = new ReportElementService(dbContext:connection,
                                                             httpContext:_httpContext.Object,
                                                             orgService:_orgService.Object,
                                                             mapHelper:new MapHelper(),
                                                             logger:_logger.Object,
                                                             timeZoneService:actualTimeZoneService);
        }

        /// <summary>
        ///     Must throw RuleViolationException if missing Name
        /// </summary>
        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void SaveReportElementType_Test_Missing_Required_Name()
        {
            var reportElementTypeDto = new ReportElementTypeDto();
            _reportElementService.SaveReportElementType(reportElementType:reportElementTypeDto);
        }

        /// <summary>
        ///     Must throw RuleViolationException if Certification and missing content
        /// </summary>
        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void SaveReportElementType_Test_Missing_CertificationContent()
        {
            var reportElementTypeDto = new ReportElementTypeDto();
            reportElementTypeDto.Name = "Has Required Name";
            reportElementTypeDto.ReportElementCategory = ReportElementCategoryName.Certifications; //Certifications;

            //reportElementTypeDto.Content = NOT SET!
            _reportElementService.SaveReportElementType(reportElementType:reportElementTypeDto);
        }

        [TestMethod]
        public void SaveReportElementType_Test_Valid_Missing_Content_Attachment_CreateNew()
        {
            var reportElementTypeDto = new ReportElementTypeDto();

            reportElementTypeDto.Name = "Attachment Name";
            reportElementTypeDto.Description = "Attachment Description";

            //reportElementTypeDto.Content = MISSING
            reportElementTypeDto.IsContentProvided = false;
            reportElementTypeDto.CtsEventType = new CtsEventTypeDto {CtsEventTypeId = 1};
            reportElementTypeDto.ReportElementCategory = ReportElementCategoryName.Attachments; //Attachment;
            reportElementTypeDto.OrganizationRegulatoryProgramId = 1;

            _reportElementService.SaveReportElementType(reportElementType:reportElementTypeDto);
        }

        [TestMethod]
        public void SaveReportElementType_Test_Valid_CreateNew()
        {
            var reportElementTypeDto = new ReportElementTypeDto();

            reportElementTypeDto.Name = "Name";
            reportElementTypeDto.Description = "Description";
            reportElementTypeDto.Content = "This is sample certification text";
            reportElementTypeDto.IsContentProvided = true;
            reportElementTypeDto.CtsEventType = new CtsEventTypeDto {CtsEventTypeId = 1};
            reportElementTypeDto.ReportElementCategory = ReportElementCategoryName.Certifications; //Certifications;
            reportElementTypeDto.OrganizationRegulatoryProgramId = 1;

            _reportElementService.SaveReportElementType(reportElementType:reportElementTypeDto);
        }

        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void SaveReportElementType_Test_DuplicateName_CreateNew()
        {
            var reportElementTypeDto = new ReportElementTypeDto();

            reportElementTypeDto.Name = "Name";
            reportElementTypeDto.Description = "Description";
            reportElementTypeDto.Content = "This is sample certification text";
            reportElementTypeDto.IsContentProvided = true;
            reportElementTypeDto.CtsEventType = new CtsEventTypeDto {CtsEventTypeId = 1};
            reportElementTypeDto.ReportElementCategory = ReportElementCategoryName.Certifications; //Certifications;
            reportElementTypeDto.OrganizationRegulatoryProgramId = 1;

            _reportElementService.SaveReportElementType(reportElementType:reportElementTypeDto);
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
            reportElementTypeDto.CtsEventType = new CtsEventTypeDto {CtsEventTypeId = 1};
            reportElementTypeDto.ReportElementCategory = ReportElementCategoryName.Certifications; //Certifications;
            reportElementTypeDto.OrganizationRegulatoryProgramId = 1;

            _reportElementService.SaveReportElementType(reportElementType:reportElementTypeDto);
        }

        [TestMethod]
        public void GetReportElementTypes_Certifications_Test()
        {
            var certificationReportElementTypes = _reportElementService.GetReportElementTypes(categoryName:ReportElementCategoryName.Certifications);
        }

        [TestMethod]
        public void GetReportElementTypes_Attachments_Test()
        {
            var attachmentReportElementTypes = _reportElementService.GetReportElementTypes(categoryName:ReportElementCategoryName.Attachments);
        }

        [TestMethod]
        public void IsReportElementTypeInUse_Test_NotInUse()
        {
            var isInUse = _reportElementService.IsReportElementTypeInUse(reportElementTypeId:1);

            Assert.IsFalse(condition:isInUse);
        }

        [TestMethod]
        public void IsReportElementTypeInUse_Test_IsInUse()
        {
            var isInUse = _reportElementService.IsReportElementTypeInUse(reportElementTypeId:2);

            Assert.IsTrue(condition:isInUse);
        }

        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void DeleteReportElementType_Test_IsInUse()
        {
            _reportElementService.DeleteReportElementType(reportElementTypeId:2);
        }

        [TestMethod]
        public void DeleteReportElementType_Test_IsNotInUse()
        {
            _reportElementService.DeleteReportElementType(reportElementTypeId:1);
        }

        [TestMethod]
        public void GetReportElementType_Test_Valid()
        {
            var dto = _reportElementService.GetReportElementType(reportElementTypeId:3);
        }
    }
}