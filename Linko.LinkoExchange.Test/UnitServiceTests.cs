using System.Configuration;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Config;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Unit;
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
    public class UnitServiceTests
    {
        #region fields

        private Mock<IConfigSettingService> _configService;
        private Mock<IHttpContextService> _httpContext;
        private Mock<ILogger> _logger;
        private Mock<IOrganizationService> _orgService;
        private Mock<ISettingService> _settings;
        private Mock<ITimeZoneService> _timeZones;
        private UnitService _unitService;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            _logger = new Mock<ILogger>();
            _httpContext = new Mock<IHttpContextService>();
            _timeZones = new Mock<ITimeZoneService>();
            _orgService = new Mock<IOrganizationService>();
            _configService = new Mock<IConfigSettingService>();

            _settings = new Mock<ISettingService>();
            _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>())).Returns(value:"gpd,mgd");

            _httpContext.Setup(i => i.GetClaimValue(It.IsAny<string>())).Returns(value:"1");
            _orgService.Setup(i => i.GetAuthority(It.IsAny<int>())).Returns(value:new OrganizationRegulatoryProgramDto {OrganizationId = 1000});

            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            _unitService = new UnitService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                           mapHelper:new MapHelper(),
                                           logger:_logger.Object,
                                           httpContextService:_httpContext.Object,
                                           timeZoneService:_timeZones.Object,
                                           orgService:_orgService.Object,
                                           settingService:_settings.Object,
                                           requestCache:new Mock<IRequestCache>().Object);
        }

        [TestMethod]
        public void GetFlowUnitValidValues()
        {
            var dtos = _unitService.GetFlowUnitValidValues();
        }

        [TestMethod]
        public void GetFlowUnitsFromCommaDelimitedString()
        {
            var dtos = _unitService.GetFlowUnitsFromCommaDelimitedString(commaDelimitedString:"gpd,mgd");
        }

        [TestMethod]
        public void ConvertResultToTargetUnit()
        {
            var currentAuthorityUnit = _unitService.GetUnit(7); // mg/L
            var targetAuthorityUnit = _unitService.GetUnit(13); // ug/L
            var result = 0.001;

            Assert.AreEqual("mg/L", currentAuthorityUnit.Name);
            Assert.AreEqual("ug/L", targetAuthorityUnit.Name);

            Assert.AreEqual(expected:result, actual:_unitService.ConvertResultToTargetUnit(result, currentAuthorityUnit, currentAuthorityUnit));
            Assert.AreEqual(expected:result, actual:_unitService.ConvertResultToTargetUnit(result, targetAuthorityUnit, targetAuthorityUnit));
            Assert.AreEqual(expected:result,
                            actual:_unitService.ConvertResultToTargetUnit(_unitService.ConvertResultToTargetUnit(result, currentAuthorityUnit, targetAuthorityUnit),
                                                                          targetAuthorityUnit, currentAuthorityUnit));
            
            Assert.AreEqual(expected:1.00, actual:_unitService.ConvertResultToTargetUnit(result, currentAuthorityUnit, targetAuthorityUnit));
            Assert.AreEqual(expected:100000.00, actual:_unitService.ConvertResultToTargetUnit(100.00, currentAuthorityUnit, targetAuthorityUnit));
        }
    }
}