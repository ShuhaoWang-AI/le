using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Data;
using AutoMapper;
using Linko.LinkoExchange.Services.AutoMapperProfile;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class OrganizationServiceTests
    {
        private OrganizationService orgService;
        private const string CONN_STRING = "Integrated Security=SSPI;Initial Catalog=LXDev01;Data Source=(local);";

        public OrganizationServiceTests()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new UserMapProfile());
                cfg.AddProfile(new EmailAuditLogEntryMapProfile());
                cfg.AddProfile(new InvitationMapProfile());
                cfg.AddProfile(new OrganziationRegulatoryProgramUserDtoMapProfile());
                cfg.AddProfile(new OrganizationMapProfile());
            });

            //Make sure there no methods were missing in the mappings loaded above via profiles
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void Initialize()
        {
            orgService = new OrganizationService(new LinkoExchangeContext(CONN_STRING), Mapper.Instance);
        }

        [TestMethod]
        public void GetOrganization()
        {
            var org = orgService.GetOrganization(1);
        }
    }
}
