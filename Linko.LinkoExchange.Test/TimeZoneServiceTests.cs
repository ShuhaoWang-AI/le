using System.Configuration;
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

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class TimeZoneServiceTests
    {
        private TimeZoneService _tZservice;

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
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _tZservice = new TimeZoneService(new LinkoExchangeContext(connectionString), Mapper.Instance);
        }

        [TestMethod]
        public void GetTimeZones_Test()
        {
            var dtos = _tZservice.GetTimeZones();
        }

    }
}
