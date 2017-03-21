﻿using System.Collections.Generic;
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
            var connection = new LinkoExchangeContext(connectionString);
            _httpContext = new Mock<IHttpContextService>();
            _orgService = new Mock<IOrganizationService>();
            _logger = new Mock<ILogger>();
            _timeZoneService = new Mock<ITimeZoneService>();

            var actualTimeZoneService = new TimeZoneService(connection, new SettingService(connection, _logger.Object, new MapHelper()), new MapHelper());

            _httpContext.Setup(s => s.GetClaimValue(It.IsAny<string>())).Returns("1");
            _orgService.Setup(s => s.GetAuthority(It.IsAny<int>())).Returns(new OrganizationRegulatoryProgramDto() { OrganizationRegulatoryProgramId = 1 });

            _paramService = new ParameterService(connection, 
                _httpContext.Object, 
                _orgService.Object, 
                new MapHelper(), 
                _logger.Object,
                actualTimeZoneService);
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
            paramGroupDto.Name = "Different Name 3";
            paramGroupDto.Description = "Different description";
            paramGroupDto.IsActive = true;
            paramGroupDto.Parameters = new List<ParameterDto>();
            paramGroupDto.Parameters.Add(new ParameterDto() { ParameterId = 1 });
            paramGroupDto.OrganizationRegulatoryProgramId = 1;
            _paramService.SaveParameterGroup(paramGroupDto);
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
            paramGroupDto.Name = "Some Name Changed Again";
            paramGroupDto.Description = "Sample desc";
            paramGroupDto.Parameters = new List<ParameterDto>();
            paramGroupDto.Parameters.Add(new ParameterDto() { ParameterId = 1 });
            paramGroupDto.OrganizationRegulatoryProgramId = 1;
            _paramService.SaveParameterGroup(paramGroupDto);
        }

        [TestMethod]
        public void SaveParameterGroup_Test_Change_Parameter_Update()
        {
            var paramGroupDto = new ParameterGroupDto() { ParameterGroupId = 4 };
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
            var paramGroupDto = new ParameterGroupDto() { ParameterGroupId = 4 };
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
        public void GetParameterGroup_Test()
        {
            var paramGroupDto = _paramService.GetParameterGroup(3);
        }

        [TestMethod]
        public void GetStaticParameterGroups_Test()
        {
            var staticParameterGroups = _paramService.GetStaticParameterGroups();
        }

        [TestMethod]
        public void DeleteParameterGroup_Test()
        {
            _paramService.DeleteParameterGroup(3);
        }

    }
}