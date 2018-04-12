using System.Collections.Generic;
using System.Configuration;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.User;
using Microsoft.AspNet.Identity;
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
    public class UserServiceTests
    {
        #region fields

        private readonly Mock<ILinkoExchangeEmailService> _linkoExchangeEmailService = new Mock<ILinkoExchangeEmailService>();
        private readonly IRequestCache _requestCache = Mock.Of<IRequestCache>();

        private Mock<ICromerrAuditLogService> _cromerrLogger;
        private Mock<IHttpContextService> _httpContext;
        private Mock<ILogger> _logger;
        private Mock<IOrganizationService> _orgService;
        private Mock<IQuestionAnswerService> _qaService;
        private IOrganizationService _realOrgService;
        private ISettingService _realSettingService;
        private UserService _realUserService;
        private Mock<ISessionCache> _sessionCache;
        private Mock<ISettingService> _settingService;
        private ITimeZoneService _timeZones;
        private Mock<ApplicationUserManager> _userManager;
        private UserService _userService;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;

            _httpContext = new Mock<IHttpContextService>();
            _httpContext.Setup(x => x.CurrentUserProfileId()).Returns(value:2);

            _sessionCache = new Mock<ISessionCache>();
            _httpContext.Setup(x => x.GetClaimValue(It.IsAny<string>())).Returns(value:"2");

            _orgService = new Mock<IOrganizationService>();
            _orgService.Setup(x => x.GetAuthority(It.IsAny<int>())).Returns(value:new OrganizationRegulatoryProgramDto {OrganizationRegulatoryProgramId = 1});

            var userStore = new Mock<IUserStore<UserProfile>>();
            _userManager = new Mock<ApplicationUserManager>(userStore.Object);
            _qaService = new Mock<IQuestionAnswerService>();
            _logger = new Mock<ILogger>();
            _cromerrLogger = new Mock<ICromerrAuditLogService>();

            //_settingService.GetGlobalSettings()
            var globalSettingLookup = new Dictionary<SystemSettingType, string>
                                      {
                                          {SystemSettingType.SupportPhoneNumber, "555-555-5555"},
                                          {SystemSettingType.SupportEmailAddress, "test@test.com"}
                                      };
            _settingService = new Mock<ISettingService>();
            _settingService.Setup(x => x.GetGlobalSettings()).Returns(value:globalSettingLookup);
            _settingService.Setup(x => x.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<int>(), SettingType.TimeZone)).Returns(value:"1");

            _timeZones = new TimeZoneService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString), settings:_settingService.Object,
                                             mapHelper:new MapHelper(), appCache:new Mock<IApplicationCache>().Object, logger:_logger.Object);
            _realSettingService = new SettingService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString), logger:_logger.Object, mapHelper:new MapHelper(),
                                                     cache:new Mock<IRequestCache>().Object, globalSettings:new Mock<IGlobalSettings>().Object);
            _realOrgService = new OrganizationService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                                      settingService:_realSettingService, httpContext:new HttpContextService(),
                                                      jurisdictionService:new JurisdictionService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                                                                                  mapHelper:new MapHelper(), logger:_logger.Object), timeZoneService:_timeZones,
                                                      mapHelper:new MapHelper());
            _realUserService = new UserService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString), httpContext:_httpContext.Object,
                                               settingService:_realSettingService,
                                               orgService:_realOrgService, requestCache:_requestCache, timeZones:_timeZones, logService:_logger.Object, mapHelper:new MapHelper(),
                                               crommerAuditLogService:_cromerrLogger.Object, linkoExchangeEmailService:_linkoExchangeEmailService.Object);

            _userService = new UserService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                           httpContext:_httpContext.Object, settingService:_settingService.Object,
                                           orgService:_orgService.Object, requestCache:_requestCache, timeZones:_timeZones, logService:_logger.Object, mapHelper:new MapHelper(),
                                           crommerAuditLogService:_cromerrLogger.Object, linkoExchangeEmailService:_linkoExchangeEmailService.Object);
        }

        [TestMethod]
        public void GetUserProfilesForOrgRegProgram()
        {
            var userDtoList = _userService.GetUserProfilesForOrgRegProgram(orgRegProgramId:1, isRegApproved:null, isRegDenied:null, isEnabled:null, isRemoved:null);
        }

        [TestMethod]
        public void GetPendingRegistrationPendingUsersOrgRegProgram()
        {
            var userDtoList = _userService.GetUserProfilesForOrgRegProgram(orgRegProgramId:2, isRegApproved:false, isRegDenied:false, isEnabled:true, isRemoved:false);
        }

        //[TestMethod]
        //public void ApproveUserRegistrationWithPermissionsForProgram()
        //{
        //    var orgRegProgUserId = 7;
        //    var permissionGroupId = 1;
        //    _realUserService.UpdateUserPermissionGroupId(orgRegProgUserId:orgRegProgUserId, permissionGroupId:permissionGroupId);
        //    _realUserService.UpdateOrganizationRegulatoryProgramUserApprovedStatus(orgRegProgUserId:orgRegProgUserId, isApproved:true, isSignatory:false);
        //}

        [TestMethod]
        public void ApprovePendingRegistrationTransactionWithEmails()
        {
            var orgRegProgUserId = 7;
            var permissionGroupId = 65;
            _realUserService.ApprovePendingRegistration(orgRegProgUserId:orgRegProgUserId, permissionGroupId:permissionGroupId, isApproved:false);
        }

        [TestMethod]
        public void GetPendingRegistrationsProgramUsers()
        {
            var orgRegProgUserId = 1;
            _realUserService.GetPendingRegistrationProgramUsers(orgRegProgramId:orgRegProgUserId);
        }

        [TestMethod]
        public void UpdateUserSignatoryStatus()
        {
            var orgRegProgUserId = 7;
            _userService.UpdateUserSignatoryStatus(orgRegProgUserId:orgRegProgUserId, isSignatory:true);
        }

        [TestMethod]
        public void UpdateUserState_RegistrationApproved()
        {
            var orgRegProgUserId = 1;
            _userService.UpdateUserState(orgRegProgUserId:orgRegProgUserId, isRegApproved:false, isRegDenied:false, isEnabled:false, isRemoved:false);
            _userService.UpdateUserState(orgRegProgUserId:orgRegProgUserId, isRegApproved:true, isRegDenied:null, isEnabled:null, isRemoved:null);
        }

        [TestMethod]
        public void UpdateUserState_RegistrationDenied()
        {
            var orgRegProgUserId = 1;
            _userService.UpdateUserState(orgRegProgUserId:orgRegProgUserId, isRegApproved:false, isRegDenied:false, isEnabled:false, isRemoved:false);
            _userService.UpdateUserState(orgRegProgUserId:orgRegProgUserId, isRegApproved:null, isRegDenied:true, isEnabled:null, isRemoved:null);
        }

        [TestMethod]
        public void UpdateUserState_Enabled()
        {
            var orgRegProgUserId = 1;
            _userService.UpdateUserState(orgRegProgUserId:orgRegProgUserId, isRegApproved:false, isRegDenied:false, isEnabled:false, isRemoved:false);
            _userService.UpdateUserState(orgRegProgUserId:orgRegProgUserId, isRegApproved:null, isRegDenied:null, isEnabled:true, isRemoved:null);
        }

        [TestMethod]
        public void UpdateUserState_Removed()
        {
            var orgRegProgUserId = 1;
            _userService.UpdateUserState(orgRegProgUserId:orgRegProgUserId, isRegApproved:false, isRegDenied:false, isEnabled:false, isRemoved:false);
        }

        [TestMethod]
        public void UpdateUserState_UnRemove()
        {
            var orgRegProgUserId = 1;
            _userService.RemoveUser(orgRegProgUserId:orgRegProgUserId);
            _userService.UpdateUserState(orgRegProgUserId:orgRegProgUserId, isRegApproved:null, isRegDenied:null, isEnabled:null, isRemoved:false);
        }

        [TestMethod]
        public void LockUserAccount()
        {
            var resultDto = _userService.LockUnlockUserAccount(targetOrgRegProgUserId:13, isAttemptingLock:true, reason:AccountLockEvent.ManualAction,
                                                               isAuthorizationRequired:true);
        }

        [TestMethod]
        public void UnLockUserAccount()
        {
            var resultDto = _userService.LockUnlockUserAccount(targetOrgRegProgUserId:13, isAttemptingLock:false, reason:AccountLockEvent.ManualAction,
                                                               isAuthorizationRequired:true);
        }

        [TestMethod]
        public void DisableUserAccount()
        {
            _userService.EnableDisableUserAccount(orgRegProgramUserId:1, isAttemptingDisable:true);
        }

        [TestMethod]
        public void ResetUser()
        {
            var result = _userService.ResetUser(targetOrgRegProgUserId:1, newEmailAddress:"markus.jeon@watertrax.com");
        }

        [TestMethod]
        public void UpdateProfile()
        {
            var dto = new UserDto
                      {
                          FirstName = "Billy",
                          LastName = "Goat",
                          TitleRole = "President",
                          BusinessName = "Acme Corp.",
                          UserProfileId = 1,
                          AddressLine1 = "1234 Main St",
                          AddressLine2 = "Apt 102",
                          CityName = "Toronto",
                          ZipCode = "55555",
                          PhoneNumber = "555-555-5555",
                          JurisdictionId = 4
                      };

            _userService.UpdateProfile(dto:dto);
        }

        [TestMethod]
        public void UpdateEmail()
        {
            var isWorked = _userService.UpdateEmail(userProfileId:1, newEmailAddress:"jbourne@test.com");
        }

        [TestMethod]
        public void UpdateUserSignatoryStatus_Test()
        {
            _userService.UpdateUserSignatoryStatus(orgRegProgUserId:1, isSignatory:true);
        }

        [TestMethod]
        public void EnableDisableUserAccount_Test()
        {
            _userService.EnableDisableUserAccount(orgRegProgramUserId:9, isAttemptingDisable:true);
        }

        [TestMethod]
        public void GetOrganizationRegulatoryProgramUser_Test()
        {
            _userService.GetOrganizationRegulatoryProgramUser(orgRegProgUserId:5, isAuthorizationRequired:true);
        }

        [TestMethod]
        public void CanUserExecuteApi_EnableDisableUserAccount_Within_Same_OrgRegProgram_Is_Admin_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns(value:"3");

            var targetOrgRegProgUserId = 4;
            var isAuthorized = _userService.CanUserExecuteApi("EnableDisableUserAccount", targetOrgRegProgUserId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_EnableDisableUserAccount_Within_Same_OrgRegProgram_Not_Admin_False_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"industry");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns(value:"4");

            var targetOrgRegProgUserId = 3;
            var isAuthorized = _userService.CanUserExecuteApi("EnableDisableUserAccount", targetOrgRegProgUserId);

            Assert.IsFalse(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_EnableDisableUserAccount_Within_Same_Authority_Is_Admin_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns(value:"3");

            var targetOrgRegProgUserId = 4;
            var isAuthorized = _userService.CanUserExecuteApi("EnableDisableUserAccount", targetOrgRegProgUserId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgramUser_Accessing_Self_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns(value:"3");

            var targetOrgRegProgUserId = 3;
            var isAuthorized = _userService.CanUserExecuteApi("GetOrganizationRegulatoryProgramUser", targetOrgRegProgUserId);

            Assert.IsTrue(condition:isAuthorized);
        }

        [TestMethod]
        public void CanUserExecuteApi_GetOrganizationRegulatoryProgramUser_Accessing_Another_True_Test()
        {
            //Setup mocks
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.PortalName)).Returns(value:"authority");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId)).Returns(value:"1");
            _httpContext.Setup(s => s.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId)).Returns(value:"3");

            var targetOrgRegProgUserId = 4;
            var isAuthorized = _userService.CanUserExecuteApi("GetOrganizationRegulatoryProgramUser", targetOrgRegProgUserId);

            Assert.IsTrue(condition:isAuthorized);
        }
    }
}