using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Settings;
using AutoMapper;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class SettingServiceTests
    {
        private SettingService settingService;
        //private const string CONN_STRING = "Integrated Security=SSPI;Initial Catalog=LXDev01;Data Source=(local);";
        private const string CONN_STRING = "Integrated Security=SSPI;Initial Catalog=LinkoExchange;Data Source=(local);";

        public SettingServiceTests()
        {
            Mapper.Initialize(cfg =>
            {
                //cfg.AddProfile(new UserMapProfile());
                //cfg.AddProfile(new EmailAuditLogEntryMapProfile());
                //cfg.AddProfile(new InvitationMapProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramUserDtoMapProfile());
                cfg.AddProfile(new OrganizationMapProfile());
                cfg.AddProfile(new SettingMapProfile());
            });

            //Make sure there no methods were missing in the mappings loaded above via profiles
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void Initialize()
        {
            settingService = new SettingService(new LinkoExchangeContext(CONN_STRING), Mapper.Instance);
        }

        [TestMethod]
        public void GetOrganizationSettingsById_actual()
        {
            OrganizationSettingDto orgSettings = settingService.GetOrganizationSettingsById_actual(1);
        }

        [TestMethod]
        public void CreateOrUpdateProgramSettings()
        {
            var settings = new ProgramSettingDto() { OrgRegProgId = 1 };
            settings.Settings = new List<SettingDto>();
            settings.Settings.Add(new SettingDto() { Type = SettingType.MaxKBQAttempts, Value = "10" });
            settings.Settings.Add(new SettingDto() { Type = SettingType.PasswordChangeDays, Value = "31" });
            settingService.CreateOrUpdateProgramSettings(settings);
        }

    }
}
