using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.User;
using AutoMapper;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Microsoft.AspNet.Identity;
using System.Configuration;
using Linko.LinkoExchange.Services;
using Moq;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.Authentication;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class UserServiceTests
    {
        private UserService _userService;
        Mock<IHttpContextService> _httpContext;
        Mock<IAuthenticationService> _authService;
        ISettingService _settingService = Mock.Of<ISettingService>();
        IEmailService _emailService = Mock.Of<IEmailService>();

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
                cfg.AddProfile(new OrganizationRegulatoryProgramUserDtoMapProfile());
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

            _authService = new Mock<IAuthenticationService>();
            _authService.Setup(x => x.GetClaimsValue(It.IsAny<string>())).Returns("1");

            _userService = new UserService(new LinkoExchangeContext(connectionString), new EmailAuditLogEntryDto(), 
                new PasswordHasher(), Mapper.Instance, _httpContext.Object, _emailService, _settingService,
                _authService.Object);
        }

        [TestMethod]
        public void GetUserProfilesForOrgRegProgram()
        {
            List<UserDto> userDtoList = _userService.GetUserProfilesForOrgRegProgram(1, null, null, null, null);
        }

        [TestMethod]
        public void GetPendingRegistrationPendingUsersOrgRegProgram()
        {
            List<UserDto> userDtoList = _userService.GetUserProfilesForOrgRegProgram(1, false, false, true, false);
        }

        [TestMethod]
        public void ApproveUserRegistrationWithPermissionsForProgram()
        {
            var orgRegProgUserId = 1;
            var permissionGroupId = 1;
            _userService.UpdateUserPermissionGroupId(1, permissionGroupId);
            _userService.UpdateOrganizationRegulatoryProgramUserApprovedStatus(orgRegProgUserId, true);
        }

        [TestMethod]
        public void UpdateUserSignatoryStatus()
        {
            var orgRegProgUserId = 1;
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
        public void LockLockUserAccount()
        {
            var resultDto = _userService.LockUnlockUserAccount(13, true);
        }

        [TestMethod]
        public void LockUnLockUserAccount()
        {
            var resultDto = _userService.LockUnlockUserAccount(13, false);
        }

        [TestMethod]
        public void DisableUserAccount()
        {
            var result = _userService.EnableDisableUserAccount(1, true);
        }

    }
}
