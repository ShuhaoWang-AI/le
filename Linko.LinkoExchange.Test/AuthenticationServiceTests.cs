using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.User;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NLog;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.TermCondition;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class AuthenticationServiceTests
    {
        string connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
        private string testEmailAddress = ConfigurationManager.AppSettings["EmailSenderFromEmail"];
        private string emailServer = ConfigurationManager.AppSettings["EmailServer"];
        private string userName = "Test User Name";

        ISettingService settService = Mock.Of<ISettingService>();
        IOrganizationService orgService = Mock.Of<IOrganizationService>();
        IProgramService progService = Mock.Of<IProgramService>();
        IInvitationService invitService = Mock.Of<IInvitationService>();
        IEmailService emailService = Mock.Of<IEmailService>();
        IPermissionService permService = Mock.Of<IPermissionService>();
        IUserService userService = Mock.Of<IUserService>();
        ISessionCache sessionCache = new SessionCache(new HttpContextServiceMock());
        private Mock<ITermConditionService> _termConditionService = new Mock<ITermConditionService>();
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
        Dictionary<SystemSettingType, string> systemSettingDict = new Dictionary<SystemSettingType, string>();
        Dictionary<SettingType, string> settingDict = new Dictionary<SettingType, string>();
        ILogger logger = Mock.Of<ILogger>();
        UserDto userInfo;
        string registrationToken = "TEST-REGISTRATION-TOKEN";
        private IQuestionAnswerService questionAnswerService = Mock.Of<IQuestionAnswerService>();
        Mock<IProgramService> progServiceMock;
        Mock<IPermissionService> permissionMock;
        Mock<IMapHelper> _mapHelper;
        Mock<ICromerrAuditLogService> _cromerrLog;

        [TestInitialize]
        public void TestInitialize()
        {
            userInfo = GetUserInfo();
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

            progServiceMock = Mock.Get(progService);
            permissionMock = Mock.Get(permService);

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
            var settingServiceMock = Mock.Get(settService);
            systemSettingDict = new Dictionary<SystemSettingType, string>();

            //settingDict.Add(SystemSettingType.FailedPasswordAttemptMaxCount, "1"); //Does not exist in system settings

            //settingDict.Add(SystemSettingType.PasswordHistoryMaxCount, "10"); //Does not exist in system settings 

            systemSettingDict.Add(SystemSettingType.EmailServer, emailServer);
            systemSettingDict.Add(SystemSettingType.SupportPhoneNumber, "+1-604-418-3201");
            systemSettingDict.Add(SystemSettingType.SupportEmailAddress, "support@linkoExchange.com");
            systemSettingDict.Add(SystemSettingType.SystemEmailEmailAddress, testEmailAddress);
            systemSettingDict.Add(SystemSettingType.SystemEmailFirstName, "LinkoExchange");
            systemSettingDict.Add(SystemSettingType.SystemEmailLastName, "System");


            settingServiceMock.Setup(i => i.GetGlobalSettings()).Returns(systemSettingDict);
            settingServiceMock.Setup(i => i.GetSettingTemplateValue(SettingType.PasswordHistoryMaxCount, It.IsAny<OrganizationTypeName>())).Returns("10");
            settingServiceMock.Setup(i => i.GetSettingTemplateValue(SettingType.PasswordChangeRequiredDays, It.IsAny<OrganizationTypeName>())).Returns("90");
            settingServiceMock.Setup(i => i.GetSettingTemplateValue(SettingType.FailedPasswordAttemptMaxCount, It.IsAny<OrganizationTypeName>())).Returns("3");

            //settingServiceMock.Setup(i => i.GetSettingTemplateValue(SettingType.PasswordHistoryMaxCount, OrganizationTypeName.Authority)).Returns("10");
            //settingServiceMock.Setup(i => i.GetSettingTemplateValue(SettingType.PasswordChangeRequiredDays, OrganizationTypeName.Authority)).Returns("90");
            //settingServiceMock.Setup(i => i.GetSettingTemplateValue(SettingType.FailedPasswordAttemptMaxCount, OrganizationTypeName.Authority)).Returns("3");

            _termConditionService.Setup(i => i.GetLatestTermConditionId()).Returns(1);
            _termConditionService.Setup(i => i.GetTermCondtionContent()).Returns("test content");

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
                passwordHasher,
                httpContextService,
                logger,
                questionAnswerService,
                _mapHelper.Object,
                _cromerrLog.Object,
                _termConditionService.Object
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
        public void ResetPassword_Test()
        {
            var result = _authenticationService.ResetPasswordAsync("TOKEN", 1, "test", 2, "AAAABBBB");
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
            }.AsQueryable();

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
            pObj.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>(), false)).Returns(new[] {
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
            pObj.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>(), false)).Returns(new[] {
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
            pObj.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>(), false)).Returns(new[] {
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
            pObj.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>(), false)).Returns(new[] {
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
                               TemplateName = SettingType.PasswordChangeRequiredDays,
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
            progMock.Setup(i => i.GetUserRegulatoryPrograms(It.IsAny<string>(), false)).Returns(new[] {
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
                TemplateName = i.Key,
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
            progMock.Setup(i => i.GetUserRegulatoryPrograms(It.IsAny<string>(), false)).Returns(new[] { orpu });

            signmanager.Setup(i => i.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), true, true))
               .Returns(Task.FromResult(SignInStatus.Success));

            var result = _authenticationService.SignInByUserName("shuhao", "password", true);

            Assert.AreEqual(AuthenticationResult.Success, result.Result.AutehticationResult);
        }

        [TestMethod]
        public void Test_Registrer_Failed_Return_NotAgreedTermsAndConditions()
        {
            userInfo.AgreeTermsAndConditions = false;
            var result = _authenticationService.Register(userInfo, registrationToken, null, null, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.NotAgreedTermsAndConditions, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_Password_Policy_Return_BadPassword1()
        {
            userInfo.Password = "1";
            var result = _authenticationService.Register(userInfo, registrationToken, null, null, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.BadPassword, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_Password_Policy_Return_BadPassword2()
        {
            userInfo.Password = "VERY long password that is not supported";
            var result = _authenticationService.Register(userInfo, registrationToken, null, null, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.BadPassword, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_Null_Return_BadUserProfileData()
        {
            var result = _authenticationService.Register(null, registrationToken, null, null, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.BadUserProfileData, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoFirstName_Return_BadUserProfileData()
        {
            userInfo.FirstName = null;
            var result = _authenticationService.Register(userInfo, registrationToken, null, null, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.BadUserProfileData, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoLastName_Return_BadUserProfileData()
        {
            userInfo.LastName = null;
            var result = _authenticationService.Register(userInfo, registrationToken, null, null, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.BadUserProfileData, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoUserName_Return_BadUserProfileData()
        {
            userInfo.UserName = null;
            var result = _authenticationService.Register(userInfo, registrationToken, null, null, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.BadUserProfileData, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoAddressLine1_Return_BadUserProfileData()
        {
            userInfo.AddressLine1 = "";
            var result = _authenticationService.Register(userInfo, registrationToken, null, null, RegistrationType.NewRegistration);
            Assert.AreEqual(RegistrationResult.BadUserProfileData, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoCityName_Return_BadUserProfileData()
        {
            userInfo.CityName = ""; ;
            var result = _authenticationService.Register(userInfo, registrationToken, null, null, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.BadUserProfileData, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoZipCode_Return_BadUserProfileData()
        {
            userInfo.ZipCode = ""; ;
            var result = _authenticationService.Register(userInfo, registrationToken, null, null, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.BadUserProfileData, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoEmail_Return_BadUserProfileData()
        {
            userInfo.Email = "";
            var result = _authenticationService.Register(userInfo, registrationToken, null, null, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.BadUserProfileData, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_Return_MisingKBQ()
        {
            var result = _authenticationService.Register(userInfo, registrationToken, null, null, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.BadUserProfileData, result.Result.Result);
        }


        [TestMethod]
        public void Test_Register_Failed_UserProfile_Return_MissingSecurityQuestion()
        {
            var result = _authenticationService.Register(userInfo, registrationToken, null, null, RegistrationType.NewRegistration);
            Assert.AreEqual(RegistrationResult.BadUserProfileData, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_Return_MisingKBQ2()
        {
            var kbqQuestions = CreateQuestions(QuestionTypeName.KBQ, 4);
            var sqQuestions = CreateQuestions(QuestionTypeName.SQ, 2);

            var result = _authenticationService.Register(userInfo, registrationToken, sqQuestions, kbqQuestions, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.MissingKBQ, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_Return_MissingSecurityQuestion2()
        {
            var kbqQuestions = CreateQuestions(QuestionTypeName.KBQ, 6);
            var sqQuestions = CreateQuestions(QuestionTypeName.SQ, 1);

            var result = _authenticationService.Register(userInfo, registrationToken, sqQuestions, kbqQuestions, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.MissingSecurityQuestion, result.Result.Result);
        }

        // Test for duplicate questions  
        [TestMethod]
        public void Test_Register_UserProfile_Return_DuplicatedKBQ()
        {
            var kbqQuestions = CreateQuestions(QuestionTypeName.KBQ, 6);
            kbqQuestions.AddRange(CreateQuestions(QuestionTypeName.KBQ, 1));
            var sqQuestions = CreateQuestions(QuestionTypeName.SQ, 2);

            var result = _authenticationService.Register(userInfo, registrationToken, sqQuestions, kbqQuestions, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.DuplicatedKBQ, result.Result.Result);
        }

        // Test for duplicate answers  
        [TestMethod]
        public void Test_Register_Failed_UserProfile_Return_DuplicatedSecurityQuestion()
        {
            var kbqQuestions = CreateQuestions(QuestionTypeName.KBQ, 6);
            var sqQuestions = CreateQuestions(QuestionTypeName.SQ, 1);
            sqQuestions.AddRange(CreateQuestions(QuestionTypeName.SQ, 1));

            var result = _authenticationService.Register(userInfo, registrationToken, sqQuestions, kbqQuestions, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.DuplicatedSecurityQuestion, result.Result.Result);
        }

        // Test for null inivtation 
        [TestMethod]
        public void Test_Register_Failed_Return_InvalidRegistrationToken()
        {
            var kbqQuestions = CreateQuestions(QuestionTypeName.KBQ, 5);
            var sqQuestions = CreateQuestions(QuestionTypeName.SQ, 3);

            var invitServiceMock = Mock.Get(invitService);
            invitServiceMock.Setup(i => i.GetInvitation(It.IsAny<string>())).Returns((InvitationDto)null);

            var result = _authenticationService.Register(userInfo, registrationToken, sqQuestions, kbqQuestions, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.InvalidRegistrationToken, result.Result.Result);
        }

        // Test for expired invitation  
        [TestMethod]
        public void Test_Register_Failed_Return_Expired_Invitation()
        {
            var kbqQuestions = CreateQuestions(QuestionTypeName.KBQ, 5);
            var sqQuestions = CreateQuestions(QuestionTypeName.SQ, 3);

            // set invitation 5 days ago
            var invitationDto = Mock.Of<InvitationDto>(i => i.InvitationDateTimeUtc == DateTimeOffset.UtcNow.AddDays(-5)
                && i.RecipientOrganizationRegulatoryProgramId == 1000
            );

            var invitServiceMock = Mock.Get(invitService);
            invitServiceMock.Setup(i => i.GetInvitation(It.IsAny<string>())).Returns(invitationDto);

            // set invitationRecipientProgram 
            var orgRegulatoryProgramDto = Mock.Of<OrganizationRegulatoryProgramDto>(i => i.OrganizationId == 2000);

            // set prgramService
            var progServiceMock = Mock.Get(progService);
            progServiceMock.Setup(i => i.GetOrganizationRegulatoryProgram(1000)).Returns(orgRegulatoryProgramDto);


            // set setting service 
            var settings = new List<SettingDto>();
            settings.AddRange(
                new[] { new SettingDto
                        {
                            TemplateName = SettingType.InvitationExpiredHours,
                            Value = "72"
                        },

                        new SettingDto
                        {
                            TemplateName = SettingType.PasswordHistoryMaxCount,
                            Value = "10"
                        }
                });

            var orgSettingDto = Mock.Of<OrganizationSettingDto>(i => i.Settings == settings);
            var settingServiceMock = Mock.Get(settService);
            settingServiceMock.Setup(i => i.GetOrganizationSettingsById(It.IsAny<int>()))
                .Returns(orgSettingDto);


            settingServiceMock.Setup(i => i.GetOrganizationSettingsByIds(It.IsAny<IEnumerable<int>>()))
               .Returns(new[] { orgSettingDto });

            var result = _authenticationService.Register(userInfo, registrationToken, sqQuestions, kbqQuestions, RegistrationType.NewRegistration);

            Assert.AreEqual(RegistrationResult.InvitationExpired, result.Result.Result);
        }

        // Test new user register
        [TestMethod]
        public void Test_Register_CreateOrganizationRegulatoryProgramForUser()
        {
            IEnumerable<AnswerDto> sqQuestions;
            IEnumerable<AnswerDto> kbqQuestions;

            SetRegistrations(out sqQuestions, out kbqQuestions);

            userManagerObj.Setup(i => i.FindByNameAsync(It.IsAny<string>())).Returns(Task.FromResult((UserProfile)null));

            IdentityResult createUserResult = IdentityResult.Success;

            userManagerObj.Setup(i => i.CreateAsync(It.IsAny<UserProfile>(), It.IsAny<string>())).
                Returns(Task.FromResult(createUserResult));

            userManagerObj.Setup(i => i.FindByIdAsync(It.IsAny<string>())).
                Returns(Task.FromResult(userProfile));

            var result = _authenticationService.Register(userInfo, registrationToken, sqQuestions, kbqQuestions, RegistrationType.NewRegistration);

            userManagerObj.Verify(i => i.CreateAsync(It.IsAny<UserProfile>(), It.IsAny<string>()));

            var qaServiceMock = Mock.Get(questionAnswerService);
            qaServiceMock.Verify(i => i.CreateUserQuestionAnswers(It.IsAny<int>(), It.IsAny<IEnumerable<AnswerDto>>()), Times.Once);
            progServiceMock.Verify(i => i.CreateOrganizationRegulatoryProgramForUser(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), RegistrationType.NewRegistration));
            permissionMock.Verify(i => i.GetApprovalPeople(It.IsAny<int>()));
        }

        // Test new existing user re-register
        [TestMethod]
        public void Test_Register_re_registration()
        {
            IEnumerable<AnswerDto> sqQuestions;
            IEnumerable<AnswerDto> kbqQuestions;

            SetRegistrations(out sqQuestions, out kbqQuestions);

            var result = _authenticationService.Register(userInfo, registrationToken, sqQuestions, kbqQuestions, RegistrationType.ReRegistration);

            var settingServiceMock = Mock.Get(settService);
            settingServiceMock.Verify(i => i.GetOrganizationSettingsByIds(It.IsAny<IEnumerable<int>>()));

            var qaServiceMock = Mock.Get(questionAnswerService);
            qaServiceMock.Verify(i => i.CreateUserQuestionAnswers(It.IsAny<int>(), It.IsAny<IEnumerable<AnswerDto>>()), Times.Once);
            qaServiceMock.Verify(i => i.DeleteUserQuestionAndAnswers(It.IsAny<int>()));

            progServiceMock.Verify(i => i.CreateOrganizationRegulatoryProgramForUser(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), RegistrationType.ReRegistration));
            permissionMock.Verify(i => i.GetApprovalPeople(It.IsAny<int>()));
        }

        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void Test_Register_CreateUser_Failed_Throw_Exception()
        {
            IEnumerable<AnswerDto> sqQuestions;
            IEnumerable<AnswerDto> kbqQuestions;

            SetRegistrations(out sqQuestions, out kbqQuestions);

            // To test new user fails   
            userManagerObj.Setup(i => i.FindByNameAsync(It.IsAny<string>())).Returns(Task.FromResult((UserProfile)null));

            IdentityResult createUserResult = null;

            userManagerObj.Setup(i => i.CreateAsync(It.IsAny<UserProfile>(), It.IsAny<string>())).
                Returns(Task.FromResult((IdentityResult)createUserResult));

            var ret = _authenticationService.Register(userInfo, registrationToken, sqQuestions, kbqQuestions, RegistrationType.NewRegistration).Result;
        }

        [TestMethod]
        public void Test_Register_SenderProgram_disabled_return_expired()
        {
            IEnumerable<AnswerDto> sqQuestions;
            IEnumerable<AnswerDto> kbqQuestions;

            SetRegistrations(out sqQuestions, out kbqQuestions);

            // sender program is disabled 
            progServiceMock.Setup(i => i.GetOrganizationRegulatoryProgram(1001)).Returns(
                Mock.Of<OrganizationRegulatoryProgramDto>(
                      i => i.IsEnabled == false)
                    );

            // To test new user register    
            var result = _authenticationService.Register(userInfo, registrationToken, sqQuestions, kbqQuestions, RegistrationType.NewRegistration);
            Assert.AreEqual(RegistrationResult.InvitationExpired, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_RecipientProgram_disabled_return_expired()
        {
            IEnumerable<AnswerDto> sqQuestions;
            IEnumerable<AnswerDto> kbqQuestions;

            SetRegistrations(out sqQuestions, out kbqQuestions);

            // sender program is disabled 
            progServiceMock.Setup(i => i.GetOrganizationRegulatoryProgram(1000)).Returns(
                Mock.Of<OrganizationRegulatoryProgramDto>(
                      i => i.IsEnabled == false)
                    );

            // To test new user register    
            var result = _authenticationService.Register(userInfo, registrationToken, sqQuestions, kbqQuestions, RegistrationType.NewRegistration);
            Assert.AreEqual(RegistrationResult.InvitationExpired, result.Result.Result);
        }

        [TestMethod]
        public void Test_Register_all_good()
        {
            IEnumerable<AnswerDto> sqQuestions;
            IEnumerable<AnswerDto> kbqQuestions;

            SetRegistrations(out sqQuestions, out kbqQuestions);

            // To test new user register    
            var result = _authenticationService.Register(userInfo, registrationToken, sqQuestions, kbqQuestions, RegistrationType.NewRegistration);
            var emailServiceMock = Mock.Get(emailService);

            emailServiceMock.Verify(i => i.SendEmail(It.IsAny<IEnumerable<string>>(),
                It.IsAny<EmailType>(), It.IsAny<IDictionary<string, string>>(), true));

            var invitServiceMock = Mock.Get(invitService);

            invitServiceMock.Verify(i => i.DeleteInvitation(It.IsAny<string>(), It.IsAny<bool>()));

            Assert.AreEqual(RegistrationResult.Success, result.Result.Result);

        }

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
                passwordHasher,
                httpContextService,
                logger,
                null,
                _mapHelper.Object,
                _cromerrLog.Object,
                _termConditionService.Object
                );

            authService.SetClaimsForOrgRegProgramSelection(1);
        }


        private void SetRegistrations(out IEnumerable<AnswerDto> sqQuestions, out IEnumerable<AnswerDto> kbqQuestions)
        {
            kbqQuestions = CreateQuestions(QuestionTypeName.KBQ, 5);
            sqQuestions = CreateQuestions(QuestionTypeName.SQ, 3);

            // set invitation 5 days ago
            var invitationDto = Mock.Of<InvitationDto>(i => i.InvitationDateTimeUtc == DateTimeOffset.UtcNow.AddDays(-5)
                && i.RecipientOrganizationRegulatoryProgramId == 1000 &&
                   i.SenderOrganizationRegulatoryProgramId == 1001
            );

            var invitServiceMock = Mock.Get(invitService);
            invitServiceMock.Setup(i => i.GetInvitation(It.IsAny<string>())).Returns(invitationDto);

            // set invitationRecipientProgram 
            var orgRegulatoryProgramDto = Mock.Of<OrganizationRegulatoryProgramDto>(i => i.OrganizationId == 2000
                 && i.IsEnabled == true
                 && i.RegulatorOrganizationId == 1000
                 && i.OrganizationDto == Mock.Of<OrganizationDto>(b => b.OrganizationId == 5000)
            );

            // set prgramService 
            // recipient
            progServiceMock.Setup(i => i.GetOrganizationRegulatoryProgram(1000)).Returns(orgRegulatoryProgramDto);

            // sender 
            progServiceMock.Setup(i => i.GetOrganizationRegulatoryProgram(1001)).Returns(
                Mock.Of<OrganizationRegulatoryProgramDto>(
                      i => i.IsEnabled == true
                      )
                    );

            var orgServiceMock = Mock.Get(orgService);
            orgServiceMock.Setup(i => i.GetOrganization(It.IsAny<int>())).Returns(
                    Mock.Of<OrganizationDto>(i => i.OrganizationName == "test-org-name")
                );

            orgServiceMock.Setup(i => i.GetUserOrganizations(It.IsAny<int>())).
                Returns(
                  new[]
                  {
                      Mock.Of<OrganizationRegulatoryProgramDto>(i=>i.OrganizationId == 1000),
                      Mock.Of<OrganizationRegulatoryProgramDto>(i=>i.OrganizationId == 1001)
                  });

            progServiceMock.Setup(i => i.CreateOrganizationRegulatoryProgramForUser(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), RegistrationType.NewRegistration)).Returns(
                   Mock.Of<OrganizationRegulatoryProgramUserDto>(i => i.IsEnabled == true && i.OrganizationRegulatoryProgramId == 100
                   )
                );

            permissionMock.Setup(i => i.GetApprovalPeople(It.IsAny<int>())).Returns(
                 new[]{
                           Mock.Of<UserDto>(i=>i.Email=="test@water.com"),
                           Mock.Of<UserDto>(i=>i.Email=="test2@water.com"),
                       }
                );

            // set setting service 
            var settings = new List<SettingDto>();
            settings.AddRange(
                new[] {
                    new SettingDto
                    {
                        TemplateName = SettingType.InvitationExpiredHours,
                        Value = "172"
                    },
                    new SettingDto
                    {
                        TemplateName = SettingType.PasswordHistoryMaxCount,
                        Value="10"
                    }});


            var orgSettingDto = Mock.Of<OrganizationSettingDto>(i => i.Settings == settings);
            var settingServiceMock = Mock.Get(settService);
            settingServiceMock.Setup(i => i.GetOrganizationSettingsById(It.IsAny<int>()))
                .Returns(orgSettingDto);

            var orgSettings = new List<OrganizationSettingDto> { orgSettingDto };

            settingServiceMock.Setup(i => i.GetOrganizationSettingsByIds(It.IsAny<IEnumerable<int>>())).Returns(orgSettings);

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

        }

        private UserDto GetUserInfo()
        {
            return new UserDto()
            {
                UserName = "test-user-name",
                FirstName = "test",
                LastName = "last",
                AddressLine1 = "addreess line1",
                CityName = "City name",
                Email = "test@test.com",
                ZipCode = "zipcode",
                AgreeTermsAndConditions = true,
                Password = "123456789"
            };
        }

        private QuestionDto CreateQuestion(int questionId, QuestionTypeName type, string content)
        {
            return new QuestionDto
            {
                QuestionId = questionId,
                QuestionType = type,
                Content = content
            };
        }

        private AnswerDto CreateAnswer(string content)
        {
            return new AnswerDto
            {
                Content = content
            };
        }

        private QuestionAnswerPairDto CreateQuestion(QuestionDto question, AnswerDto answer)
        {
            return new QuestionAnswerPairDto
            {
                Question = question,
                Answer = answer
            };
        }

        private List<AnswerDto> CreateQuestions(IEnumerable<QuestionDto> questions, IEnumerable<AnswerDto> answers)
        {
            var qap = new List<AnswerDto>();
            for (var i = 0; i < questions.Count(); i++)
            {
                var answer = answers.ElementAt(i);
                var question = questions.ElementAt(i);

                qap.Add(new AnswerDto() { QuestionId = question.QuestionId.Value, Content = answer.Content });
            }

            return qap;
        }

        public List<AnswerDto> CreateQuestions(QuestionTypeName type, int count)
        {
            var qformat = type.ToString() + " #{0}";
            var aformat = type.ToString() + " #{0} answer.";

            var qaParies = new List<QuestionAnswerPairDto>();

            var questions = new List<QuestionDto>();
            var answers = new List<AnswerDto>();

            for (var i = 0; i < count; i++)
            {
                questions.Add(new QuestionDto
                {
                    QuestionId = i,
                    QuestionType = type,
                    Content = string.Format(qformat, i)
                });

                answers.Add(new AnswerDto
                {
                    Content = string.Format(aformat, i)
                });
            }

            return CreateQuestions(questions, answers);
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
        if (session.ContainsKey(key))
        {
            return session[key];
        }
        return "";
    }

    public void SetSessionValue(string key, object value)
    {
        session[key] = value;
    }

    public int CurrentUserProfileId()
    {
        return 1;
    }

    public string CurrentUserIPAddress()
    {
        return "000.000.000.000";
    }

    public string CurrentUserHostName()
    {
        return "test.dns.hostname";
    }

    public string GetClaimValue(string claimType)
    {
        throw new NotImplementedException();
    }
}