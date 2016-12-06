using System.Collections.Generic;
using System.Configuration;
using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.User;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class UserServiceTests
    {
        private UserService _userService;
        private UserService _realUserService;
        Mock<IHttpContextService> _httpContext;
        Mock<ISessionCache> _sessionCache;
        IRequestCache _requestCache = Mock.Of<IRequestCache>();
        Mock<IOrganizationService> _orgService;
        IOrganizationService _realOrgService;
        Mock<ISettingService> _settingService;
        ISettingService _realSettingService;
        IEmailService _emailService = Mock.Of<IEmailService>();
        ITimeZoneService _timeZones;
        Mock<ApplicationUserManager> _userManager;
        Mock<IQuestionAnswerService> _qaService;
        Mock<ILogger> _logger;

        public UserServiceTests()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new UserMapProfile());
                //cfg.AddProfile(new EmailAuditLogEntryMapProfile());
                //cfg.AddProfile(new InvitationMapProfile());
                cfg.AddProfile(new PermissionGroupMapProfile());
                cfg.AddProfile(new RegulatoryProgramMapperProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramMapProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramUserMapProfile());
                cfg.AddProfile(new OrganizationMapProfile());
                cfg.AddProfile(new SettingMapProfile());
            });

            //Make sure there no methods were missing in the mappings loaded above via profiles
            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;

            _httpContext = new Mock<IHttpContextService>();
            _httpContext.Setup(x => x.CurrentUserProfileId()).Returns(2);

            _sessionCache = new Mock<ISessionCache>();
            _sessionCache.Setup(x => x.GetClaimValue(It.IsAny<string>())).Returns("2");

            _orgService = new Mock<IOrganizationService>();
            _orgService.Setup(x => x.GetAuthority(It.IsAny<int>())).Returns(new OrganizationRegulatoryProgramDto() { OrganizationRegulatoryProgramId = 1 });

            var userStore = new Mock<IUserStore<UserProfile>>();
            _userManager = new Mock<ApplicationUserManager>(userStore.Object);
            _qaService = new Mock<IQuestionAnswerService>();
            _logger = new Mock<ILogger>();

            //_settingService.GetGlobalSettings()
            var globalSettingLookup = new Dictionary<SystemSettingType, string>();
            globalSettingLookup.Add(SystemSettingType.SupportPhoneNumber, "555-555-5555");
            globalSettingLookup.Add(SystemSettingType.SupportEmailAddress, "test@test.com");
            _settingService = new Mock<ISettingService>();
            _settingService.Setup(x => x.GetGlobalSettings()).Returns(globalSettingLookup);
            _settingService.Setup(x => x.GetOrganizationSettingValue(It.IsAny<int>(), It.IsAny<int>(), SettingType.TimeZone)).Returns("1");

            _timeZones = new TimeZoneService(new LinkoExchangeContext(connectionString), Mapper.Instance, _settingService.Object);
            _realSettingService = new SettingService(new LinkoExchangeContext(connectionString), Mapper.Instance, _logger.Object);
            _realOrgService = new OrganizationService(new LinkoExchangeContext(connectionString),
                Mapper.Instance, _realSettingService, new HttpContextService(), new JurisdictionService(new LinkoExchangeContext(connectionString), Mapper.Instance));
            _realUserService = new UserService(new LinkoExchangeContext(connectionString), new EmailAuditLogEntryDto(),
                new PasswordHasher(), Mapper.Instance, _httpContext.Object, _emailService, _realSettingService,
                _sessionCache.Object, _realOrgService, _requestCache, _timeZones, _qaService.Object, _logger.Object);

            _userService = new UserService(new LinkoExchangeContext(connectionString), new EmailAuditLogEntryDto(), 
                new PasswordHasher(), Mapper.Instance, _httpContext.Object, _emailService, _settingService.Object, 
                _sessionCache.Object, _orgService.Object, _requestCache, _timeZones, _qaService.Object, _logger.Object);
        }

        [TestMethod]
        public void GetUserProfilesForOrgRegProgram()
        {
            var userDtoList = _userService.GetUserProfilesForOrgRegProgram(1, null, null, null, null);
        }

        [TestMethod]
        public void GetPendingRegistrationPendingUsersOrgRegProgram()
        {
            var userDtoList = _userService.GetUserProfilesForOrgRegProgram(2, false, false, true, false);
        }

        [TestMethod]
        public void ApproveUserRegistrationWithPermissionsForProgram()
        {
            var orgRegProgUserId = 7;
            var permissionGroupId = 1;
            _realUserService.UpdateUserPermissionGroupId(orgRegProgUserId, permissionGroupId);
            _realUserService.UpdateOrganizationRegulatoryProgramUserApprovedStatus(orgRegProgUserId, true);
        }

        [TestMethod]
        public void ApprovePendingRegistrationTransactionWithEmails()
        {
            var orgRegProgUserId = 7;
            var permissionGroupId = 65;
            _realUserService.ApprovePendingRegistration(orgRegProgUserId, permissionGroupId, false);
        }

        
        [TestMethod]
        public void GetPendingRegistrationsProgramUsers()
        {
            var orgRegProgUserId = 1;
            _realUserService.GetPendingRegistrationProgramUsers(orgRegProgUserId);
        }

        [TestMethod]
        public void UpdateUserSignatoryStatus()
        {
            var orgRegProgUserId = 7;
            _userService.UpdateUserSignatoryStatus(orgRegProgUserId, true);
        }

        [TestMethod]
        public void UpdateUserState_RegistrationApproved()
        {
            var orgRegProgUserId = 1;
            _userService.UpdateUserState(orgRegProgUserId, false, false, false, false);
            _userService.UpdateUserState(orgRegProgUserId, true, null, null, null);
        }

        [TestMethod]
        public void UpdateUserState_RegistrationDenied()
        {
            var orgRegProgUserId = 1;
            _userService.UpdateUserState(orgRegProgUserId, false, false, false, false);
            _userService.UpdateUserState(orgRegProgUserId, null, true, null, null);
        }

        [TestMethod]
        public void UpdateUserState_Enabled()
        {
            var orgRegProgUserId = 1;
            _userService.UpdateUserState(orgRegProgUserId, false, false, false, false);
            _userService.UpdateUserState(orgRegProgUserId, null, null, true, null);
        }
        [TestMethod]
        public void UpdateUserState_Removed()
        {
            var orgRegProgUserId = 1;
            _userService.UpdateUserState(orgRegProgUserId, false, false, false, false);
        }

        [TestMethod]
        public void UpdateUserState_UnRemove()
        {
            var orgRegProgUserId = 1;
            _userService.RemoveUser(orgRegProgUserId);
            _userService.UpdateUserState(orgRegProgUserId, null, null, null, false);
        }

        [TestMethod]
        public void LockUserAccount()
        {
            var resultDto = _userService.LockUnlockUserAccount(13, true, false);
        }

        [TestMethod]
        public void UnLockUserAccount()
        {
            var resultDto = _userService.LockUnlockUserAccount(13, false, false);
        }

        [TestMethod]
        public void DisableUserAccount()
        {
            _userService.EnableDisableUserAccount(1, true);
        }

        [TestMethod]
        public void ResetUser()
        {
            var result = _userService.ResetUser(1, "markus.jeon@watertrax.com");
        }

        [TestMethod]
        public void UpdateUser()
        {
            var dto = new UserDto()
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

            _userService.UpdateUser(dto);
        }

        [TestMethod]
        public void UpdateProfile()
        {
            var dto = new UserDto()
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

            _userService.UpdateProfile(dto);
        }

        [TestMethod]
        public void UpdateProfileComplete()
        {
            var dto = new UserDto()
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

            List<AnswerDto> kbqList = new List<AnswerDto>();
            List<AnswerDto> sqList = new List<AnswerDto>();
            _userService.UpdateProfile(dto, sqList, kbqList);
        }

        [TestMethod]
        public void UpdateEmail()
        {
            var isWorked = _userService.UpdateEmail(1, "jbourne@test.com");
        }


        [TestMethod]
        public void UpdateUserSignatoryStatus_Test()
        {
            _userService.UpdateUserSignatoryStatus(1, true);
        }


        [TestMethod]
        public void EnableDisableUserAccount_Test()
        {
            _userService.EnableDisableUserAccount(9, true);
        }

    }
}
