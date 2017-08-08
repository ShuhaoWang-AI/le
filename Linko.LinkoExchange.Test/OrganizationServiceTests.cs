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
        private OrganizationService _orgService;
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

            _orgService = new OrganizationService(new LinkoExchangeContext(connectionString), 
                new SettingService(new LinkoExchangeContext(connectionString), _logger.Object, new MapHelper(), new Mock<IRequestCache>().Object, new Mock<IGlobalSettings>().Object), _httpContext.Object,
                new JurisdictionService(new LinkoExchangeContext(connectionString), new MapHelper(), _logger.Object), _timeZones, new MapHelper());
        }

        [TestMethod]
        public void GetOrganization()
        {
            var org = _orgService.GetOrganization(1001);
        }

        [TestMethod]
        public void GetChildOrganizationRegulatoryPrograms()
        {
            var childOrgs = _orgService.GetChildOrganizationRegulatoryPrograms(1);
        }

        
        [TestMethod]
        public void UpdateEnableDisableFlag()
        {
            var result = _orgService.UpdateEnableDisableFlag(2, true);
        }

        [TestMethod]
        public void Test_GetUserOrganizations()
        {
            var orgs = _orgService.GetUserOrganizations(7); 

        }

        [TestMethod]
        public void Test_GetUserRegulatories()
        {
            var orgs = _orgService.GetUserRegulators(1); 

        }

        [TestMethod]
        public void Test_GetChildOrganizationRegulatoryPrograms()
        {
            var orgs = _orgService.GetChildOrganizationRegulatoryPrograms(1, "M");

        }

        [TestMethod]
        public void GetRemainingIndustryLicenseCount()
        {
            var dto = _orgService.GetRemainingIndustryLicenseCount(1);
        }

        [TestMethod]
        public void GetRemainingUserLicenseCount_ForAuthority()
        {
            var dto = _orgService.GetRemainingUserLicenseCount(1);
        }

        [TestMethod]
        public void GetRemainingUserLicenseCount_ForIndustry()
        {
            var dto = _orgService.GetRemainingUserLicenseCount(2);
        }

        [TestMethod]
        public void GetUserRegulatories()
        {
            var dto = _orgService.GetUserRegulators(13);
        }

        [TestMethod]
        public void GetUserAuthorityListForEmailContent()
        {
            string authorityListString = _orgService.GetUserAuthorityListForEmailContent(1);
        }

        [TestMethod]
        public void GetOrganizationRegulatoryProgram()
        {
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("Industry");

            var result = _orgService.GetOrganizationRegulatoryProgram(1);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgram_AsAuthority_Authorized_Itself_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns("1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns("1000");

            int targetOrgRegProgId = 1;
            var isAuthorized = _orgService.CanUserExecuteApi("GetOrganizationRegulatoryProgram", targetOrgRegProgId);

            Assert.IsTrue(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgram_AsIndustry_Authorized_Itself_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("3");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns("2");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns("1002");

            int targetOrgRegProgId = 3;
            var isAuthorized = _orgService.CanUserExecuteApi("GetOrganizationRegulatoryProgram", targetOrgRegProgId);

            Assert.IsTrue(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgram_AsAuthority_Unauthorized_AnotherAuthority_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns("1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns("1000");

            int targetOrgRegProgId = 999;
            var isAuthorized = _orgService.CanUserExecuteApi("GetOrganizationRegulatoryProgram", targetOrgRegProgId);

            Assert.IsFalse(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgram_AsIndustry_Unauthorized_AnotherIndustry_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("3");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns("2");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns("1002");

            int targetOrgRegProgId = 4;
            var isAuthorized = _orgService.CanUserExecuteApi("GetOrganizationRegulatoryProgram", targetOrgRegProgId);

            Assert.IsFalse(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgram_AsIndustry_Unauthorized_NoPermissions_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("3");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns("14");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns("1002");

            int targetOrgRegProgId = 3;
            var isAuthorized = _orgService.CanUserExecuteApi("GetOrganizationRegulatoryProgram", targetOrgRegProgId);

            Assert.IsFalse(isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgram_AsIndustry_Unauthorized_StandardUser_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns("industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns("3");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns("10");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns("1002");

            int targetOrgRegProgId = 3;
            var isAuthorized = _orgService.CanUserExecuteApi("GetOrganizationRegulatoryProgram", targetOrgRegProgId);

            Assert.IsFalse(isAuthorized);
        }
    }
}
