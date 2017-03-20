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
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Organization;
using System;
using Linko.LinkoExchange.Services.Parameter;
using Linko.LinkoExchange.Services.TimeZone;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class ParameterServiceTests
    {
        ParameterService _paramService;
        Mock<IHttpContextService> _httpContext;
        Mock<IOrganizationService> _orgService;
        Mock<ILogger> _logger;
        Mock<TimeZoneService> _timeZoneService;

        public ParameterServiceTests()
        {
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _httpContext = new Mock<IHttpContextService>();
            _orgService = new Mock<IOrganizationService>();
            _logger = new Mock<ILogger>();
            _timeZoneService = new Mock<TimeZoneService>();

            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns("1");
            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>())).Returns(new OrganizationRegulatoryProgramDto() { OrganizationRegulatoryProgramId = 1 });

            _paramService = new ParameterService(new LinkoExchangeContext(connectionString), 
                _httpContext.Object, 
                _orgService.Object, 
                new MapHelper(), 
                _logger.Object,
                _timeZoneService.Object);
        }


        /// <summary>
        /// Must throw RuleViolationException if missing Name
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RuleViolationException))]
        public void SaveParameterGroup_Test_Missing_Required_Name()
        {
            var paramGroupDto = new ParameterGroupDto();
            _paramService.SaveParameterGroup(paramGroupDto);
        }

      

    }
}
