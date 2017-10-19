using System.Collections.Generic;
using System.Configuration;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleStringLiteral

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class OrganizationServiceTests
    {
        #region fields

        private Mock<IHttpContextService> _httpContext;
        private Mock<ILogger> _logger;
        private OrganizationService _orgService;
        private Mock<ISettingService> _settingService;
        private ITimeZoneService _timeZones;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            _logger = new Mock<ILogger>();
            _httpContext = new Mock<IHttpContextService>();

            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns(value:"1000");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"Authority");

            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;

            var globalSettingLookup = new Dictionary<SystemSettingType, string>();
            globalSettingLookup.Add(key:SystemSettingType.SupportPhoneNumber, value:"555-555-5555");
            globalSettingLookup.Add(key:SystemSettingType.SupportEmailAddress, value:"test@test.com");

            _settingService = new Mock<ISettingService>();
            _settingService.Setup(x => x.GetGlobalSettings()).Returns(value:globalSettingLookup);
            _settingService.Setup(x => x.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<int>(), SettingType.TimeZone)).Returns(value:"1");

            _timeZones = new TimeZoneService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString), settings:_settingService.Object,
                                             mapHelper:new MapHelper(), appCache:new Mock<IApplicationCache>().Object);

            _orgService = new OrganizationService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                                  settingService:new SettingService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                                                                    logger:_logger.Object, mapHelper:new MapHelper(), cache:new Mock<IRequestCache>().Object,
                                                                                    globalSettings:new Mock<IGlobalSettings>().Object), httpContext:_httpContext.Object,
                                                  jurisdictionService:new JurisdictionService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                                                                              mapHelper:new MapHelper(), logService:_logger.Object), timeZoneService:_timeZones,
                                                  mapHelper:new MapHelper());
        }

        [TestMethod]
        public void GetOrganization()
        {
            var org = _orgService.GetOrganization(organizationId:1001);
        }

        [TestMethod]
        public void GetChildOrganizationRegulatoryPrograms()
        {
            var childOrgs = _orgService.GetChildOrganizationRegulatoryPrograms(orgRegProgId:1);
        }

        [TestMethod]
        public void UpdateEnableDisableFlag()
        {
            var result = _orgService.UpdateEnableDisableFlag(orgRegProgId:2, isEnabled:true);
        }

        [TestMethod]
        public void Test_GetUserOrganizations()
        {
            var orgs = _orgService.GetUserOrganizations(userId:7);
        }

        [TestMethod]
        public void Test_GetUserRegulatories()
        {
            var orgs = _orgService.GetUserRegulators(userId:1);
        }

        [TestMethod]
        public void Test_GetChildOrganizationRegulatoryPrograms()
        {
            var orgs = _orgService.GetChildOrganizationRegulatoryPrograms(orgRegProgId:1, searchString:"M");
        }

        [TestMethod]
        public void GetRemainingIndustryLicenseCount()
        {
            var dto = _orgService.GetRemainingIndustryLicenseCount(orgRegProgramId:1);
        }

        [TestMethod]
        public void GetRemainingUserLicenseCount_ForAuthority()
        {
            var dto = _orgService.GetRemainingUserLicenseCount(orgRegProgramId:1);
        }

        [TestMethod]
        public void GetRemainingUserLicenseCount_ForIndustry()
        {
            var dto = _orgService.GetRemainingUserLicenseCount(orgRegProgramId:2);
        }

        [TestMethod]
        public void GetUserRegulatories()
        {
            var dto = _orgService.GetUserRegulators(userId:13);
        }

        [TestMethod]
        public void GetUserAuthorityListForEmailContent()
        {
            var authorityListString = _orgService.GetUserAuthorityListForEmailContent(userProfileId:1);
        }

        [TestMethod]
        public void GetOrganizationRegulatoryProgram()
        {
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"Industry");

            var result = _orgService.GetOrganizationRegulatoryProgram(orgRegProgId:1);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgram_AsAuthority_Authorized_Itself_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns(value:"1000");

            var targetOrgRegProgId = 1;
            var isAuthorized = _orgService.CanUserExecuteApi("GetOrganizationRegulatoryProgram", targetOrgRegProgId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgram_AsIndustry_Authorized_Itself_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"3");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns(value:"2");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns(value:"1002");

            var targetOrgRegProgId = 3;
            var isAuthorized = _orgService.CanUserExecuteApi("GetOrganizationRegulatoryProgram", targetOrgRegProgId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgram_AsAuthority_Unauthorized_AnotherAuthority_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns(value:"1000");

            var targetOrgRegProgId = 999;
            var isAuthorized = _orgService.CanUserExecuteApi("GetOrganizationRegulatoryProgram", targetOrgRegProgId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgram_AsIndustry_Unauthorized_AnotherIndustry_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"3");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns(value:"2");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns(value:"1002");

            var targetOrgRegProgId = 4;
            var isAuthorized = _orgService.CanUserExecuteApi("GetOrganizationRegulatoryProgram", targetOrgRegProgId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgram_AsIndustry_Unauthorized_NoPermissions_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"3");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns(value:"14");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns(value:"1002");

            var targetOrgRegProgId = 3;
            var isAuthorized = _orgService.CanUserExecuteApi("GetOrganizationRegulatoryProgram", targetOrgRegProgId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgram_AsIndustry_Unauthorized_StandardUser_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"3");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns(value:"10");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationId)).Returns(value:"1002");

            var targetOrgRegProgId = 3;
            var isAuthorized = _orgService.CanUserExecuteApi("GetOrganizationRegulatoryProgram", targetOrgRegProgId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void Formatting_Organization_Address_String_Complete_Test()
        {
            //Setup mocks
            var formattedString = IndustryViewModelAddressGetter("PO Box 2088", "1050 Edgewater Cres", "Squamish", "BC", "V8B0B4");

            Assert.IsTrue(formattedString == "PO Box 2088 1050 Edgewater Cres, Squamish, BC V8B0B4");
        }

        [TestMethod]
        public void Formatting_Organization_Address_String_No_Address_At_All_Test()
        {
            //Setup mocks
            var formattedString = IndustryViewModelAddressGetter("", "", "", "", "");

            Assert.IsTrue(formattedString == "");
        }

        [TestMethod]
        public void Formatting_Organization_Address_String_Only_AddressLine1_State_Test()
        {
            //Setup mocks
            var formattedString = IndustryViewModelAddressGetter("PO Box 2088", "", "", "BC", "");

            Assert.IsTrue(formattedString == "PO Box 2088, BC");
        }

        [TestMethod]
        public void Formatting_Organization_Address_String_Only_AddressLine1_City_State_Test()
        {
            //Setup mocks
            var formattedString = IndustryViewModelAddressGetter("PO Box 2088", "", "Squamish", "BC", "");

            Assert.IsTrue(formattedString == "PO Box 2088, Squamish, BC");
        }

        [TestMethod]
        public void Formatting_Organization_Address_String_Only_City_State_Test()
        {
            //Setup mocks
            var formattedString = IndustryViewModelAddressGetter("", "", "Squamish", "BC", "");

            Assert.IsTrue(formattedString == "Squamish, BC");
        }

        [TestMethod]
        public void Formatting_Organization_Address_String_Only_State_Test()
        {
            //Setup mocks
            var formattedString = IndustryViewModelAddressGetter("", "", "", "BC", "");

            Assert.IsTrue(formattedString == "BC");
        }

        /// <summary>
        /// This code snippet was created to test the logic within the IndustryViewModel's Address "get" function
        /// It should mirror the code you will find there.
        /// (See Bug 4624)
        /// </summary>
        /// <param name="AddressLine1"></param>
        /// <param name="AddressLine2"></param>
        /// <param name="CityName"></param>
        /// <param name="State"></param>
        /// <param name="ZipCode"></param>
        /// <returns></returns>
        private string IndustryViewModelAddressGetter(string AddressLine1, string AddressLine2, string CityName,
           string State, string ZipCode)
        {
            string address = null;

            if (string.IsNullOrEmpty(value: address))
            {
                var formattedAddress = string.Format(format: "{0} {1}, {2}, {3} {4}", args: new object[] { AddressLine1, AddressLine2, CityName, State, ZipCode });
                formattedAddress = formattedAddress.Replace(oldValue: " , , ", newValue: ", ");
                formattedAddress = formattedAddress.Replace(oldValue: ", ,", newValue: ",");
                formattedAddress = formattedAddress.Replace(oldValue: " , ", newValue: ", ").Trim();
                if (formattedAddress.Length > 0 && formattedAddress[index: 0] == ',')
                {
                    //trim leading comma
                    formattedAddress = formattedAddress.Substring(startIndex: 1);
                }
                if (formattedAddress.Length > 0 && formattedAddress[index: formattedAddress.Length - 1] == ',')
                {
                    //trim trailing comma
                    formattedAddress = formattedAddress.Substring(startIndex: 0, length: formattedAddress.Length - 1);
                }

                return formattedAddress.Trim();
            }
            else
            {
                return address;
            }
        }
    }
}