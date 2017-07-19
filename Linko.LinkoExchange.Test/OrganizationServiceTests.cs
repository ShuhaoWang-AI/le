using System.Configuration;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Core.Enum;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class OrganizationServiceTests
    {
        private OrganizationService orgService;
        Mock<ILogger> _logger;
        Mock<IHttpContextService> _httpContext;
        ITimeZoneService _timeZones;
        Mock<ISettingService> _settingService;

        public OrganizationServiceTests()
        {
        }

        [TestInitialize]
        public void Initialize()
        {
            _logger = new Mock<ILogger>();
            _httpContext = new Mock<IHttpContextService>();

            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns("1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns("1000");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("Authority");

            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;

            var globalSettingLookup = new Dictionary<SystemSettingType, string>();
            globalSettingLookup.Add(SystemSettingType.SupportPhoneNumber, "555-555-5555");
            globalSettingLookup.Add(SystemSettingType.SupportEmailAddress, "test@test.com");

            _settingService = new Mock<ISettingService>();
            _settingService.Setup(x => x.GetGlobalSettings()).Returns(globalSettingLookup);
            _settingService.Setup(x => x.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<int>(), SettingType.TimeZone)).Returns("1");

            _timeZones = new TimeZoneService(new LinkoExchangeContext(connectionString), _settingService.Object, new MapHelper(), new Mock<IApplicationCache>().Object);

            orgService = new OrganizationService(new LinkoExchangeContext(connectionString), 
                new SettingService(new LinkoExchangeContext(connectionString), _logger.Object, new MapHelper(), new Mock<IApplicationCache>().Object, new Mock<IGlobalSettings>().Object), _httpContext.Object,
                new JurisdictionService(new LinkoExchangeContext(connectionString), new MapHelper()), _timeZones, new MapHelper());
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
            var dto = orgService.GetRemainingUserLicenseCount(1);
        }

        [TestMethod]
        public void GetRemainingUserLicenseCount_ForIndustry()
        {
            var dto = orgService.GetRemainingUserLicenseCount(2);
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
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("Industry");

            var result = orgService.GetOrganizationRegulatoryProgram(1);
        }

    }
}
