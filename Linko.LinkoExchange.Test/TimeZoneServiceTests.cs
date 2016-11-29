﻿using System.Configuration;
using AutoMapper;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.AutoMapperProfile;
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

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class TimeZoneServiceTests
    {
        private TimeZoneService _tZservice;
        Mock<ISettingService> _settings;

        public TimeZoneServiceTests()
        {
            Mapper.Initialize(cfg =>
            {
                //cfg.AddProfile(new UserMapProfile());
                //cfg.AddProfile(new EmailAuditLogEntryMapProfile());
                //cfg.AddProfile(new InvitationMapProfile());
                //cfg.AddProfile(new OrganizationRegulatoryProgramUserDtoMapProfile());
                //cfg.AddProfile(new OrganizationMapProfile());
                //cfg.AddProfile(new OrganizationRegulatoryProgramMapProfile());
                //cfg.AddProfile(new RegulatoryProgramMapperProfile());
                //cfg.AddProfile(new PermissionGroupMapProfile());
                //cfg.AddProfile(new JurisdictionMapProfile());
                cfg.AddProfile(new TimeZoneMapProfile());
            }); 

            //Make sure there no methods were missing in the mappings loaded above via profiles
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void Initialize()
        {
            _settings = new Mock<ISettingService>();
            _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), 1, It.IsAny<SettingType>())).Returns("1");
            _settings.Setup(i => i.GetOrganizationSettingValue(It.IsAny<int>(), 2, It.IsAny<SettingType>())).Returns("4");

            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _tZservice = new TimeZoneService(new LinkoExchangeContext(connectionString), Mapper.Instance, _settings.Object);
        }

        [TestMethod]
        public void GetTimeZones_Test()
        {
            var dtos = _tZservice.GetTimeZones();
        }

        [TestMethod]
        public void GetCurrentUtcDateTime_Test()
        {
            DateTime now = DateTime.UtcNow;
            DateTimeOffset? registrationDate = now;
            DateTime regDatePST = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(registrationDate.Value.UtcDateTime, 1000, 1);
            DateTime regDateEST = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(registrationDate.Value.UtcDateTime, 1000, 2);
            Assert.AreEqual(regDateEST, regDatePST.AddHours(3));


            DateTimeOffset now2 = DateTimeOffset.UtcNow;
            registrationDate = now2;
            regDatePST = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(registrationDate.Value.UtcDateTime, 1000, 1);
            regDateEST = _tZservice.GetLocalizedDateTimeUsingSettingForThisOrg(registrationDate.Value.UtcDateTime, 1000, 2);
            Assert.AreEqual(regDateEST, regDatePST.AddHours(3));
           
        }

    }
}
