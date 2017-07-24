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
using Linko.LinkoExchange.Services.Cache;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class SettingServiceTests
    {
        private SettingService _settingService;
        Mock<ILogger> _logger;

        public SettingServiceTests()
        {
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _logger = new Mock<ILogger>();
            _settingService = new SettingService(new LinkoExchangeContext(connectionString), _logger.Object, new MapHelper(), new Mock<IRequestCache>().Object, new Mock<IGlobalSettings>().Object);
        }

        [TestMethod]
        public void GetOrganizationSettingsById_actual()
        {
            OrganizationSettingDto orgSettings = _settingService.GetOrganizationSettingsById(1001);
        }

        [TestMethod]
        public void GetOrganizationSettingValue_Test()
        {
            var result = _settingService.GetOrganizationSettingValue(1003, 1, SettingType.PasswordHistoryMaxCount);
        }

        [TestMethod]
        public void GetOrgRegProgramSettingValue_Test()
        {
            var result = _settingService.GetOrgRegProgramSettingValue(1, SettingType.IndustryLicenseTotalCount);
        }

        [TestMethod]
        public void CreateOrUpdateProgramSettings()
        {
            var settings = new ProgramSettingDto() { OrgRegProgId = 1 };
            settings.Settings = new List<SettingDto>();
            settings.Settings.Add(new SettingDto() { TemplateName = SettingType.FailedKBQAttemptMaxCount, Value = "10" });
            settings.Settings.Add(new SettingDto() { TemplateName = SettingType.FailedPasswordAttemptMaxCount, Value = "31" });
            settings.Settings.Add(new SettingDto() { TemplateName = SettingType.EmailContactInfoEmailAddress, Value = "test@test.com" });

            _settingService.CreateOrUpdateProgramSettings(settings);
        }

        [TestMethod]
        [ExpectedException(typeof(RuleViolationException))]
        public void CreateOrUpdateProgramSettings_Atomic_Rollback_Test()
        {
            var settings = new ProgramSettingDto() { OrgRegProgId = 1 };
            settings.Settings = new List<SettingDto>();
            settings.Settings.Add(new SettingDto() { TemplateName = SettingType.FailedKBQAttemptMaxCount, Value = "10" });
            settings.Settings.Add(new SettingDto() { TemplateName = SettingType.FailedPasswordAttemptMaxCount, Value = "31" });
            settings.Settings.Add(new SettingDto() { TemplateName = SettingType.EmailContactInfoEmailAddress, Value = null });

            _settingService.CreateOrUpdateProgramSettings(settings);
        }

        [TestMethod]
        public void CreateOrUpdateOrganizationSettings()
        {
            var settings = new OrganizationSettingDto() { OrganizationId = 1001 };
            settings.Settings = new List<SettingDto>();
            settings.Settings.Add(new SettingDto() { TemplateName = SettingType.TimeZone, Value = "2" });
            settings.Settings.Add(new SettingDto() { TemplateName = SettingType.FailedPasswordAttemptMaxCount, Value = "5" });
            _settingService.CreateOrUpdateOrganizationSettings(settings);
        }

        [TestMethod]
        [ExpectedException(typeof(RuleViolationException))]
        public void CreateOrUpdateOrganizationSettings_Atomic_Rollback_Test()
        {
            var settings = new OrganizationSettingDto() { OrganizationId = 1001 };
            settings.Settings = new List<SettingDto>();
            settings.Settings.Add(new SettingDto() { TemplateName = SettingType.TimeZone, Value = "2" });
            settings.Settings.Add(new SettingDto() { TemplateName = SettingType.FailedPasswordAttemptMaxCount, Value = "5" });
            settings.Settings.Add(new SettingDto() { TemplateName = SettingType.EmailContactInfoEmailAddress, Value = null });
            _settingService.CreateOrUpdateOrganizationSettings(settings);
        }


    }
}
