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
        Mock<ITimeZoneService> _timeZoneService;

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
            _timeZoneService = new Mock<ITimeZoneService>();

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

        /// <summary>
        /// Must throw RuleViolationException if empty Parameter list (or null)
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RuleViolationException))]
        public void SaveParameterGroup_Test_Parameter_List_Is_Null()
        {
            var paramGroupDto = new ParameterGroupDto();
            paramGroupDto.Name = "Some Name";
            //paramGroupDto.Parameters = ???
            _paramService.SaveParameterGroup(paramGroupDto);
        }

        /// <summary>
        /// Must throw RuleViolationException if empty Parameter list (or null)
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(RuleViolationException))]
        public void SaveParameterGroup_Test_Empty_Parameters_List()
        {
            var paramGroupDto = new ParameterGroupDto();
            paramGroupDto.Name = "Some Name";
            paramGroupDto.Parameters = new List<ParameterDto>();
            _paramService.SaveParameterGroup(paramGroupDto);
        }

        [TestMethod]
        public void SaveParameterGroup_Test_Valid_CreateNew()
        {
            var paramGroupDto = new ParameterGroupDto();
            paramGroupDto.Name = "Some Name";
            paramGroupDto.Parameters = new List<ParameterDto>();
            paramGroupDto.Parameters.Add(new ParameterDto() { });
            paramGroupDto.OrganizationRegulatoryProgramId = 1;
            _paramService.SaveParameterGroup(paramGroupDto);
        }


    }
}
