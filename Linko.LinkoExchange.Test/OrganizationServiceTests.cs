using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Data;
using AutoMapper;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using System.Configuration;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class OrganizationServiceTests
    {
        private OrganizationService orgService;

        public OrganizationServiceTests()
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
            }); 

            //Make sure there no methods were missing in the mappings loaded above via profiles
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            orgService = new OrganizationService(new LinkoExchangeContext(connectionString), Mapper.Instance);
        }

        [TestMethod]
        public void GetOrganization()
        {
            var org = orgService.GetOrganization(1001);
        }

        [TestMethod]
        public void GetChildOrganizationRegulatoryPrograms()
        {
            var childOrgs = orgService.GetChildOrganizationRegulatoryPrograms(1);
        }

        
        [TestMethod]
        public void UpdateEnableDisableFlag()
        {
            orgService.UpdateEnableDisableFlag(1, true);
        }

        [TestMethod]
        public void GetUserOrganizationsByOrgRegProgUserId()
        {
            var orgs = orgService.GetUserOrganizationsByOrgRegProgUserId(1);
        }

        [TestMethod]
        public void Test_GetUserOrganizations()
        {
            var orgs = orgService.GetUserOrganizations(7); 

        }
    }
}
