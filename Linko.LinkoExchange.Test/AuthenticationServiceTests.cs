using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
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
using System.Data.Entity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using AutoMapper;
using Linko.LinkoExchange.Services;

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
        ISessionCache sessionCache = new  SessionCache(new HttpContextServiceMock());
        //Mock.Of<ISessionCache>();

        IPasswordHasher passwordHasher = new PasswordHasher();
        IRequestCache requestCache = Mock.Of<IRequestCache>();
        IHttpContextService httpContextService = Mock.Of<IHttpContextService>();

        Mock<IAuthenticationManager> authManger = new Mock<IAuthenticationManager>();
        Mock<ApplicationSignInManager> signmanager;
        Mock<LinkoExchangeContext> dbContext;
        Mock<ApplicationUserManager> userManagerObj;
        UserProfile userProfile;
        AuthenticationService _authenticationService;
        Dictionary<SettingType, string> settingDict = new Dictionary<SettingType, string>();
        Mock<IHttpContextService> _httpContext;
  

        [TestInitialize]
        public void TestInitialize()
        {
            userProfile = Mock.Of<UserProfile>(
                  i => i.UserProfileId == 1 &&
                  i.Id == "owin-user-id" &&
                  i.FirstName == "firstName" &&
                  i.LastName == "lastName" &&
                  i.IsAccountLocked == true &&
                  i.IsAccountResetRequired == false &&
                  i.IsIdentityProofed == true &&
                  i.UserName == "firstNameLastName" &&
                  i.UserProfileId == 1 &&
                  i.ZipCode == "zipcode" && 
                  i.Email == "test@watertrax.com" &&
                  i.Claims == new List<IdentityUserClaim>()
              );

            AutoMapperConfig.Setup(); 

            var userMock = Mock.Get(userProfile);
            userMock.SetupProperty(i => i.Id, "owin-user-id");

            var userStore = new Mock<IUserStore<UserProfile>>();
            userManagerObj = new Mock<ApplicationUserManager>(userStore.Object);
            signmanager = new Mock<ApplicationSignInManager>(userManagerObj.Object, authManger.Object);
            dbContext = new Mock<LinkoExchangeContext>(connectionString);
            IList<Claim> claims = new List<Claim>();
            var tc = Task.FromResult(claims);
            userManagerObj.Setup(p => p.GetClaimsAsync(It.IsAny<string>())).Returns(tc);

            // Set up for setting service
            var settingMock = Mock.Get(settService);
            settingDict = new Dictionary<SettingType, string>();

            settingDict.Add(SettingType.PasswordRequireLength, "6");
            settingDict.Add(SettingType.PasswordRequireDigit, "true");
            settingDict.Add(SettingType.PasswordExpiredDays, "90");
            settingDict.Add(SettingType.FailedPasswordAttemptMaxCount, "1");

            settingDict.Add(SettingType.PasswordHistoryMaxCount, "10");
            settingDict.Add(SettingType.EmailServer, "6");
            settingDict.Add(SettingType.SupportPhoneNumber, "+1-604-418-3201");
            settingDict.Add(SettingType.SupportEmail, "support@linkoExchange.com");
            settingDict.Add(SettingType.SystemEmailEmailAddress, "shuhao.wang@watertrax.com");
            settingDict.Add(SettingType.SystemEmailFirstName, "LinkoExchange");
            settingDict.Add(SettingType.SystemEmailLastName, "System");

            settingMock.Setup(i => i.GetGlobalSettings()).Returns(settingDict); 

            _authenticationService = new AuthenticationService(
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
                sessionCache,
                requestCache,
                AutoMapper.Mapper.Instance,
                passwordHasher,
                httpContextService
                );

            userManagerObj.Setup(
                   p => p.FindByNameAsync(It.IsAny<string>())).
                           Returns(
                              Task.FromResult(userProfile)
                           );
        }


        [TestMethod]
        public void Return_UserNotFound()
        {
            userProfile = null;
            userManagerObj.Setup(
               p => p.FindByNameAsync(It.IsAny<string>())).
                       Returns(
                          Task.FromResult(userProfile)
                       );
            var result = _authenticationService.SignInByUserName("shuhao", "password", true);
            Assert.AreEqual(AuthenticationResult.UserNotFound, result.Result.AutehticationResult);
        }
        
        [TestMethod]
        public void Return_PasswordLockedOut()
        {
            userManagerObj.Setup(p => p.IsLockedOutAsync(It.IsAny<string>())).Returns(Task.FromResult(true));

            var result = _authenticationService.SignInByUserName("shuhao", "password", true);

            Assert.AreEqual(AuthenticationResult.PasswordLockedOut, result.Result.AutehticationResult);
        }

        [TestMethod]
        public void Return_UserIsLocked()
        {
            userProfile.IsAccountLocked = true;
            userManagerObj.Setup(
               p => p.FindByNameAsync(It.IsAny<string>())).
                       Returns(
                          Task.FromResult(userProfile)
                       );
            var result = _authenticationService.SignInByUserName("shuhao", "password", true);

            Assert.AreEqual(AuthenticationResult.UserIsLocked, result.Result.AutehticationResult);   
        } 

        /// <summary>
        /// Test for UC-29, 4.a, 5.a, 6.a
        /// </summary>
        [TestMethod]
        public void Return_RegistrationApprovalPending()
        {
            userProfile.IsAccountLocked = false; 

            // Setup for dbContext 
            var users = new List<UserProfile>
            {
                userProfile
            }.AsQueryable() ;

            var usersMock = new Mock<DbSet<UserProfile>>();
            usersMock.As<IQueryable<UserProfile>>().Setup(p => p.Provider).Returns(users.Provider);
            usersMock.As<IQueryable<UserProfile>>().Setup(p => p.Expression).Returns(users.Expression);
            usersMock.As<IQueryable<UserProfile>>().Setup(p => p.ElementType).Returns(users.ElementType);
            usersMock.As<IQueryable<UserProfile>>().Setup(p => p.GetEnumerator()).Returns(users.GetEnumerator());

            dbContext.Setup(p => p.Users).Returns(usersMock.Object);

            // Setup for program service  

            var orpu1 = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                   p => p.IsRegistrationApproved == false
                ); 

            var orpu2 = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                p => p.IsRegistrationApproved == false
              );

            var pObj = Mock.Get(progService);
            pObj.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>())).Returns(new[] {
                 orpu1, orpu2
            }); 

            var result = _authenticationService.SignInByUserName("shuhao", "password", true);

            Assert.AreEqual(AuthenticationResult.RegistrationApprovalPending, result.Result.AutehticationResult);
        } 

        /// <summary>
        /// Test for UC-29, 4.a, user doesn't have any approved and enabled programs
        /// </summary>
        [TestMethod]
        public void Return_RegistrationApprovalPending2()
        {
            userProfile.IsAccountLocked = false;
            // Setup for program service  
            var orpu1 = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                   p => p.IsRegistrationApproved == false
                );

            var orpu2 = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                p => p.IsRegistrationApproved == true &&
                    p.OrganizationRegulatoryProgramDto.IsEnabled == false
              );

            var pObj = Mock.Get(progService);
            pObj.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>())).Returns(new[] {
                 orpu1, orpu2
            });

            var result = _authenticationService.SignInByUserName("shuhao", "password", true);

            Assert.AreEqual(AuthenticationResult.RegistrationApprovalPending, result.Result.AutehticationResult);
        }

        /// <summary>
        /// Test for UC-29, 5.a, user account is disabled for all industries, and authorities, and not internal account
        /// </summary>
        [TestMethod]
        public void Return_UserIsDisabled()
        {
            userProfile.IsAccountLocked = false;
            userProfile.IsInternalAccount = false;

            // Setup for program service  
            var orpu1 = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                  p => p.IsRegistrationApproved == false
               );

            var orpu2 = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                p => p.IsRegistrationApproved == true &&
                     p.IsEnabled == false && 
                    p.OrganizationRegulatoryProgramDto.IsEnabled == true 
                   
              );

            var pObj = Mock.Get(progService);
            pObj.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>())).Returns(new[] {
                 orpu1, orpu2
            });

            var result = _authenticationService.SignInByUserName("shuhao", "password", true);

            Assert.AreEqual(AuthenticationResult.UserIsDisabled, result.Result.AutehticationResult);
        }


        /// <summary>
        /// Test for UC-29, 6.a, user doesn't have access to any enabled industry or authority 
        /// </summary>
        [TestMethod]
        public void Return_AccountIsNotAssociated()
        {
            userProfile.IsAccountLocked = false;
            userProfile.IsInternalAccount = false;

            // Setup for program service  
            var orpu1 = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                  p => p.IsRegistrationApproved == false
               );

            var orpu2 = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                p => p.IsRegistrationApproved == true &&
                     p.IsEnabled == true &&
                    p.OrganizationRegulatoryProgramDto.IsEnabled == false

              );

            var orpu3 = Mock.Of<OrganizationRegulatoryProgramUserDto>(
             p => p.IsRegistrationApproved == true &&
                  p.IsEnabled == false &&
                 p.OrganizationRegulatoryProgramDto.IsEnabled == true

           );

            var pObj = Mock.Get(progService);
            pObj.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>())).Returns(new[] {
                 orpu1, orpu2, orpu3
            });

            var result = _authenticationService.SignInByUserName("shuhao", "password", true);

            Assert.AreEqual(AuthenticationResult.AccountIsNotAssociated, result.Result.AutehticationResult);
        }


        [TestMethod]
        public void Return_PasswordExpired()
        {
            userProfile.IsAccountLocked = false;
            userProfile.IsInternalAccount = false;

            // Setup for dbContext 
            var passwordHistries = new List<UserPasswordHistory>
            {
                new UserPasswordHistory
                {
                    UserProfileId = userProfile.UserProfileId,
                    LastModificationDateTimeUtc = DateTime.Now.AddDays(-2)
                } 

            }.AsQueryable();

            var passwordHistryMock = new Mock<DbSet<UserPasswordHistory>>();
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.Provider).Returns(passwordHistries.Provider);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.Expression).Returns(passwordHistries.Expression);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.ElementType).Returns(passwordHistries.ElementType);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.GetEnumerator()).Returns(passwordHistries.GetEnumerator());

            dbContext.Setup(p => p.UserPasswordHistories).Returns(passwordHistryMock.Object);

            // Setup for Setting service 
            var orgSettingDto = Mock.Of<OrganizationSettingDto>(
                  i => i.OrganizationId == 1 &&
                      i.Settings == new List<SettingDto> {
                           new SettingDto
                           {
                               Type = SettingType.PasswordExpiredDays,
                               Value = "1"
                           }
                      }
                );
            var settings = new List<OrganizationSettingDto> { orgSettingDto }; 

            var setMock = Mock.Get(settService);
            setMock.Setup(i => i.GetOrganizationSettingsByIds(It.IsAny<IEnumerable<int>>())).Returns(settings);


            var orpu1 = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                  p => p.IsRegistrationApproved == false
               );

            var orpu2 = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                p => p.IsRegistrationApproved == true &&
                     p.IsEnabled == true &&
                    p.OrganizationRegulatoryProgramDto.IsEnabled == false

              );

            var orpu3 = Mock.Of<OrganizationRegulatoryProgramUserDto>(
             p => p.IsRegistrationApproved == true &&
                  p.IsEnabled == true &&
                 p.OrganizationRegulatoryProgramDto.IsEnabled == true

           );

            var progMock = Mock.Get(progService);
            progMock.Setup(i=>i.GetUserRegulatoryPrograms(It.IsAny<string>())).Returns(new[] {
                 orpu1, orpu2, orpu3
            });

            var result = _authenticationService.SignInByUserName("shuhao", "password", true);

            Assert.AreEqual(AuthenticationResult.PasswordExpired, result.Result.AutehticationResult);

            var userProfileId = sessionCache.GetValue(CacheKey.UserProfileId);
            Assert.AreEqual(userProfile.UserProfileId, userProfileId);
        }


        [TestMethod]
        public void Return_SignIn_Success()
        {
            userProfile.IsAccountLocked = false;
            userProfile.IsInternalAccount = false;

            // Setup for dbContext 
            var passwordHistries = new List<UserPasswordHistory>
            {
                new UserPasswordHistory
                {
                    UserProfileId = userProfile.UserProfileId,
                    LastModificationDateTimeUtc = DateTime.Now.AddDays(-1)
                }

            }.AsQueryable();

            var passwordHistryMock = new Mock<DbSet<UserPasswordHistory>>();
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.Provider).Returns(passwordHistries.Provider);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.Expression).Returns(passwordHistries.Expression);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.ElementType).Returns(passwordHistries.ElementType);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.GetEnumerator()).Returns(passwordHistries.GetEnumerator());

            dbContext.Setup(p => p.UserPasswordHistories).Returns(passwordHistryMock.Object);

            var settings = settingDict.Select(i => new SettingDto
            {
                Type = i.Key,
                Value = i.Value
            }).ToList(); 

            // Setup for Setting service 
            var orgSettingDto = Mock.Of<OrganizationSettingDto>(
                  i => i.OrganizationId == 1 &&
                      i.Settings == settings 
                      ); 

            var orgSettings = new List<OrganizationSettingDto> { orgSettingDto };

            var setMock = Mock.Get(settService);
            setMock.Setup(i => i.GetOrganizationSettingsByIds(It.IsAny<IEnumerable<int>>())).Returns(orgSettings); 

            var orpu = Mock.Of<OrganizationRegulatoryProgramUserDto>(
                    p => p.IsRegistrationApproved == true &&
                    p.IsEnabled == true &&
                    p.OrganizationRegulatoryProgramDto.IsEnabled == true

         );

            var progMock = Mock.Get(progService);
            progMock.Setup(i => i.GetUserRegulatoryPrograms(It.IsAny<string>())).Returns(new[] {orpu}); 

            signmanager.Setup(i => i.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), true,  true))
               .Returns(Task.FromResult(SignInStatus.Success)); 

            var result = _authenticationService.SignInByUserName("shuhao", "password", true);

            Assert.AreEqual(AuthenticationResult.Success, result.Result.AutehticationResult);
        }

        [TestMethod]
        public void Test_SetClaimsForOrgRegProgramSelection()
        {
            userManagerObj.Setup(p => p.FindByNameAsync(It.IsAny<string>())).
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
                new LinkoExchangeContext(connectionString),
                userService,
                sessionCache,
                requestCache,
                Mapper.Instance,
                passwordHasher,
                httpContextService
                );

            authService.SetClaimsForOrgRegProgramSelection(1);
        }
    }
}


public class HttpContextServiceMock : IHttpContextService
{
    private Dictionary<string, object> session = new Dictionary<string, object>();
    public System.Web.HttpContext Current()
    {
        throw new NotImplementedException();
    }

    public string GetRequestBaseUrl()
    {
        return "";
    }

    public object GetSessionValue(string key)
    {
        if(session.ContainsKey(key))
        {
            return session[key];
        }
        return "";
    }

    public void SetSessionValue(string key, object value)
    {
        session[key] = value;
    }
}