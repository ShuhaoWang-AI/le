using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TermCondition;
using Linko.LinkoExchange.Services.User;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
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
    public class AuthenticationServiceTests
    {
        #region fields

        private readonly Mock<IAuthenticationManager> _authManger = new Mock<IAuthenticationManager>();
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;

        private readonly Mock<ICromerrAuditLogService> _cromerrLog = new Mock<ICromerrAuditLogService>();
        private readonly string _emailServer = ConfigurationManager.AppSettings[name:"EmailServer"];
        private readonly IHttpContextService _httpContextService = Mock.Of<IHttpContextService>();
        private readonly IInvitationService _invitService = Mock.Of<IInvitationService>();
        private readonly Mock<ILinkoExchangeEmailService> _linkoExchangeEmailService = new Mock<ILinkoExchangeEmailService>();
        private readonly ILogger _logger = Mock.Of<ILogger>();
        private readonly Mock<IMapHelper> _mapHelper = new Mock<IMapHelper>();

        private readonly IOrganizationService _orgService = Mock.Of<IOrganizationService>();

        //Mock.Of<ISessionCache>();

        private readonly IPasswordHasher _passwordHasher = new PasswordHasher();
        private readonly IPermissionService _permService = Mock.Of<IPermissionService>();
        private readonly IProgramService _progService = Mock.Of<IProgramService>();
        private readonly IQuestionAnswerService _questionAnswerService = Mock.Of<IQuestionAnswerService>();
        private readonly string _registrationToken = "TEST-REGISTRATION-TOKEN";
        private readonly IRequestCache _requestCache = Mock.Of<IRequestCache>();
        private readonly ISessionCache _sessionCache = new SessionCache(httpContext:new HttpContextServiceMock());
        private readonly Dictionary<SettingType, string> _settingDict = new Dictionary<SettingType, string>();

        private readonly ISettingService _settService = Mock.Of<ISettingService>();
        private readonly Mock<ITermConditionService> _termConditionService = new Mock<ITermConditionService>();
        private readonly string _testEmailAddress = ConfigurationManager.AppSettings[name:"EmailSenderFromEmail"];
        private readonly IUserService _userService = Mock.Of<IUserService>();

        private AuthenticationService _authenticationService;
        private Mock<LinkoExchangeContext> _dbContext;
        private Mock<IPermissionService> _permissionMock;
        private Mock<IProgramService> _progServiceMock;
        private Mock<ApplicationSignInManager> _signmanager;
        private Dictionary<SystemSettingType, string> _systemSettingDict = new Dictionary<SystemSettingType, string>();
        private UserDto _userInfo;
        private Mock<ApplicationUserManager> _userManagerObj;

        private UserProfile _userProfile;

        #endregion

        [TestInitialize]
        public void TestInitialize()
        {
            _userInfo = GetUserInfo();
            _userProfile = Mock.Of<UserProfile>(
                                                i => i.UserProfileId == 1
                                                     && i.Id == "owin-user-id"
                                                     && i.FirstName == "firstName"
                                                     && i.LastName == "lastName"
                                                     && i.IsAccountLocked
                                                     && i.IsAccountResetRequired == false
                                                     && i.IsIdentityProofed
                                                     && i.UserName == "firstNameLastName"
                                                     && i.UserProfileId == 1
                                                     && i.ZipCode == "zipcode"
                                                     && i.Email == "test@watertrax.com"
                                                     && i.Claims == new List<IdentityUserClaim>()
                                               );

            _progServiceMock = Mock.Get(mocked:_progService);
            _permissionMock = Mock.Get(mocked:_permService);

            var userMock = Mock.Get(mocked:_userProfile);
            userMock.SetupProperty(i => i.Id, initialValue:"owin-user-id");

            var userStore = new Mock<IUserStore<UserProfile>>();
            _userManagerObj = new Mock<ApplicationUserManager>(userStore.Object);
            _signmanager = new Mock<ApplicationSignInManager>(_userManagerObj.Object, _authManger.Object);
            _dbContext = new Mock<LinkoExchangeContext>(_connectionString);

            IList<Claim> claims = new List<Claim>();
            var tc = Task.FromResult(result:claims);

            _userManagerObj.Setup(p => p.GetClaimsAsync(It.IsAny<string>())).Returns(value:tc);

            // Set up for setting service
            var settingServiceMock = Mock.Get(mocked:_settService);
            _systemSettingDict = new Dictionary<SystemSettingType, string>();

            //settingDict.Add(SystemSettingType.FailedPasswordAttemptMaxCount, "1"); //Does not exist in system settings

            //settingDict.Add(SystemSettingType.PasswordHistoryMaxCount, "10"); //Does not exist in system settings 

            _systemSettingDict.Add(key:SystemSettingType.EmailServer, value:_emailServer);
            _systemSettingDict.Add(key:SystemSettingType.SupportPhoneNumber, value:"+1-604-418-3201");
            _systemSettingDict.Add(key:SystemSettingType.SupportEmailAddress, value:"support@linkoExchange.com");
            _systemSettingDict.Add(key:SystemSettingType.SystemEmailEmailAddress, value:_testEmailAddress);
            _systemSettingDict.Add(key:SystemSettingType.SystemEmailFirstName, value:"LinkoExchange");
            _systemSettingDict.Add(key:SystemSettingType.SystemEmailLastName, value:"System");

            settingServiceMock.Setup(i => i.GetGlobalSettings()).Returns(value:_systemSettingDict);

            settingServiceMock.Setup(i => i.GetSettingTemplateValue(SettingType.PasswordHistoryMaxCount, It.IsAny<OrganizationTypeName>())).Returns(value:"10");

            settingServiceMock.Setup(i => i.GetSettingTemplateValue(SettingType.PasswordChangeRequiredDays, It.IsAny<OrganizationTypeName>())).Returns(value:"90");

            settingServiceMock.Setup(i => i.GetSettingTemplateValue(SettingType.FailedPasswordAttemptMaxCount, It.IsAny<OrganizationTypeName>())).Returns(value:"3");

            //settingServiceMock.Setup(i => i.GetSettingTemplateValue(SettingType.PasswordHistoryMaxCount, OrganizationTypeName.Authority)).Returns("10");
            //settingServiceMock.Setup(i => i.GetSettingTemplateValue(SettingType.PasswordChangeRequiredDays, OrganizationTypeName.Authority)).Returns("90");
            //settingServiceMock.Setup(i => i.GetSettingTemplateValue(SettingType.FailedPasswordAttemptMaxCount, OrganizationTypeName.Authority)).Returns("3");

            _termConditionService.Setup(i => i.GetLatestTermConditionId()).Returns(value:1);
            _termConditionService.Setup(i => i.GetTermConditionContent()).Returns(value:"test content");

            _authenticationService = new AuthenticationService(
                                                               userManager:_userManagerObj.Object,
                                                               signInManager:_signmanager.Object,
                                                               authenticationManager:_authManger.Object,
                                                               settingService:_settService,
                                                               organizationService:_orgService,
                                                               programService:_progService,
                                                               invitationService:_invitService,
                                                               permissionService:_permService,
                                                               linkoExchangeContext:_dbContext.Object,
                                                               userService:_userService,
                                                               sessionCache:_sessionCache,
                                                               requestCache:_requestCache,
                                                               passwordHasher:_passwordHasher,
                                                               httpContext:_httpContextService,
                                                               logger:_logger,
                                                               questionAnswerService:_questionAnswerService,
                                                               mapHelper:_mapHelper.Object,
                                                               crommerAuditLogService:_cromerrLog.Object,
                                                               termConditionService:_termConditionService.Object,
                                                               linkoExchangeEmailService:_linkoExchangeEmailService.Object
                                                              );

            _userManagerObj.Setup(
                                  p => p.FindByNameAsync(It.IsAny<string>())).Returns(value:Task.FromResult(result:_userProfile));
        }

        [TestMethod]
        public void Return_UserNotFound()
        {
            _userProfile = null;
            _userManagerObj.Setup(
                                  p => p.FindByNameAsync(It.IsAny<string>())).Returns(value:Task.FromResult(result:_userProfile));
            var result = _authenticationService.SignInByUserName(userName:"shuhao", password:"password", isPersistent:true);
            Assert.AreEqual(expected:AuthenticationResult.UserNotFound, actual:result.Result.AutehticationResult);
        }

        [TestMethod]
        public void Return_PasswordLockedOut()
        {
            _userManagerObj.Setup(p => p.IsLockedOutAsync(It.IsAny<string>())).Returns(value:Task.FromResult(result:true));

            var result = _authenticationService.SignInByUserName(userName:"shuhao", password:"password", isPersistent:true);

            Assert.AreEqual(expected:AuthenticationResult.PasswordLockedOut, actual:result.Result.AutehticationResult);
        }

        [TestMethod]
        public void ResetPassword_Test()
        {
            var result = _authenticationService.ResetPasswordAsync(resetPasswordToken:"TOKEN", userQuestionAnswerId:1, answer:"test", failedCount:2, newPassword:"AAAABBBB");
        }

        [TestMethod]
        public void Return_UserIsLocked()
        {
            _userProfile.IsAccountLocked = true;
            _userManagerObj.Setup(
                                  p => p.FindByNameAsync(It.IsAny<string>())).Returns(value:Task.FromResult(result:_userProfile));
            var result = _authenticationService.SignInByUserName(userName:"shuhao", password:"password", isPersistent:true);

            Assert.AreEqual(expected:AuthenticationResult.UserIsLocked, actual:result.Result.AutehticationResult);
        }

        /// <summary>
        ///     Test for UC-29, 4.a, 5.a, 6.a
        /// </summary>
        [TestMethod]
        public void Return_RegistrationApprovalPending()
        {
            _userProfile.IsAccountLocked = false;

            // Setup for dbContext 
            var users = new List<UserProfile>
                        {
                            _userProfile
                        }.AsQueryable();

            var usersMock = new Mock<DbSet<UserProfile>>();
            usersMock.As<IQueryable<UserProfile>>().Setup(p => p.Provider).Returns(value:users.Provider);
            usersMock.As<IQueryable<UserProfile>>().Setup(p => p.Expression).Returns(value:users.Expression);
            usersMock.As<IQueryable<UserProfile>>().Setup(p => p.ElementType).Returns(value:users.ElementType);
            usersMock.As<IQueryable<UserProfile>>().Setup(p => p.GetEnumerator()).Returns(value:users.GetEnumerator());

            _dbContext.Setup(p => p.Users).Returns(value:usersMock.Object);

            // Setup for program service  

            var orpu1 = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved == false);

            var orpu2 = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved == false);

            var pObj = Mock.Get(mocked:_progService);

            pObj.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>(), false, false)).Returns(value:new[]
                                                                                                         {
                                                                                                             orpu1, orpu2
                                                                                                         });

            var result = _authenticationService.SignInByUserName(userName:"shuhao", password:"password", isPersistent:true);

            Assert.AreEqual(expected:AuthenticationResult.RegistrationApprovalPending, actual:result.Result.AutehticationResult);
        }

        /// <summary>
        ///     Test for UC-29, 4.a, user doesn't have any approved and enabled programs
        /// </summary>
        [TestMethod]
        public void Return_RegistrationApprovalPending2()
        {
            _userProfile.IsAccountLocked = false;

            // Setup for program service  
            var orpu1 = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved == false);

            var orpu2 = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved && p.OrganizationRegulatoryProgramDto.IsEnabled == false);

            var pObj = Mock.Get(mocked:_progService);
            pObj.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>(), false, false)).Returns(value:new[] {orpu1, orpu2});

            var result = _authenticationService.SignInByUserName(userName:"shuhao", password:"password", isPersistent:true);

            Assert.AreEqual(expected:AuthenticationResult.RegistrationApprovalPending, actual:result.Result.AutehticationResult);
        }

        /// <summary>
        ///     Test for UC-29, 5.a, user account is disabled for all industries, and authorities, and not internal account
        /// </summary>
        [TestMethod]
        public void Return_UserIsDisabled()
        {
            _userProfile.IsAccountLocked = false;
            _userProfile.IsInternalAccount = false;

            // Setup for program service  
            var orpu1 = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved == false);

            var orpu2 = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved && p.IsEnabled == false && p.OrganizationRegulatoryProgramDto.IsEnabled);

            var pObj = Mock.Get(mocked:_progService);
            pObj.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>(), false, false)).Returns(value:new[] {orpu1, orpu2});

            var result = _authenticationService.SignInByUserName(userName:"shuhao", password:"password", isPersistent:true);

            Assert.AreEqual(expected:AuthenticationResult.UserIsDisabled, actual:result.Result.AutehticationResult);
        }

        /// <summary>
        ///     Test for UC-29, 6.a, user doesn't have access to any enabled industry or authority
        /// </summary>
        [TestMethod]
        public void Return_AccountIsNotAssociated()
        {
            _userProfile.IsAccountLocked = false;
            _userProfile.IsInternalAccount = false;

            // Setup for program service  
            var orpu1 = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved == false);

            var orpu2 = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved && p.IsEnabled && p.OrganizationRegulatoryProgramDto.IsEnabled == false);

            var orpu3 = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved && p.IsEnabled == false && p.OrganizationRegulatoryProgramDto.IsEnabled);

            var pObj = Mock.Get(mocked:_progService);
            pObj.Setup(p => p.GetUserRegulatoryPrograms(It.IsAny<string>(), false, false)).Returns(value:new[] {orpu1, orpu2, orpu3});

            var result = _authenticationService.SignInByUserName(userName:"shuhao", password:"password", isPersistent:true);

            Assert.AreEqual(expected:AuthenticationResult.AccountIsNotAssociated, actual:result.Result.AutehticationResult);
        }

        [TestMethod]
        public void Return_PasswordExpired()
        {
            _userProfile.IsAccountLocked = false;
            _userProfile.IsInternalAccount = false;

            // Setup for dbContext 
            var passwordHistries = new List<UserPasswordHistory>
                                   {
                                       new UserPasswordHistory
                                       {
                                           UserProfileId = _userProfile.UserProfileId,
                                           LastModificationDateTimeUtc = DateTime.Now.AddDays(value:-2)
                                       }
                                   }.AsQueryable();

            var passwordHistryMock = new Mock<DbSet<UserPasswordHistory>>();
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.Provider).Returns(value:passwordHistries.Provider);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.Expression).Returns(value:passwordHistries.Expression);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.ElementType).Returns(value:passwordHistries.ElementType);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.GetEnumerator()).Returns(value:passwordHistries.GetEnumerator());

            _dbContext.Setup(p => p.UserPasswordHistories).Returns(value:passwordHistryMock.Object);

            // Setup for Setting service 
            var orgSettingDto = Mock.Of<OrganizationSettingDto>(i => i.OrganizationId == 1
                                                                     && i.Settings
                                                                     == new List<SettingDto>
                                                                        {
                                                                            new SettingDto
                                                                            {
                                                                                TemplateName = SettingType.PasswordChangeRequiredDays,
                                                                                Value = "1"
                                                                            }
                                                                        }
                                                               );
            var settings = new List<OrganizationSettingDto> {orgSettingDto};

            var setMock = Mock.Get(mocked:_settService);
            setMock.Setup(i => i.GetOrganizationSettingsByIds(It.IsAny<IEnumerable<int>>())).Returns(value:settings);

            var orpu1 = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved == false);

            var orpu2 = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved && p.IsEnabled && p.OrganizationRegulatoryProgramDto.IsEnabled == false);

            var orpu3 = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved && p.IsEnabled && p.OrganizationRegulatoryProgramDto.IsEnabled);

            var progMock = Mock.Get(mocked:_progService);
            progMock.Setup(i => i.GetUserRegulatoryPrograms(It.IsAny<string>(), false, false)).Returns(value:new[] {orpu1, orpu2, orpu3});

            var result = _authenticationService.SignInByUserName(userName:"shuhao", password:"password", isPersistent:true);

            Assert.AreEqual(expected:AuthenticationResult.PasswordExpired, actual:result.Result.AutehticationResult);

            var userProfileId = _sessionCache.GetValue(key:CacheKey.UserProfileId);
            Assert.AreEqual(expected:_userProfile.UserProfileId, actual:userProfileId);
        }

        [TestMethod]
        public void Return_SignIn_Success()
        {
            _userProfile.IsAccountLocked = false;
            _userProfile.IsInternalAccount = false;

            // Setup for dbContext 
            var passwordHistries = new List<UserPasswordHistory>
                                   {
                                       new UserPasswordHistory
                                       {
                                           UserProfileId = _userProfile.UserProfileId,
                                           LastModificationDateTimeUtc = DateTime.Now.AddDays(value:-1)
                                       }
                                   }.AsQueryable();

            var passwordHistryMock = new Mock<DbSet<UserPasswordHistory>>();
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.Provider).Returns(value:passwordHistries.Provider);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.Expression).Returns(value:passwordHistries.Expression);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.ElementType).Returns(value:passwordHistries.ElementType);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.GetEnumerator()).Returns(value:passwordHistries.GetEnumerator());

            _dbContext.Setup(p => p.UserPasswordHistories).Returns(value:passwordHistryMock.Object);

            var settings = _settingDict.Select(i => new SettingDto
                                                    {
                                                        TemplateName = i.Key,
                                                        Value = i.Value
                                                    }).ToList();

            // Setup for Setting service 
            var orgSettingDto = Mock.Of<OrganizationSettingDto>(i => i.OrganizationId == 1 && i.Settings == settings);

            var orgSettings = new List<OrganizationSettingDto> {orgSettingDto};

            var setMock = Mock.Get(mocked:_settService);
            setMock.Setup(i => i.GetOrganizationSettingsByIds(It.IsAny<IEnumerable<int>>())).Returns(value:orgSettings);

            var orpu = Mock.Of<OrganizationRegulatoryProgramUserDto>(p => p.IsRegistrationApproved && p.IsEnabled && p.OrganizationRegulatoryProgramDto.IsEnabled);

            var progMock = Mock.Get(mocked:_progService);
            progMock.Setup(i => i.GetUserRegulatoryPrograms(It.IsAny<string>(), false, false)).Returns(value:new[] {orpu});

            _signmanager.Setup(i => i.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), true, true)).Returns(value:Task.FromResult(result:SignInStatus.Success));

            var result = _authenticationService.SignInByUserName(userName:"shuhao", password:"password", isPersistent:true);

            Assert.AreEqual(expected:AuthenticationResult.Success, actual:result.Result.AutehticationResult);
        }

        [TestMethod]
        public void Test_Registrer_Failed_Return_NotAgreedTermsAndConditions()
        {
            _userInfo.AgreeTermsAndConditions = false;
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.NotAgreedTermsAndConditions, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_Password_Policy_Return_BadPassword1()
        {
            _userInfo.Password = "1";
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.BadPassword, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_Password_Policy_Return_BadPassword2()
        {
            _userInfo.Password = "VERY long password that is not supported";
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.BadPassword, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_Null_Return_BadUserProfileData()
        {
            var result = _authenticationService.Register(userInfo:null, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.BadUserProfileData, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoFirstName_Return_BadUserProfileData()
        {
            _userInfo.FirstName = null;
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.BadUserProfileData, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoLastName_Return_BadUserProfileData()
        {
            _userInfo.LastName = null;
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.BadUserProfileData, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoUserName_Return_BadUserProfileData()
        {
            _userInfo.UserName = null;
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.BadUserProfileData, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoAddressLine1_Return_BadUserProfileData()
        {
            _userInfo.AddressLine1 = "";
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);
            Assert.AreEqual(expected:RegistrationResult.BadUserProfileData, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoCityName_Return_BadUserProfileData()
        {
            _userInfo.CityName = "";

            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.BadUserProfileData, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoZipCode_Return_BadUserProfileData()
        {
            _userInfo.ZipCode = "";

            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.BadUserProfileData, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_NoEmail_Return_BadUserProfileData()
        {
            _userInfo.Email = "";
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.BadUserProfileData, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_Return_MisingKBQ()
        {
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.BadUserProfileData, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_Return_MissingSecurityQuestion()
        {
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:null, kbqQuestions:null,
                                                         registrationType:RegistrationType.NewRegistration);
            Assert.AreEqual(expected:RegistrationResult.BadUserProfileData, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_Return_MisingKBQ2()
        {
            var kbqQuestions = CreateQuestions(type:QuestionTypeName.KBQ, count:4);
            var sqQuestions = CreateQuestions(type:QuestionTypeName.SQ, count:2);

            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:sqQuestions, kbqQuestions:kbqQuestions,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.MissingKBQ, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_Failed_UserProfile_Return_MissingSecurityQuestion2()
        {
            var kbqQuestions = CreateQuestions(type:QuestionTypeName.KBQ, count:6);
            var sqQuestions = CreateQuestions(type:QuestionTypeName.SQ, count:1);

            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:sqQuestions, kbqQuestions:kbqQuestions,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.MissingSecurityQuestion, actual:result.Result);
        }

        // Test for duplicate questions  
        [TestMethod]
        public void Test_Register_UserProfile_Return_DuplicatedKBQ()
        {
            var kbqQuestions = CreateQuestions(type:QuestionTypeName.KBQ, count:6);
            kbqQuestions.AddRange(collection:CreateQuestions(type:QuestionTypeName.KBQ, count:1));
            var sqQuestions = CreateQuestions(type:QuestionTypeName.SQ, count:2);

            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:sqQuestions, kbqQuestions:kbqQuestions,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.DuplicatedKBQ, actual:result.Result);
        }

        // Test for duplicate answers  
        [TestMethod]
        public void Test_Register_Failed_UserProfile_Return_DuplicatedSecurityQuestion()
        {
            var kbqQuestions = CreateQuestions(type:QuestionTypeName.KBQ, count:6);
            var sqQuestions = CreateQuestions(type:QuestionTypeName.SQ, count:1);
            sqQuestions.AddRange(collection:CreateQuestions(type:QuestionTypeName.SQ, count:1));

            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:sqQuestions, kbqQuestions:kbqQuestions,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.DuplicatedSecurityQuestion, actual:result.Result);
        }

        // Test for null invitation 
        [TestMethod]
        public void Test_Register_Failed_Return_InvalidRegistrationToken()
        {
            var kbqQuestions = CreateQuestions(type:QuestionTypeName.KBQ, count:5);
            var sqQuestions = CreateQuestions(type:QuestionTypeName.SQ, count:3);

            var invitServiceMock = Mock.Get(mocked:_invitService);
            invitServiceMock.Setup(i => i.GetInvitation(It.IsAny<string>())).Returns(value:null);

            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:sqQuestions, kbqQuestions:kbqQuestions,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.InvalidRegistrationToken, actual:result.Result);
        }

        // Test for expired invitation  
        [TestMethod]
        public void Test_Register_Failed_Return_Expired_Invitation()
        {
            var kbqQuestions = CreateQuestions(type:QuestionTypeName.KBQ, count:5);
            var sqQuestions = CreateQuestions(type:QuestionTypeName.SQ, count:3);

            // set invitation 5 days ago

            var invitationDto = Mock.Of<InvitationDto>(i => i.InvitationDateTimeUtc == DateTimeOffset.UtcNow.AddDays(-5) && i.RecipientOrganizationRegulatoryProgramId == 1000);

            var invitServiceMock = Mock.Get(mocked:_invitService);
            invitServiceMock.Setup(i => i.GetInvitation(It.IsAny<string>())).Returns(value:invitationDto);

            // set invitationRecipientProgram 
            var orgRegulatoryProgramDto = Mock.Of<OrganizationRegulatoryProgramDto>(i => i.OrganizationId == 2000);

            // set prgramService
            var progServiceMock = Mock.Get(mocked:_progService);

            progServiceMock.Setup(i => i.GetOrganizationRegulatoryProgram(1000)).Returns(value:orgRegulatoryProgramDto);

            // set setting service 
            var settings = new List<SettingDto>();
            settings.AddRange(
                              collection:new[]
                                         {
                                             new SettingDto
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
            var settingServiceMock = Mock.Get(mocked:_settService);
            settingServiceMock.Setup(i => i.GetOrganizationSettingsById(It.IsAny<int>())).Returns(value:orgSettingDto);

            settingServiceMock.Setup(i => i.GetOrganizationSettingsByIds(It.IsAny<IEnumerable<int>>())).Returns(value:new[] {orgSettingDto});

            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:sqQuestions, kbqQuestions:kbqQuestions,
                                                         registrationType:RegistrationType.NewRegistration);

            Assert.AreEqual(expected:RegistrationResult.InvitationExpired, actual:result.Result);
        }

        // Test new user register
        [TestMethod]
        public void Test_Register_CreateOrganizationRegulatoryProgramForUser()
        {
            IEnumerable<AnswerDto> sqQuestions;
            IEnumerable<AnswerDto> kbqQuestions;

            SetRegistrations(sqQuestions:out sqQuestions, kbqQuestions:out kbqQuestions);

            _userManagerObj.Setup(i => i.FindByNameAsync(It.IsAny<string>())).Returns(value:Task.FromResult(result:(UserProfile) null));

            var createUserResult = IdentityResult.Success;

            _userManagerObj.Setup(i => i.CreateAsync(It.IsAny<UserProfile>(), It.IsAny<string>())).Returns(value:Task.FromResult(result:createUserResult));

            _userManagerObj.Setup(i => i.FindByIdAsync(It.IsAny<string>())).Returns(value:Task.FromResult(result:_userProfile));

            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:sqQuestions, kbqQuestions:kbqQuestions,
                                                         registrationType:RegistrationType.NewRegistration);

            _userManagerObj.Verify(i => i.CreateAsync(It.IsAny<UserProfile>(), It.IsAny<string>()));

            var qaServiceMock = Mock.Get(mocked:_questionAnswerService);
            qaServiceMock.Verify(i => i.CreateUserQuestionAnswers(It.IsAny<int>(), It.IsAny<IEnumerable<AnswerDto>>()), times:Times.Once);
            _permissionMock.Verify(i => i.GetApprovalPeople(It.IsAny<OrganizationRegulatoryProgramDto>(), It.IsAny<bool>()));
        }

        // Test new existing user re-register
        [TestMethod]
        public void Test_Register_re_registration()
        {
            IEnumerable<AnswerDto> sqQuestions;
            IEnumerable<AnswerDto> kbqQuestions;

            SetRegistrations(sqQuestions:out sqQuestions, kbqQuestions:out kbqQuestions);

            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:sqQuestions, kbqQuestions:kbqQuestions,
                                                         registrationType:RegistrationType.ReRegistration);

            var settingServiceMock = Mock.Get(mocked:_settService);
            settingServiceMock.Verify(i => i.GetOrganizationSettingsByIds(It.IsAny<IEnumerable<int>>()));

            var qaServiceMock = Mock.Get(mocked:_questionAnswerService);
            qaServiceMock.Verify(i => i.CreateUserQuestionAnswers(It.IsAny<int>(), It.IsAny<IEnumerable<AnswerDto>>()), times:Times.Once);
            qaServiceMock.Verify(i => i.DeleteUserQuestionAndAnswers(It.IsAny<int>()));

            _permissionMock.Verify(i => i.GetApprovalPeople(It.IsAny<OrganizationRegulatoryProgramDto>(), It.IsAny<bool>()));
        }

        [TestMethod]
        [ExpectedException(exceptionType:typeof(AggregateException))]
        public void Test_Register_CreateUser_Failed_Throw_Exception()
        {
            IEnumerable<AnswerDto> sqQuestions;
            IEnumerable<AnswerDto> kbqQuestions;

            SetRegistrations(sqQuestions:out sqQuestions, kbqQuestions:out kbqQuestions);

            // To test new user fails   
            _userManagerObj.Setup(i => i.FindByNameAsync(It.IsAny<string>())).Returns(value:Task.FromResult(result:(UserProfile) null));

            IdentityResult createUserResult = null;

            _userManagerObj.Setup(i => i.CreateAsync(It.IsAny<UserProfile>(), It.IsAny<string>())).Returns(value:Task.FromResult(result:createUserResult));

            var ret = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:sqQuestions, kbqQuestions:kbqQuestions,
                                                      registrationType:RegistrationType.NewRegistration).Result;
        }

        [TestMethod]
        public void Test_Register_SenderProgram_disabled_return_expired()
        {
            IEnumerable<AnswerDto> sqQuestions;
            IEnumerable<AnswerDto> kbqQuestions;

            SetRegistrations(sqQuestions:out sqQuestions, kbqQuestions:out kbqQuestions);

            // sender program is disabled 
            _progServiceMock.Setup(i => i.GetOrganizationRegulatoryProgram(1001)).Returns(value:Mock.Of<OrganizationRegulatoryProgramDto>(i => i.IsEnabled == false));

            // To test new user register    
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:sqQuestions, kbqQuestions:kbqQuestions,
                                                         registrationType:RegistrationType.NewRegistration);
            Assert.AreEqual(expected:RegistrationResult.InvitationExpired, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_RecipientProgram_disabled_return_expired()
        {
            IEnumerable<AnswerDto> sqQuestions;
            IEnumerable<AnswerDto> kbqQuestions;

            SetRegistrations(sqQuestions:out sqQuestions, kbqQuestions:out kbqQuestions);

            // sender program is disabled 
            _progServiceMock.Setup(i => i.GetOrganizationRegulatoryProgram(1000)).Returns(value:Mock.Of<OrganizationRegulatoryProgramDto>(i => i.IsEnabled == false));

            // To test new user register    
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:sqQuestions, kbqQuestions:kbqQuestions,
                                                         registrationType:RegistrationType.NewRegistration);
            Assert.AreEqual(expected:RegistrationResult.InvitationExpired, actual:result.Result);
        }

        [TestMethod]
        public void Test_Register_all_good()
        {
            IEnumerable<AnswerDto> sqQuestions;
            IEnumerable<AnswerDto> kbqQuestions;

            SetRegistrations(sqQuestions:out sqQuestions, kbqQuestions:out kbqQuestions);

            // To test new user register    
            var result = _authenticationService.Register(userInfo:_userInfo, registrationToken:_registrationToken, securityQuestions:sqQuestions, kbqQuestions:kbqQuestions,
                                                         registrationType:RegistrationType.NewRegistration);

            var invitServiceMock = Mock.Get(mocked:_invitService);

            invitServiceMock.Verify(i => i.DeleteInvitation(It.IsAny<string>(), It.IsAny<bool>()));

            Assert.AreEqual(expected:RegistrationResult.Success, actual:result.Result);
        }

        [TestMethod]
        public void Test_SetClaimsForOrgRegProgramSelection()
        {
            _userManagerObj.Setup(p => p.FindByNameAsync(It.IsAny<string>())).Returns(value:Task.FromResult(result:_userProfile));

            var authService = new AuthenticationService(
                                                        userManager:_userManagerObj.Object,
                                                        signInManager:_signmanager.Object,
                                                        authenticationManager:_authManger.Object,
                                                        settingService:_settService,
                                                        organizationService:_orgService,
                                                        programService:_progService,
                                                        invitationService:_invitService,
                                                        permissionService:_permService,
                                                        linkoExchangeContext:new LinkoExchangeContext(nameOrConnectionString:_connectionString),
                                                        userService:_userService,
                                                        sessionCache:_sessionCache,
                                                        requestCache:_requestCache,
                                                        passwordHasher:_passwordHasher,
                                                        httpContext:_httpContextService,
                                                        logger:_logger,
                                                        questionAnswerService:null,
                                                        mapHelper:_mapHelper.Object,
                                                        crommerAuditLogService:_cromerrLog.Object,
                                                        termConditionService:_termConditionService.Object,
                                                        linkoExchangeEmailService:_linkoExchangeEmailService.Object
                                                       );

            authService.SetClaimsForOrgRegProgramSelection(orgRegProgId:1);
        }

        private void SetRegistrations(out IEnumerable<AnswerDto> sqQuestions, out IEnumerable<AnswerDto> kbqQuestions)
        {
            kbqQuestions = CreateQuestions(type:QuestionTypeName.KBQ, count:5);
            sqQuestions = CreateQuestions(type:QuestionTypeName.SQ, count:3);

            // set invitation 5 days ago
            var invitationDto = Mock.Of<InvitationDto>(i => i.InvitationDateTimeUtc == DateTimeOffset.UtcNow.AddDays(-5)
                                                            && i.RecipientOrganizationRegulatoryProgramId == 1000
                                                            && i.SenderOrganizationRegulatoryProgramId == 1001
                                                      );

            var invitServiceMock = Mock.Get(mocked:_invitService);
            invitServiceMock.Setup(i => i.GetInvitation(It.IsAny<string>())).Returns(value:invitationDto);

            // set invitationRecipientProgram 
            var orgRegulatoryProgramDto = Mock.Of<OrganizationRegulatoryProgramDto>(i => i.OrganizationId == 2000
                                                                                         && i.IsEnabled
                                                                                         && i.RegulatorOrganizationId == 1000
                                                                                         && i.OrganizationDto == Mock.Of<OrganizationDto>(b => b.OrganizationId == 5000)
                                                                                   );

            // set prgramService 
            // recipient
            _progServiceMock.Setup(i => i.GetOrganizationRegulatoryProgram(1000)).Returns(value:orgRegulatoryProgramDto);

            // sender 
            _progServiceMock.Setup(i => i.GetOrganizationRegulatoryProgram(1001)).Returns(value:Mock.Of<OrganizationRegulatoryProgramDto>(i => i.IsEnabled));

            var orgServiceMock = Mock.Get(mocked:_orgService);
            orgServiceMock.Setup(i => i.GetOrganization(It.IsAny<int>())).Returns(value:Mock.Of<OrganizationDto>(i => i.OrganizationName == "test-org-name"));

            orgServiceMock.Setup(i => i.GetUserOrganizations(It.IsAny<int>())).Returns(value:new[]
                                                                                             {
                                                                                                 Mock.Of<OrganizationRegulatoryProgramDto>(i => i.OrganizationId == 1000),
                                                                                                 Mock.Of<OrganizationRegulatoryProgramDto>(i => i.OrganizationId == 1001)
                                                                                             });

            orgServiceMock.Setup(i => i.GetAuthority(It.IsAny<int>())).Returns(value:Mock.Of<OrganizationRegulatoryProgramDto>(i => i.OrganizationId == 1000));

            _permissionMock.Setup(i => i.GetApprovalPeople(It.IsAny<OrganizationRegulatoryProgramDto>(), It.IsAny<bool>()))
                           .Returns(value:new[]
                                          {
                                              Mock.Of<UserDto>(i => i.Email == "test@water.com"),
                                              Mock.Of<UserDto>(i => i.Email == "test2@water.com")
                                          }
                                   );

            // set setting service 
            var settings = new List<SettingDto>();
            settings.AddRange(collection:new[]
                                         {
                                             new SettingDto
                                             {
                                                 TemplateName = SettingType.InvitationExpiredHours,
                                                 Value = "172"
                                             },
                                             new SettingDto
                                             {
                                                 TemplateName = SettingType.PasswordHistoryMaxCount,
                                                 Value = "10"
                                             }
                                         });

            var orgSettingDto = Mock.Of<OrganizationSettingDto>(i => i.Settings == settings);
            var settingServiceMock = Mock.Get(mocked:_settService);
            settingServiceMock.Setup(i => i.GetOrganizationSettingsById(It.IsAny<int>())).Returns(value:orgSettingDto);

            var orgSettings = new List<OrganizationSettingDto> {orgSettingDto};

            settingServiceMock.Setup(i => i.GetOrganizationSettingsByIds(It.IsAny<IEnumerable<int>>())).Returns(value:orgSettings);

            // Setup for dbContext 
            var passwordHistries = new List<UserPasswordHistory>
                                   {
                                       new UserPasswordHistory
                                       {
                                           UserProfileId = _userProfile.UserProfileId,
                                           LastModificationDateTimeUtc = DateTime.Now.AddDays(value:-1)
                                       }
                                   }.AsQueryable();

            var passwordHistryMock = new Mock<DbSet<UserPasswordHistory>>();
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.Provider).Returns(value:passwordHistries.Provider);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.Expression).Returns(value:passwordHistries.Expression);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.ElementType).Returns(value:passwordHistries.ElementType);
            passwordHistryMock.As<IQueryable<UserPasswordHistory>>().Setup(p => p.GetEnumerator()).Returns(value:passwordHistries.GetEnumerator());

            _dbContext.Setup(p => p.UserPasswordHistories).Returns(value:passwordHistryMock.Object);
        }

        private UserDto GetUserInfo()
        {
            return new UserDto
                   {
                       UserName = "test-user-name",
                       FirstName = "test",
                       LastName = "last",
                       AddressLine1 = "address line1",
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
                var answer = answers.ElementAt(index:i);
                var question = questions.ElementAt(index:i);

                qap.Add(item:new AnswerDto {QuestionId = question.QuestionId.Value, Content = answer.Content});
            }

            return qap;
        }

        private List<AnswerDto> CreateQuestions(QuestionTypeName type, int count)
        {
            var qformat = type + " #{0}";
            var aformat = type + " #{0} answer.";

            var qaParies = new List<QuestionAnswerPairDto>();

            var questions = new List<QuestionDto>();
            var answers = new List<AnswerDto>();

            for (var i = 0; i < count; i++)
            {
                questions.Add(item:new QuestionDto
                                   {
                                       QuestionId = i,
                                       QuestionType = type,
                                       Content = string.Format(format:qformat, arg0:i)
                                   });

                answers.Add(item:new AnswerDto
                                 {
                                     Content = string.Format(format:aformat, arg0:i)
                                 });
            }

            return CreateQuestions(questions:questions, answers:answers);
        }
    }
}

public class HttpContextServiceMock : IHttpContextService
{
    #region fields

    private readonly Dictionary<string, object> _session = new Dictionary<string, object>();

    #endregion

    #region interface implementations

    public HttpContext Current => null;

    public string GetRequestBaseUrl()
    {
        return "";
    }

    public object GetSessionValue(string key)
    {
        if (_session.ContainsKey(key:key))
        {
            return _session[key:key];
        }

        return "";
    }

    public void SetSessionValue(string key, object value)
    {
        _session[key:key] = value;
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

    #endregion
}