using System.Configuration;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.TimeZone;
using System;
using Moq;
using NLog;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Unit;
using Linko.LinkoExchange.Services.Config;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class UnitServiceTests
    {
        private UnitService _unitService;
        Mock<ISettingService> _settings;
        Mock<ILogger> _logger;
        Mock<IHttpContextService> _httpContext;
        Mock<ITimeZoneService> _timeZones;
        Mock<IOrganizationService> _orgService;
        Mock<IConfigSettingService> _configService;

        public UnitServiceTests()
        {
           
        }

        [TestInitialize]
        public void Initialize()
        {
            _logger = new Mock<ILogger>();
            _httpContext = new Mock<IHttpContextService>();
            _timeZones = new Mock<ITimeZoneService>();
            _orgService = new Mock<IOrganizationService>();
            _configService = new Mock<IConfigSettingService>();

            _settings = new Mock<ISettingService>();
            _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<SettingType>())).Returns("gpd,mgd");

            _httpContext.Setup(i => i.GetClaimValue(It.IsAny<string>())).Returns("1");
            _orgService.Setup(i => i.GetAuthority(It.IsAny<int>())).Returns(new OrganizationRegulatoryProgramDto() { OrganizationId = 1000} );


            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _unitService = new UnitService(new LinkoExchangeContext(connectionString),
                new MapHelper(),
                _logger.Object,
                _httpContext.Object,
                _timeZones.Object,
                _orgService.Object,
                _settings.Object);
        }

        [TestMethod]
        public void GetFlowUnitValidValues()
        {
            var dtos = _unitService.GetFlowUnitValidValues();
        }

        [TestMethod]
        public void GetFlowUnitsFromCommaDelimitedString()
        {
            var dtos = _unitService.GetFlowUnitsFromCommaDelimitedString("gpd,mgd");
        }


    }
}
