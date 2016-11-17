using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Settings;
using AutoMapper;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;
using System.Configuration;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class SettingServiceTests
    {
        private SettingService _settingService;

        public SettingServiceTests()
        {
            Mapper.Initialize(cfg =>
            {
                //cfg.AddProfile(new UserMapProfile());
                //cfg.AddProfile(new EmailAuditLogEntryMapProfile());
                //cfg.AddProfile(new InvitationMapProfile());
                cfg.AddProfile(new RegulatoryProgramMapperProfile());
                cfg.AddProfile(new OrganizationMapProfile());
                //cfg.AddProfile(new OrganizationRegulatoryProgramUserDtoMapProfile());
                cfg.AddProfile(new SettingMapProfile());
            });

            //Make sure there no methods were missing in the mappings loaded above via profiles
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _settingService = new SettingService(new LinkoExchangeContext(connectionString), Mapper.Instance);
        }

        [TestMethod]
        public void GetOrganizationSettingsById_actual()
        {
            OrganizationSettingDto orgSettings = _settingService.GetOrganizationSettingsById(1001);
        }

        [TestMethod]
        public void GetOrgRegProgramSettingValue_Test()
        {
            var result = _settingService.GetOrgRegProgramSettingValue(1, SettingType.EmailContactInfoName);
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
