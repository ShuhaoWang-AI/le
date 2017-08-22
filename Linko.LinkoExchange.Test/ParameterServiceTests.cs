using System;
using System.Collections.Generic;
using System.Configuration;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Parameter;
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
    public class ParameterServiceTests
    {
        #region fields

        private Mock<IHttpContextService> _httpContext;
        private Mock<ILogger> _logger;
        private Mock<IApplicationCache> _mockAppCache;
        private Mock<IRequestCache> _mockRequestCache;
        private Mock<IOrganizationService> _orgService;
        private ParameterService _paramService;
        private Mock<ISettingService> _settingsService;
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
            _settingsService = new Mock<ISettingService>();

            _mockRequestCache = new Mock<IRequestCache>();
            _mockRequestCache.Setup(c => c.GetValue("1-TimeZone")).Returns(value:"6");
            _mockRequestCache.Setup(c => c.GetValue("VolumeFlowRateLimitBasisId")).Returns(value:"3");

            _mockAppCache = new Mock<IApplicationCache>();
            _mockAppCache.Setup(c => c.Get("TimeZoneId_6")).Returns(value:"Eastern Standard Time");

            var actualTimeZoneService = new TimeZoneService(dbContext:connection,
                                                            settings:new SettingService(dbContext:connection, logger:_logger.Object, mapHelper:new MapHelper(),
                                                                                        cache:_mockRequestCache.Object, globalSettings:new Mock<IGlobalSettings>().Object),
                                                            mapHelper:new MapHelper(), appCache:_mockAppCache.Object);
            var actualSettings = new SettingService(dbContext:connection, logger:_logger.Object, mapHelper:new MapHelper(), cache:_mockRequestCache.Object,
                                                    globalSettings:new Mock<IGlobalSettings>().Object);

            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns(value:"1");
            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>())).Returns(value:new OrganizationRegulatoryProgramDto {OrganizationRegulatoryProgramId = 1});

            _paramService = new ParameterService(dbContext:connection,
                                                 httpContext:_httpContext.Object,
                                                 orgService:_orgService.Object,
                                                 mapHelper:new MapHelper(),
                                                 logger:_logger.Object,
                                                 timeZoneService:actualTimeZoneService,
                                                 settings:actualSettings);
        }

        /// <summary>
        ///     Must throw RuleViolationException if missing Name
        /// </summary>
        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void SaveParameterGroup_Test_Missing_Required_Name()
        {
            var paramGroupDto = new ParameterGroupDto();
            _paramService.SaveParameterGroup(parameterGroupDto:paramGroupDto);
        }

        /// <summary>
        ///     Must throw RuleViolationException if empty Parameter list (or null)
        /// </summary>
        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void SaveParameterGroup_Test_Parameter_List_Is_Null()
        {
            var paramGroupDto = new ParameterGroupDto();
            paramGroupDto.Name = "Some Name";

            //paramGroupDto.Parameters = ???
            _paramService.SaveParameterGroup(parameterGroupDto:paramGroupDto);
        }

        /// <summary>
        ///     Must throw RuleViolationException if empty Parameter list (or null)
        /// </summary>
        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void SaveParameterGroup_Test_Empty_Parameters_List()
        {
            var paramGroupDto = new ParameterGroupDto();
            paramGroupDto.Name = "Some Name";
            paramGroupDto.Parameters = new List<ParameterDto>();
            _paramService.SaveParameterGroup(parameterGroupDto:paramGroupDto);
        }

        [TestMethod]
        public void SaveParameterGroup_Test_Valid_CreateNew()
        {
            var paramGroupDto = new ParameterGroupDto
                                {
                                    Name = "Parameter Group ABC",
                                    Description = "Some description",
                                    IsActive = true,
                                    Parameters = new List<ParameterDto> {new ParameterDto {ParameterId = 1}, new ParameterDto {ParameterId = 2}},
                                    OrganizationRegulatoryProgramId = 1
                                };
            var newId = _paramService.SaveParameterGroup(parameterGroupDto:paramGroupDto);
        }

        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void SaveParameterGroup_Test_CreateNew_DuplicateName()
        {
            var paramGroupDto = new ParameterGroupDto
                                {
                                    Name = "Some Name",
                                    Parameters = new List<ParameterDto> {new ParameterDto {ParameterId = 1}},
                                    OrganizationRegulatoryProgramId = 1
                                };
            _paramService.SaveParameterGroup(parameterGroupDto:paramGroupDto);
        }

        [TestMethod]
        public void SaveParameterGroup_Test_Valid_Update()
        {
            var paramGroupDto = new ParameterGroupDto
                                {
                                    ParameterGroupId = 4,
                                    Name = "Parameter Group A+",
                                    Description = "Sample desc",
                                    Parameters = new List<ParameterDto> {new ParameterDto {ParameterId = 1}},
                                    OrganizationRegulatoryProgramId = 1
                                };
            var existingId = _paramService.SaveParameterGroup(parameterGroupDto:paramGroupDto);
        }

        [TestMethod]
        public void SaveParameterGroup_Test_Change_Parameter_Update()
        {
            var paramGroupDto = new ParameterGroupDto
                                {
                                    ParameterGroupId = 7,
                                    Name = "Some Name Changed Again",
                                    Description = "Sample desc",
                                    Parameters = new List<ParameterDto> {new ParameterDto {ParameterId = 2}},
                                    OrganizationRegulatoryProgramId = 1
                                };
            _paramService.SaveParameterGroup(parameterGroupDto:paramGroupDto);
        }

        [TestMethod]
        public void SaveParameterGroup_Test_Multiple_UniqueParameters_Update()
        {
            var paramGroupDto = new ParameterGroupDto
                                {
                                    ParameterGroupId = 7,
                                    Name = "Some Name Changed Again",
                                    Description = "Sample desc",
                                    Parameters = new List<ParameterDto> {new ParameterDto {ParameterId = 1}, new ParameterDto {ParameterId = 2}},
                                    OrganizationRegulatoryProgramId = 1
                                };
            _paramService.SaveParameterGroup(parameterGroupDto:paramGroupDto);
        }

        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void SaveParameterGroup_Test_Multiple_DuplicateParameters_Update()
        {
            var paramGroupDto = new ParameterGroupDto
                                {
                                    ParameterGroupId = 4,
                                    Name = "Some Name Changed Again",
                                    Description = "Sample desc",
                                    Parameters = new List<ParameterDto> {new ParameterDto {ParameterId = 2}, new ParameterDto {ParameterId = 2}},
                                    OrganizationRegulatoryProgramId = 1
                                };
            _paramService.SaveParameterGroup(parameterGroupDto:paramGroupDto);
        }

        [TestMethod]
        public void SaveParameterGroup_Test_Inline_Updating_Of_Parmeters()
        {
            var totalMetalsParameterGroup = _paramService.GetParameterGroup(parameterGroupId:1);

            //Remove the first one
            ParameterDto parameterToRemove = null;
            foreach (var parameter in totalMetalsParameterGroup.Parameters)
            {
                parameterToRemove = parameter;
                break;
            }

            totalMetalsParameterGroup.Parameters.Remove(item:parameterToRemove);

            //Add new one
            totalMetalsParameterGroup.Parameters.Add(item:new ParameterDto {ParameterId = 2});

            _paramService.SaveParameterGroup(parameterGroupDto:totalMetalsParameterGroup);
        }

        [TestMethod]
        public void GetParameterGroup_Test()
        {
            var paramGroupDto = _paramService.GetParameterGroup(parameterGroupId:3, isAuthorizationRequired:false);
        }

        [TestMethod]
        public void GetStaticParameterGroups_Test()
        {
            var staticParameterGroups = _paramService.GetStaticParameterGroups(isGetActiveOnly:true);
        }

        [TestMethod]
        public void DeleteParameterGroup_Test()
        {
            _paramService.DeleteParameterGroup(parameterGroupId:7);

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
            var resultDtos = _paramService.GetGlobalParameters(startsWith:"CHLOR");
        }

        [TestMethod]
        public void GetGlobalParameters_StartsWith_All_LowerCase_Test()
        {
            var resultDtos = _paramService.GetGlobalParameters(startsWith:"chlor");
        }

        [TestMethod]
        public void GetGlobalParameters_StartsWith_All_MixedCase_Test()
        {
            var resultDtos = _paramService.GetGlobalParameters(startsWith:"chLOr");
        }

        [TestMethod]
        public void GetGlobalParameters_StartsWith_All_Leading_Whitespace_Test()
        {
            var resultDtos = _paramService.GetGlobalParameters(startsWith:"  Chlor");
        }

        [TestMethod]
        public void GetGlobalParameters_StartsWith_All_Alphanumeric_Test()
        {
            var resultDtos = _paramService.GetGlobalParameters(startsWith:"1,2");
        }

        [TestMethod]
        public void GetAllParameterGroups_Test()
        {
            var resultDtos = _paramService.GetAllParameterGroups(monitoringPointId:129, sampleEndDateTimeLocal:new DateTime(year:1997, month:3, day:31));
        }

        [TestMethod]
        public void CanUserExecuteApi_GetParameterGroup_AsAuthority_Authorized_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");

            var parameterGroupId = 1;
            var isAuthorized = _paramService.CanUserExecuteApi("GetParameterGroup", parameterGroupId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetParameterGroup_AsAuthority_Unauthorized_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"2");

            var parameterGroupId = 1;
            var isAuthorized = _paramService.CanUserExecuteApi("GetParameterGroup", parameterGroupId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetParameterGroup_AsIndustry_Authorized_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"2");

            var parameterGroupId = 1;
            var isAuthorized = _paramService.CanUserExecuteApi("GetParameterGroup", parameterGroupId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetParameterGroup_AsIndustry_Unauthorized_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"99");

            var mockDifferentAuthority = new OrganizationRegulatoryProgramDto {OrganizationRegulatoryProgramId = 3000};
            _orgService.Setup(s => s.GetAuthority(99)).Returns(value:mockDifferentAuthority);

            var parameterGroupId = 1;
            var isAuthorized = _paramService.CanUserExecuteApi("GetParameterGroup", parameterGroupId);

            Assert.IsFalse(condition:isAuthorized);
        }
    }
}