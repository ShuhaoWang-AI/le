using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.User;
using AutoMapper;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Microsoft.AspNet.Identity;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class UserServiceTests
    {
        private UserService userService;
        private const string CONN_STRING = "Integrated Security=SSPI;Initial Catalog=LXDev01;Data Source=(local);";
        //private const string CONN_STRING = "Integrated Security=SSPI;Initial Catalog=LinkoExchange;Data Source=(local);";

        public UserServiceTests()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new UserMapProfile());
                //cfg.AddProfile(new EmailAuditLogEntryMapProfile());
                //cfg.AddProfile(new InvitationMapProfile());
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
            userService = new UserService(new LinkoExchangeContext(CONN_STRING), new EmailAuditLogEntryDto(), new PasswordHasher(), Mapper.Instance);
        }

        [TestMethod]
        public void GetUserProfilesForOrgRegProgram()
        {
            List<UserDto> userDtoList = userService.GetUserProfilesForOrgRegProgram(1, null, null, null, null);
        }

        [TestMethod]
        public void GetPendingRegistrationPendingUsersOrgRegProgram()
        {
            List<UserDto> userDtoList = userService.GetUserProfilesForOrgRegProgram(1, false, false, true, false);
        }

        [TestMethod]
        public void ApproveUserRegistrationWithPermissionsForProgram()
        {
            var orgRegProgUserId = 1;
            var permissionGroupId = 1;
            userService.UpdateUserPermissionGroupId(1, 1);
            userService.UpdateOrganizationRegulatoryProgramUserApprovedStatus(orgRegProgUserId, true);
        }
    }
}
