using System.Collections.Generic;
using System.Configuration;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using System;
using Linko.LinkoExchange.Services.Parameter;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.HttpContext;

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
        Mock<ISettingService> _settingsService;
        Mock<IRequestCache> _mockRequestCache;
        Mock<IApplicationCache> _mockAppCache;

        public ParameterServiceTests()
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
            _settingsService = new Mock<ISettingService>();

            _mockRequestCache = new Mock<IRequestCache>();
            _mockRequestCache.Setup(c => c.GetValue("1-TimeZone")).Returns("6");
            _mockRequestCache.Setup(c => c.GetValue("VolumeFlowRateLimitBasisId")).Returns("3");

            _mockAppCache = new Mock<IApplicationCache>();
            _mockAppCache.Setup(c => c.Get("TimeZoneId_6")).Returns("Eastern Standard Time");

            var actualTimeZoneService = new TimeZoneService(connection, new SettingService(connection, _logger.Object, new MapHelper(), _mockRequestCache.Object, new Mock<IGlobalSettings>().Object), new MapHelper(), _mockAppCache.Object);
            var actualSettings = new SettingService(connection, _logger.Object, new MapHelper(), _mockRequestCache.Object, new Mock<IGlobalSettings>().Object);

            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns("1");
            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>())).Returns(new OrganizationRegulatoryProgramDto() { OrganizationRegulatoryProgramId = 1 });

            _paramService = new ParameterService(connection, 
                _httpContext.Object, 
                _orgService.Object, 
                new MapHelper(), 
                _logger.Object,
                actualTimeZoneService,
                actualSettings);
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
            paramGroupDto.Name = "Parameter Group ABC";
            paramGroupDto.Description = "Some description";
            paramGroupDto.IsActive = true;
            paramGroupDto.Parameters = new List<ParameterDto>();
            paramGroupDto.Parameters.Add(new ParameterDto() { ParameterId = 1 });
            paramGroupDto.Parameters.Add(new ParameterDto() { ParameterId = 2 });
            paramGroupDto.OrganizationRegulatoryProgramId = 1;
            var newId = _paramService.SaveParameterGroup(paramGroupDto);
        }

        [TestMethod]
        [ExpectedException(typeof(RuleViolationException))]
        public void SaveParameterGroup_Test_CreateNew_DuplicateName()
        {
            var paramGroupDto = new ParameterGroupDto();
            paramGroupDto.Name = "Some Name";
            paramGroupDto.Parameters = new List<ParameterDto>();
            paramGroupDto.Parameters.Add(new ParameterDto() { ParameterId = 1 });
            paramGroupDto.OrganizationRegulatoryProgramId = 1;
            _paramService.SaveParameterGroup(paramGroupDto);
        }

        [TestMethod]
        public void SaveParameterGroup_Test_Valid_Update()
        {
            var paramGroupDto = new ParameterGroupDto() { ParameterGroupId = 4 };
            paramGroupDto.Name = "Parameter Group A+";
            paramGroupDto.Description = "Sample desc";
            paramGroupDto.Parameters = new List<ParameterDto>();
            paramGroupDto.Parameters.Add(new ParameterDto() { ParameterId = 1 });
            paramGroupDto.OrganizationRegulatoryProgramId = 1;
            var existingId = _paramService.SaveParameterGroup(paramGroupDto);
        }

        [TestMethod]
        public void SaveParameterGroup_Test_Change_Parameter_Update()
        {
            var paramGroupDto = new ParameterGroupDto() { ParameterGroupId = 7 };
            paramGroupDto.Name = "Some Name Changed Again";
            paramGroupDto.Description = "Sample desc";
            paramGroupDto.Parameters = new List<ParameterDto>();
            paramGroupDto.Parameters.Add(new ParameterDto() { ParameterId = 2 });
            paramGroupDto.OrganizationRegulatoryProgramId = 1;
            _paramService.SaveParameterGroup(paramGroupDto);
        }

        [TestMethod]
        public void SaveParameterGroup_Test_Multiple_UniqueParameters_Update()
        {
            var paramGroupDto = new ParameterGroupDto() { ParameterGroupId = 7 };
            paramGroupDto.Name = "Some Name Changed Again";
            paramGroupDto.Description = "Sample desc";
            paramGroupDto.Parameters = new List<ParameterDto>();
            paramGroupDto.Parameters.Add(new ParameterDto() { ParameterId = 1 });
            paramGroupDto.Parameters.Add(new ParameterDto() { ParameterId = 2 });
            paramGroupDto.OrganizationRegulatoryProgramId = 1;
            _paramService.SaveParameterGroup(paramGroupDto);
        }

        [TestMethod]
        [ExpectedException(typeof(RuleViolationException))]
        public void SaveParameterGroup_Test_Multiple_DuplicateParameters_Update()
        {
            var paramGroupDto = new ParameterGroupDto() { ParameterGroupId = 4 };
            paramGroupDto.Name = "Some Name Changed Again";
            paramGroupDto.Description = "Sample desc";
            paramGroupDto.Parameters = new List<ParameterDto>();
            paramGroupDto.Parameters.Add(new ParameterDto() { ParameterId = 2 });
            paramGroupDto.Parameters.Add(new ParameterDto() { ParameterId = 2 });
            paramGroupDto.OrganizationRegulatoryProgramId = 1;
            _paramService.SaveParameterGroup(paramGroupDto);
        }

        [TestMethod]
        public void SaveParameterGroup_Test_Inline_Updating_Of_Parmeters()
        {
            var totalMetalsParameterGroup = _paramService.GetParameterGroup(1);

            //Remove the first one
            ParameterDto parameterToRemove = null;
            foreach (var parameter in totalMetalsParameterGroup.Parameters)
            {
                parameterToRemove = parameter;
                break;
            }
            totalMetalsParameterGroup.Parameters.Remove(parameterToRemove);

            //Add new one
            totalMetalsParameterGroup.Parameters.Add(new ParameterDto { ParameterId = 2 });

            _paramService.SaveParameterGroup(totalMetalsParameterGroup);
        }

        [TestMethod]
        public void GetParameterGroup_Test()
        {
            var paramGroupDto = _paramService.GetParameterGroup(3, false);
        }

        [TestMethod]
        public void GetStaticParameterGroups_Test()
        {
            var staticParameterGroups = _paramService.GetStaticParameterGroups(isGetActiveOnly: true);
        }

        [TestMethod]
        public void DeleteParameterGroup_Test()
        {
            _paramService.DeleteParameterGroup(7);
            //_paramService.DeleteParameterGroup(5);
            //_paramService.DeleteParameterGroup(6);
        }

        [TestMethod]
        public void GetGlobalParameters_Test()
        {
            var resultDtos = _paramService.GetGlobalParameters();
        }

        [TestMethod]
        public void GetGlobalParameters_StartsWith_All_Caps_Test()
        {
            var resultDtos = _paramService.GetGlobalParameters("CHLOR");
        }

        [TestMethod]
        public void GetGlobalParameters_StartsWith_All_LowerCase_Test()
        {
            var resultDtos = _paramService.GetGlobalParameters("chlor");
        }

        [TestMethod]
        public void GetGlobalParameters_StartsWith_All_MixedCase_Test()
        {
            var resultDtos = _paramService.GetGlobalParameters("chLOr");
        }

        [TestMethod]
        public void GetGlobalParameters_StartsWith_All_Leading_Whitespace_Test()
        {
            var resultDtos = _paramService.GetGlobalParameters("  Chlor");
        }

        [TestMethod]
        public void GetGlobalParameters_StartsWith_All_Alphanumeric_Test()
        {
            var resultDtos = _paramService.GetGlobalParameters("1,2");
        }

        [TestMethod]
        public void GetAllParameterGroups_Test()
        {
            var resultDtos = _paramService.GetAllParameterGroups(129, new DateTime(1997, 3, 31));
        }




    }
}
