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

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class JurisdictionTests
    {
        private JurisdictionService _jService;

        public JurisdictionTests()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new UserMapProfile());
                cfg.AddProfile(new EmailAuditLogEntryMapProfile());
                cfg.AddProfile(new InvitationMapProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramUserDtoMapProfile());
                cfg.AddProfile(new OrganizationMapProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramMapProfile());
                cfg.AddProfile(new RegulatoryProgramMapperProfile());
                cfg.AddProfile(new PermissionGroupMapProfile());
                cfg.AddProfile(new JurisdictionMapProfile());
            }); 

            //Make sure there no methods were missing in the mappings loaded above via profiles
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _jService = new JurisdictionService(new LinkoExchangeContext(connectionString), Mapper.Instance);
        }

        [TestMethod]
        public void GetStateProvs_Test()
        {
            var states = _jService.GetStateProvs(2);
        }

        [TestMethod]
        public void GetJurisdictionById_Test()
        {
            var state = _jService.GetJurisdictionById(34);
        }

    }
}
