using System.Collections.Generic;
using System.Configuration;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Settings;
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
    public class SettingServiceTests
    {
        #region fields

        private Mock<ILogger> _logger;
        private SettingService _settingService;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            _logger = new Mock<ILogger>();
            _settingService = new SettingService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString), logger:_logger.Object, mapHelper:new MapHelper(),
                                                 cache:new Mock<IRequestCache>().Object, globalSettings:new Mock<IGlobalSettings>().Object);
        }

        [TestMethod]
        public void GetOrganizationSettingsById_actual()
        {
            var orgSettings = _settingService.GetOrganizationSettingsById(organizationId:1001);
        }

        [TestMethod]
        public void GetOrganizationSettingValue_Test()
        {
            var result = _settingService.GetOrganizationSettingValue(organizationId:1003, regProgramId:1, settingType:SettingType.PasswordHistoryMaxCount);
        }

        [TestMethod]
        public void GetOrgRegProgramSettingValue_Test()
        {
            var result = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:1, settingType:SettingType.IndustryLicenseTotalCount);
        }

        [TestMethod]
        public void CreateOrUpdateProgramSettings()
        {
            var settings = new ProgramSettingDto {OrgRegProgId = 1};
            settings.Settings = new List<SettingDto>();
            settings.Settings.Add(item:new SettingDto {TemplateName = SettingType.FailedKBQAttemptMaxCount, Value = "10"});
            settings.Settings.Add(item:new SettingDto {TemplateName = SettingType.FailedPasswordAttemptMaxCount, Value = "31"});
            settings.Settings.Add(item:new SettingDto {TemplateName = SettingType.EmailContactInfoEmailAddress, Value = "test@test.com"});

            _settingService.CreateOrUpdateProgramSettings(settingDtos:settings);
        }

        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void CreateOrUpdateProgramSettings_Atomic_Rollback_Test()
        {
            var settings = new ProgramSettingDto {OrgRegProgId = 1};
            settings.Settings = new List<SettingDto>();
            settings.Settings.Add(item:new SettingDto {TemplateName = SettingType.FailedKBQAttemptMaxCount, Value = "10"});
            settings.Settings.Add(item:new SettingDto {TemplateName = SettingType.FailedPasswordAttemptMaxCount, Value = "31"});
            settings.Settings.Add(item:new SettingDto {TemplateName = SettingType.EmailContactInfoEmailAddress, Value = null});

            _settingService.CreateOrUpdateProgramSettings(settingDtos:settings);
        }

        [TestMethod]
        public void CreateOrUpdateOrganizationSettings()
        {
            var settings = new OrganizationSettingDto {OrganizationId = 1001};
            settings.Settings = new List<SettingDto>();
            settings.Settings.Add(item:new SettingDto {TemplateName = SettingType.TimeZone, Value = "2"});
            settings.Settings.Add(item:new SettingDto {TemplateName = SettingType.FailedPasswordAttemptMaxCount, Value = "5"});
            _settingService.CreateOrUpdateOrganizationSettings(settingDtos:settings);
        }

        [TestMethod]
        [ExpectedException(exceptionType:typeof(RuleViolationException))]
        public void CreateOrUpdateOrganizationSettings_Atomic_Rollback_Test()
        {
            var settings = new OrganizationSettingDto {OrganizationId = 1001};
            settings.Settings = new List<SettingDto>();
            settings.Settings.Add(item:new SettingDto {TemplateName = SettingType.TimeZone, Value = "2"});
            settings.Settings.Add(item:new SettingDto {TemplateName = SettingType.FailedPasswordAttemptMaxCount, Value = "5"});
            settings.Settings.Add(item:new SettingDto {TemplateName = SettingType.EmailContactInfoEmailAddress, Value = null});
            _settingService.CreateOrUpdateOrganizationSettings(settingDtos:settings);
        }
    }
}