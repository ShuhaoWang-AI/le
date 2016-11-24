using System.Configuration;
using AutoMapper;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class OrganizationServiceTests
    {
        private OrganizationService orgService;
        Mock<ILogger> _logger;

        public OrganizationServiceTests()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new UserMapProfile());
                cfg.AddProfile(new EmailAuditLogEntryMapProfile());
                cfg.AddProfile(new InvitationMapProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramUserMapProfile());
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
            _logger = new Mock<ILogger>();
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            orgService = new OrganizationService(new LinkoExchangeContext(connectionString), 
                Mapper.Instance, new SettingService(new LinkoExchangeContext(connectionString), Mapper.Instance, _logger.Object), new HttpContextService(),
                new JurisdictionService(new LinkoExchangeContext(connectionString), Mapper.Instance));
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
            var result = orgService.UpdateEnableDisableFlag(2, true);
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

        [TestMethod]
        public void Test_GetUserRegulatories()
        {
            var orgs = orgService.GetUserRegulators(1); 

        }

        [TestMethod]
        public void Test_GetChildOrganizationRegulatoryPrograms()
        {
            var orgs = orgService.GetChildOrganizationRegulatoryPrograms(1, "M");

        }

        [TestMethod]
        public void GetRemainingIndustryLicenseCount()
        {
            var dto = orgService.GetRemainingIndustryLicenseCount(1);
        }

        [TestMethod]
        public void GetRemainingUserLicenseCount_ForAuthority()
        {
            var dto = orgService.GetRemainingUserLicenseCount(1, true);
        }

        [TestMethod]
        public void GetRemainingUserLicenseCount_ForIndustry()
        {
            var dto = orgService.GetRemainingUserLicenseCount(2, false);
        }

        [TestMethod]
        public void GetUserRegulatories()
        {
            var dto = orgService.GetUserRegulators(13);
        }

        [TestMethod]
        public void GetUserAuthorityListForEmailContent()
        {
            string authorityListString = orgService.GetUserAuthorityListForEmailContent(1);
        }

        [TestMethod]
        public void GetOrganizationRegulatoryProgram()
        {
            var result = orgService.GetOrganizationRegulatoryProgram(1);
        }

    }
}
