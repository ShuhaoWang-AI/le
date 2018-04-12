using System.Configuration;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.CopyOfRecord;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Unit;
using Linko.LinkoExchange.Services.User;
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
    public class ReportTemplateServiceTests
    {
        #region fields

        private readonly Mock<IUserService> _userService = new Mock<IUserService>();

        private Mock<ICopyOfRecordService> _copyOfRecordService = new Mock<ICopyOfRecordService>();
        private Mock<IHttpContextService> _httpContext = new Mock<IHttpContextService>();
        private Mock<ILogger> _logger = new Mock<ILogger>();
        private Mock<IOrganizationService> _orgService = new Mock<IOrganizationService>();
        private Mock<IProgramService> _programService = new Mock<IProgramService>();
        private Mock<ISettingService> _settingService = new Mock<ISettingService>();
        private Mock<ITimeZoneService> _timeZoneService = new Mock<ITimeZoneService>();
        private ReportTemplateService _reportTemplateService;

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
            var actualSettingService = new SettingService(dbContext:connection, logger:_logger.Object, mapHelper:new MapHelper(), cache:new Mock<IRequestCache>().Object,
                                                          globalSettings:new Mock<IGlobalSettings>().Object);
            var actualTimeZoneService = new TimeZoneService(dbContext:connection, settings:actualSettingService, mapHelper:new MapHelper(),
                                                            appCache:new Mock<IApplicationCache>().Object, logger:_logger.Object);
            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns(value:"1");

            var authorityOrgRegProgramDto = new OrganizationRegulatoryProgramDto
                                            {
                                                OrganizationDto = new OrganizationDto
                                                                  {
                                                                      OrganizationId = 1000,
                                                                      OrganizationName = "Axys Chemicals",
                                                                      AddressLine1 = "1232 Johnson St.",
                                                                      AddressLine2 = "PO Box 1234",
                                                                      CityName = "Gotham",
                                                                      ZipCode = "90210"
                                                                  }
                                            };

            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>())).Returns(value:authorityOrgRegProgramDto);

            var actualUnitService = new UnitService(dbContext:connection, mapHelper:new MapHelper(), logger:_logger.Object, httpContextService:_httpContext.Object,
                                                    timeZoneService:actualTimeZoneService, orgService:_orgService.Object, settingService:actualSettingService,
                                                    requestCache:new Mock<IRequestCache>().Object);
            var actualSampleService = new SampleService(dbContext:connection, httpContext:_httpContext.Object, orgService:_orgService.Object, mapHelper:new MapHelper(),
                                                        logger:_logger.Object, timeZoneService:actualTimeZoneService, settings:actualSettingService, unitService:actualUnitService,
                                                        cache:new Mock<IApplicationCache>().Object);

            _reportTemplateService = new ReportTemplateService(
                                                               dbContext:connection,
                                                               httpContextService:_httpContext.Object,
                                                               userService:_userService.Object,
                                                               mapHelper:new MapHelper(),
                                                               logger:_logger.Object,
                                                               timeZoneService:actualTimeZoneService,
                                                               orgService:_orgService.Object
                                                              );
        }

        [TestMethod]
        public void GetReportPackageTemplates()
        {
            var templateDtos = _reportTemplateService.GetReportPackageTemplates(isForCreatingDraft:true, includeChildObjects:false);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetReportPackageTemplate_AsAuthority_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");

            var reportPackageTemplateId = 1;
            var isAuthorized = _reportTemplateService.CanUserExecuteApi("GetReportPackageTemplate", reportPackageTemplateId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetReportPackageTemplate_AsAuthority_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"99");

            var reportPackageTemplateId = 1;
            var isAuthorized = _reportTemplateService.CanUserExecuteApi("GetReportPackageTemplate", reportPackageTemplateId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetReportPackageTemplate_AsIndustry_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"2");

            var reportPackageTemplateId = 5;
            var isAuthorized = _reportTemplateService.CanUserExecuteApi("GetReportPackageTemplate", reportPackageTemplateId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetReportPackageTemplate_AsIndustry_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"2");

            var reportPackageTemplateId = 1;
            var isAuthorized = _reportTemplateService.CanUserExecuteApi("GetReportPackageTemplate", reportPackageTemplateId);

            Assert.IsFalse(condition:isAuthorized);
        }
    }
}