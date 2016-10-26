using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Core.Domain;
using Microsoft.Owin.Security;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.User;
using Microsoft.AspNet.Identity;
using System.Security.Claims;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class AuthenticationServiceTests
    {
        string connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
        ISettingService settService = Mock.Of<ISettingService>();
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        IProgramService progService = Mock.Of<IProgramService>();
        IInvitationService invitService = Mock.Of<IInvitationService>();
        IEmailService emailService = Mock.Of<IEmailService>();
        IPermissionService permService = Mock.Of<IPermissionService>();
        IUserService userService = Mock.Of<IUserService>();
        ISessionCache sessionCache = Mock.Of<ISessionCache>();
        Mock<IAuthenticationManager> authManger = new Mock<IAuthenticationManager>();
        Mock<ApplicationSignInManager> signmanager;
        Mock<LinkoExchangeContext> dbContext;
        Mock<ApplicationUserManager> userManagerObj;
        UserProfile userProfile;

       [TestInitialize]
        public void TestInitialize()
        {
            userProfile = Mock.Of<UserProfile>(
                  i => i.FirstName == "firstName" &&
                  i.LastName == "lastName" &&
                  i.IsAccountLocked == true &&
                  i.IsAccountResetRequired == false &&
                  i.IsIdentityProofed == true &&
                  i.UserName == "firstNameLastName" &&
                  i.UserProfileId == 1 &&
                  i.ZipCode == "zipcode"
              );


            var userStore = new Mock<IUserStore<UserProfile>>();
            userManagerObj = new Mock<ApplicationUserManager>(userStore.Object);
            signmanager = new Mock<ApplicationSignInManager>(userManagerObj.Object, authManger.Object);

            dbContext = new Mock<LinkoExchangeContext>(connectionString);

        
            IList<Claim> claims = new List<Claim>();
            var tc = Task.FromResult(claims);
            userManagerObj.Setup(p => p.GetClaimsAsync(It.IsAny<string>())).Returns(tc); 
        }


        [TestMethod]
        public void ShouldReturnUserNotFound()
        {
            userProfile = null;
            userManagerObj.Setup(
              p => p.FindByNameAsync(It.IsAny<string>())).
                      Returns(
                         Task.FromResult(userProfile)
                      ); 

            var authService = new AuthenticationService(
                userManagerObj.Object,
                signmanager.Object,
                authManger.Object,
                settService,
                orgService,
                progService,
                invitService,
                emailService,
                permService,
                dbContext.Object,
                userService,
                sessionCache
                );

            var result = authService.SignInByUserName("shuhao", "password", true);

            Assert.AreEqual(AuthenticationResult.UserNotFound, result.Result.AutehticationResult);
        }


        [TestMethod]
        public void Test_FindByName()
        {

            userManagerObj.Setup( p => p.FindByNameAsync(It.IsAny<string>())).
                  Returns(
                     Task.FromResult(userProfile)
                  );


            var authService = new AuthenticationService(
                userManagerObj.Object,
                signmanager.Object,
                authManger.Object,
                settService,
                orgService,
                progService,
                invitService,
                emailService,
                permService,
                dbContext.Object,
                userService,
                sessionCache
                ); 

            var result = authService.SignInByUserName("shuhao", "password", true);

            Assert.AreEqual(AuthenticationResult.UserIsLocked, result.Result.AutehticationResult); 
        }
    }
}
