using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
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
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NLog;

namespace Linko.LinkoExchange.Services.Authentication
{
    public class AuthenticationService:IAuthenticationService
    {
        #region fields

        private readonly IAuthenticationManager _authenticationManager;
        private readonly ICromerrAuditLogService _crommerAuditLogService;
        private readonly LinkoExchangeContext _dbContext;

        private readonly IDictionary<SystemSettingType, string> _globalSettings;
        private readonly IHttpContextService _httpContext;
        private readonly IInvitationService _invitationService;
        private readonly ILinkoExchangeEmailService _linkoExchangeEmailService;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _organizationService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IPermissionService _permissionService;
        private readonly IProgramService _programService;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IRequestCache _requestCache;
        private readonly ISessionCache _sessionCache;

        private readonly ISettingService _settingService;

        private readonly ApplicationSignInManager _signInManager;
        private readonly ITermConditionService _termConditionService;
        private readonly ApplicationUserManager _userManager;
        private readonly IUserService _userService;

        #endregion

        #region constructors and destructor

        public AuthenticationService(ApplicationUserManager userManager,
                                     ApplicationSignInManager signInManager
                                     , IAuthenticationManager authenticationManager
                                     , ISettingService settingService
                                     , IOrganizationService organizationService
                                     , IProgramService programService
                                     , IInvitationService invitationService
                                     , IPermissionService permissionService
                                     , LinkoExchangeContext linkoExchangeContext
                                     , IUserService userService
                                     , ISessionCache sessionCache
                                     , IRequestCache requestCache
                                     , IPasswordHasher passwordHasher
                                     , IHttpContextService httpContext
                                     , ILogger logger
                                     , IQuestionAnswerService questionAnswerService
                                     , IMapHelper mapHelper
                                     , ICromerrAuditLogService crommerAuditLogService
                                     , ITermConditionService termConditionService
                                     , ILinkoExchangeEmailService linkoExchangeEmailService
        )
        {
            if (linkoExchangeContext == null)
            {
                throw new ArgumentNullException(paramName:nameof(linkoExchangeContext));
            }
            if (userManager == null)
            {
                throw new ArgumentNullException(paramName:nameof(userManager));
            }
            if (signInManager == null)
            {
                throw new ArgumentNullException(paramName:nameof(signInManager));
            }
            if (authenticationManager == null)
            {
                throw new ArgumentNullException(paramName:nameof(authenticationManager));
            }
            if (settingService == null)
            {
                throw new ArgumentNullException(paramName:nameof(settingService));
            }
            if (organizationService == null)
            {
                throw new ArgumentNullException(paramName:nameof(organizationService));
            }
            if (programService == null)
            {
                throw new ArgumentNullException(paramName:nameof(programService));
            }
            if (invitationService == null)
            {
                throw new ArgumentNullException(paramName:nameof(invitationService));
            }

            if (permissionService == null)
            {
                throw new ArgumentNullException(paramName:nameof(permissionService));
            }
            if (userService == null)
            {
                throw new ArgumentNullException(paramName:nameof(userService));
            }
            if (sessionCache == null)
            {
                throw new ArgumentNullException(paramName:nameof(sessionCache));
            }
            if (requestCache == null)
            {
                throw new ArgumentNullException(paramName:nameof(requestCache));
            }
            if (httpContext == null)
            {
                throw new ArgumentNullException(paramName:nameof(httpContext));
            }
            if (logger == null)
            {
                throw new ArgumentNullException(paramName:nameof(logger));
            }
            if (questionAnswerService == null)
            {
                throw new ArgumentNullException(paramName:nameof(questionAnswerService));
            }
            if (mapHelper == null)
            {
                throw new ArgumentNullException(paramName:nameof(mapHelper));
            }
            if (crommerAuditLogService == null)
            {
                throw new ArgumentNullException(paramName:nameof(crommerAuditLogService));
            }
            if (termConditionService == null)
            {
                throw new ArgumentNullException(paramName:nameof(termConditionService));
            }
            if (linkoExchangeEmailService == null)
            {
                throw new ArgumentNullException(paramName:nameof(linkoExchangeEmailService));
            }

            _dbContext = linkoExchangeContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _authenticationManager = authenticationManager;
            _settingService = settingService;
            _organizationService = organizationService;
            _programService = programService;
            _invitationService = invitationService;
            _globalSettings = _settingService.GetGlobalSettings();
            _permissionService = permissionService;
            _userService = userService;
            _sessionCache = sessionCache;
            _requestCache = requestCache;
            _passwordHasher = passwordHasher;
            _httpContext = httpContext;
            _logger = logger;
            _questionAnswerService = questionAnswerService;
            _mapHelper = mapHelper;
            _crommerAuditLogService = crommerAuditLogService;
            _termConditionService = termConditionService;
            _linkoExchangeEmailService = linkoExchangeEmailService;
        }

        #endregion

        #region interface implementations

        public IList<Claim> GetClaims()
        {
            if (_httpContext.Current.User.Identity.IsAuthenticated)
            {
                var identity = _httpContext.Current.User.Identity as ClaimsIdentity;
                if (identity == null)
                {
                    return null;
                }

                var claims = identity.Claims.ToList();

                var uClaims = new Dictionary<string, Claim>();

                foreach (var claim in claims)
                {
                    if (!uClaims.ContainsKey(key:claim.Type))
                    {
                        uClaims.Add(key:claim.Type, value:claim);
                    }
                }

                return uClaims.Values.ToList();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///     Set current user's additional claims, such as current organizationId, current authorityId, current programId
        /// </summary>
        /// <param name="claims"> The claims to set </param>
        public void SetCurrentUserClaims(IDictionary<string, string> claims)
        {
            if (claims != null && claims.Count >= 1)
            {
                var currentClaims = GetClaims();
                if (currentClaims != null)
                {
                    var owinUserId = currentClaims.FirstOrDefault(i => i.Type == CacheKey.OwinUserId)?.Value;
                    using (var itor = claims.GetEnumerator())
                    {
                        while (itor.MoveNext())
                        {
                            currentClaims.Add(item:new Claim(type:itor.Current.Key, value:itor.Current.Value));
                        }
                    }

                    ClearClaims(userId:owinUserId);
                    SaveClaims(userId:owinUserId, claims:currentClaims);
                }
            }
        }

        // Change or reset password
        /// <summary>
        ///     Change password happens after a user login, and change his password.
        ///     New password must meet the following criteria
        ///     1. Meet the strictest password policies when the user have multiple access to organizations and programs
        ///     2. Cannot be the same as the last X number of passwords saved in UserPasswordHistory table.
        /// </summary>
        /// <param name="userId"> User Id </param>
        /// <param name="newPassword"> The new password </param>
        /// <returns> </returns>
        public Task<AuthenticationResultDto> ChangePasswordAsync(string userId, string newPassword)
        {
            var emailEntries = new List<EmailEntry>();
            var authenticationResult = new AuthenticationResultDto();

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var applicationUser = _userManager.FindById(userId:userId);
                    if (applicationUser == null)
                    {
                        authenticationResult.Success = false;
                        authenticationResult.Result = AuthenticationResult.UserNotFound;
                        return Task.FromResult(result:authenticationResult);
                    }

                    var authorityOrganizationIds = GetUserAuthorityOrganizationIds(userid:applicationUser.UserProfileId);
                    var organizationSettings = _settingService.GetOrganizationSettingsByIds(organizationIds:authorityOrganizationIds).SelectMany(i => i.Settings).ToList();

                    SetPasswordPolicy(organizationSettings:organizationSettings);

                    // Use PasswordValidator
                    var validateResult = _userManager.PasswordValidator.ValidateAsync(item:newPassword).Result;
                    if (validateResult.Succeeded == false)
                    {
                        authenticationResult.Success = false;
                        authenticationResult.Errors = validateResult.Errors;
                        return Task.FromResult(result:authenticationResult);
                    }

                    // Check if the new password is one of the password used last # numbers
                    if (!IsValidPasswordCheckInHistory(password:newPassword, userProfileId:applicationUser.UserProfileId, organizationSettings:organizationSettings))
                    {
                        var numberOfPasswordsInHistory = GetStrictestPasswordHistoryCounts(organizationSettings:organizationSettings, orgTypeName:null);
                        authenticationResult.Success = false;
                        authenticationResult.Result = AuthenticationResult.CanNotUseOldPassword;
                        authenticationResult.Errors = new[] {$"You cannot use the last {numberOfPasswordsInHistory} passwords."};
                        return Task.FromResult(result:authenticationResult);
                    }

                    _userManager.RemovePassword(userId:userId);
                    _userManager.AddPassword(userId:userId, password:newPassword);

                    //create history record
                    var history = _dbContext.UserPasswordHistories.Create();
                    history.UserProfileId = applicationUser.UserProfileId;
                    history.PasswordHash = _passwordHasher.HashPassword(password:newPassword);
                    history.LastModificationDateTimeUtc = DateTimeOffset.Now;
                    _dbContext.UserPasswordHistories.Add(entity:history);
                    _dbContext.SaveChanges();

                    //Send Email
                    var contentReplacements = new Dictionary<string, string>();
                    var supportPhoneNumber = _globalSettings[key:SystemSettingType.SupportPhoneNumber];
                    var supportEmail = _globalSettings[key:SystemSettingType.SupportEmailAddress];

                    var authorityList = _organizationService.GetUserAuthorityListForEmailContent(userProfileId:applicationUser.UserProfileId);
                    contentReplacements.Add(key:"firstName", value:applicationUser.FirstName);
                    contentReplacements.Add(key:"lastName", value:applicationUser.LastName);
                    contentReplacements.Add(key:"authorityList", value:authorityList);
                    contentReplacements.Add(key:"supportPhoneNumber", value:supportPhoneNumber);
                    contentReplacements.Add(key:"supportEmail", value:supportEmail);

                    var emailEntry = new EmailEntry
                                     {
                                         EmailType = EmailType.Profile_PasswordChanged,
                                         ContentReplacements = contentReplacements,
                                         RecipientEmailAddress = applicationUser.Email,
                                         RecipientFirstName = applicationUser.FirstName,
                                         RecipientLastName = applicationUser.LastName
                                     };

                    var currentOrganizationRegulatoryProgramId = _httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId);
                    if (currentOrganizationRegulatoryProgramId.Trim().Length > 0)
                    {
                        var currentOrganizationRegulatoryProgram =
                            _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:int.Parse(s:currentOrganizationRegulatoryProgramId));
                        emailEntry.RecipientOrganizationId = currentOrganizationRegulatoryProgram.OrganizationId;
                        emailEntry.RecipientOrgulatoryProgramId = currentOrganizationRegulatoryProgram.RegulatoryProgramId;
                        emailEntry.RecipientRegulatorOrganizationId = currentOrganizationRegulatoryProgram.RegulatorOrganizationId;
                    }

                    emailEntries.Add(item:emailEntry);

                    //Cromerr
                    //Need to log for all associated regulatory program orgs
                    var orgRegProgUsers = _dbContext.OrganizationRegulatoryProgramUsers
                                                    .Include(path:"OrganizationRegulatoryProgram")
                                                    .Where(u => u.UserProfileId == applicationUser.UserProfileId).ToList();
                    foreach (var actorProgramUser in orgRegProgUsers)
                    {
                        _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:actorProgramUser);

                        var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                                      {
                                                          RegulatoryProgramId = actorProgramUser.OrganizationRegulatoryProgram.RegulatoryProgramId,
                                                          OrganizationId = actorProgramUser.OrganizationRegulatoryProgram.OrganizationId
                                                      };
                        cromerrAuditLogEntryDto.RegulatorOrganizationId = actorProgramUser.OrganizationRegulatoryProgram.RegulatorOrganizationId
                                                                          ?? cromerrAuditLogEntryDto.OrganizationId;
                        cromerrAuditLogEntryDto.UserProfileId = actorProgramUser.UserProfileId;
                        cromerrAuditLogEntryDto.UserName = applicationUser.UserName;
                        cromerrAuditLogEntryDto.UserFirstName = applicationUser.FirstName;
                        cromerrAuditLogEntryDto.UserLastName = applicationUser.LastName;
                        cromerrAuditLogEntryDto.UserEmailAddress = applicationUser.Email;
                        cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                        cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
                        contentReplacements = new Dictionary<string, string>
                                              {
                                                  {"firstName", applicationUser.FirstName},
                                                  {"lastName", applicationUser.LastName},
                                                  {"userName", applicationUser.UserName},
                                                  {"emailAddress", applicationUser.Email}
                                              };

                        _crommerAuditLogService.Log(eventType:CromerrEvent.Profile_PasswordChanged, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);
                    }

                    // Do email audit log.
                    _linkoExchangeEmailService.WriteEmailAuditLogs(emailEntries:emailEntries);

                    transaction.Commit();

                    // Send emails.
                    _linkoExchangeEmailService.SendEmails(emailEntries:emailEntries);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return Task.FromResult(result:authenticationResult);
        }

        public ICollection<RegistrationResult> ValidateUserProfileForRegistration(UserDto userInfo, RegistrationType registrationType)
        {
            var inValidUserProfileMessages = new List<RegistrationResult>();

            var result = _userService.ValidateUserProfileData(userProfile:userInfo);
            if (result != RegistrationResult.Success)
            {
                inValidUserProfileMessages.Add(item:RegistrationResult.BadUserProfileData);
            }

            if (userInfo == null)
            {
                inValidUserProfileMessages.Add(item:RegistrationResult.BadUserProfileData);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(value:userInfo.UserName))
                {
                    inValidUserProfileMessages.Add(item:RegistrationResult.MissingUserName);
                }

                if (string.IsNullOrWhiteSpace(value:userInfo.Email))
                {
                    inValidUserProfileMessages.Add(item:RegistrationResult.MissingEmailAddress);
                }
            }

            return inValidUserProfileMessages;
        }

        /// <summary>
        ///     Create a new user for registration
        ///     Confirmed: No possible for one user being invited to a program that he is in already.
        /// </summary>
        /// <param name="userInfo"> The registration user information. </param>
        /// <param name="registrationToken"> Registration token </param>
        /// <param name="securityQuestions"> Security questions </param>
        /// <param name="kbqQuestions"> KBQ questions </param>
        /// <param name="registrationType"> Registration type </param>
        /// <returns> Registration results. </returns>
        public RegistrationResultDto Register(
            UserDto userInfo,
            string registrationToken,
            IEnumerable<AnswerDto> securityQuestions,
            IEnumerable<AnswerDto> kbqQuestions,
            RegistrationType registrationType)
        {
            var registrationResult = new RegistrationResultDto();

            if (registrationType != RegistrationType.ReRegistration)
            {
                if (userInfo == null)
                {
                    registrationResult.Result = RegistrationResult.BadUserProfileData;
                    return registrationResult;
                }

                if (string.IsNullOrWhiteSpace(value:userInfo.UserName))
                {
                    var errText = "User name cannot be null or whitespace.";
                    _logger.Error(message:errText);
                    throw new Exception(message:errText);
                }

                userInfo.UserName = userInfo.UserName.Trim();

                var validatResult = ValidateRegistrationData(userProfile:userInfo, securityQuestions:securityQuestions, kbqQuestions:kbqQuestions);
                if (validatResult != RegistrationResult.Success)
                {
                    registrationResult.Result = validatResult;
                    return registrationResult;
                }
            }

            if (string.IsNullOrWhiteSpace(value:registrationToken))
            {
                registrationResult.Result = RegistrationResult.InvalidRegistrationToken;
                return registrationResult;
            }

            _logger.Info(message:$"Register. userName={userInfo.UserName}, registrationToken={registrationToken}");

            var invitationDto = _invitationService.GetInvitation(invitationId:registrationToken);

            if (invitationDto == null)
            {
                registrationResult.Result = RegistrationResult.InvalidRegistrationToken;
                return registrationResult;
            }

            #region Check invitation expiration 

            // UC-42 1.a 
            // Check token is expired or not? from organization settings
            var invitationRecipientProgram =
                _programService.GetOrganizationRegulatoryProgram(organizationRegulatoryProgramId:invitationDto.RecipientOrganizationRegulatoryProgramId);
            var inivitationRecipintOrganizationSettings =
                _settingService.GetOrganizationSettingsById(organizationId:invitationRecipientProgram.RegulatorOrganizationId ?? invitationRecipientProgram.OrganizationId);

            // always get the authority settings as currently industry don't have settings 

            var invitationExpirationHours = ValueParser.TryParseInt(value:ConfigurationManager.AppSettings[name:"DefaultInviteExpirationHours"], defaultValue:72);
            if (inivitationRecipintOrganizationSettings.Settings.Any())
            {
                invitationExpirationHours = ValueParser.TryParseInt(value:inivitationRecipintOrganizationSettings
                                                                        .Settings.Single(i => i.TemplateName == SettingType.InvitationExpiredHours).Value,
                                                                    defaultValue:invitationExpirationHours);
            }

            if (DateTimeOffset.UtcNow > invitationDto.InvitationDateTimeUtc.AddHours(hours:invitationExpirationHours))
            {
                registrationResult.Result = RegistrationResult.InvitationExpired;
                return registrationResult;
            }

            #endregion  End of Checking invitation expiration 

            // TODO: Need to check invitation email address same as user info email address. Otherwise, in future if user can update email address then it might update wrong user info

            // Email should be unique.
            var applicationUser = _userManager.FindByEmail(email:userInfo.Email);

            // 2.a  Actor is already registration with LinkoExchange  

            if (applicationUser != null)
            {
                if (registrationType == RegistrationType.NewRegistration)
                {
                    registrationResult.Result = RegistrationResult.EmailIsUsed;
                    return registrationResult;
                }
            }
            else
            {
                if (registrationType == RegistrationType.NewRegistration)
                {
                    var testUser = _userManager.FindByName(userName:userInfo.UserName);
                    if (testUser != null)
                    {
                        registrationResult.Result = RegistrationResult.UserNameIsUsed;
                        return registrationResult;
                    }
                }
            }

            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    var termConditionId = _termConditionService.GetLatestTermConditionId();

                    #region Create a new user registration 

                    if (registrationType == RegistrationType.NewRegistration)
                    {
                        applicationUser = AssignUser(userName:userInfo.UserName, email:userInfo.Email);

                        #region Update the new user profile  

                        applicationUser.TitleRole = userInfo.TitleRole;
                        applicationUser.BusinessName = userInfo.BusinessName;
                        applicationUser.FirstName = userInfo.FirstName;
                        applicationUser.LastName = userInfo.LastName;
                        applicationUser.UserName = userInfo.UserName;
                        applicationUser.AddressLine1 = userInfo.AddressLine1;
                        applicationUser.AddressLine2 = userInfo.AddressLine2;
                        applicationUser.CityName = userInfo.CityName;
                        applicationUser.JurisdictionId = userInfo.JurisdictionId;
                        applicationUser.ZipCode = userInfo.ZipCode;
                        applicationUser.PhoneNumber = userInfo.PhoneNumber;
                        applicationUser.PhoneExt = userInfo.PhoneExt;
                        applicationUser.PhoneNumberConfirmed = true;

                        applicationUser.IsAccountLocked = false;
                        applicationUser.IsAccountResetRequired = false;
                        applicationUser.LockoutEnabled = true;
                        applicationUser.EmailConfirmed = true;
                        applicationUser.IsInternalAccount = false;
                        applicationUser.IsIdentityProofed = false;
                        applicationUser.CreationDateTimeUtc = DateTimeOffset.Now;
                        applicationUser.TermConditionAgreedDateTimeUtc = DateTimeOffset.Now;
                        applicationUser.TermConditionId = termConditionId;

                        #endregion

                        var result = _userManager.Create(user:applicationUser, password:userInfo.Password);
                        if (result == IdentityResult.Success)
                        {
                            // Retrieve user again to get userProfile Id. 
                            applicationUser = _userManager.FindById(userId:applicationUser.Id);
                        }
                        else
                        {
                            var errText = $"Creating user failed. Email={userInfo.Email}, userName={userInfo.UserName}";
                            _logger.Error(message:errText);
                            throw new Exception(message:errText);
                        }
                    }

                    #endregion

                    #region User is from re-registration 

                    if (registrationType == RegistrationType.ReRegistration && applicationUser.TermConditionId != termConditionId)
                    {
                        applicationUser.TermConditionAgreedDateTimeUtc = DateTimeOffset.Now;
                        applicationUser.TermConditionId = termConditionId;
                    }

                    // Existing user re-register
                    // Check if the password has been in # password in history
                    if (registrationType == RegistrationType.ResetRegistration)
                    {
                        var passwordHash = _passwordHasher.HashPassword(password:userInfo.Password);

                        var authorityOrganizationIds = GetUserAuthorityOrganizationIds(userid:applicationUser.UserProfileId);
                        var organizationSettings = _settingService.GetOrganizationSettingsByIds(organizationIds:authorityOrganizationIds).SelectMany(i => i.Settings).ToList();

                        if (!IsValidPasswordCheckInHistory(password:userInfo.Password, userProfileId:applicationUser.UserProfileId, organizationSettings:organizationSettings))
                        {
                            var numberOfPasswordsInHistory = GetStrictestPasswordHistoryCounts(organizationSettings:organizationSettings, orgTypeName:null);
                            registrationResult.Result = RegistrationResult.CanNotUseLastNumberOfPasswords;
                            registrationResult.Errors = new[]
                                                        {
                                                            $"You cannot use the last {numberOfPasswordsInHistory} passwords."
                                                        };
                            return registrationResult;
                        }

                        applicationUser.TitleRole = userInfo.TitleRole;
                        applicationUser.BusinessName = userInfo.BusinessName;
                        applicationUser.FirstName = userInfo.FirstName;
                        applicationUser.LastName = userInfo.LastName;
                        applicationUser.AddressLine1 = userInfo.AddressLine1;
                        applicationUser.AddressLine2 = userInfo.AddressLine2;
                        applicationUser.CityName = userInfo.CityName;
                        applicationUser.JurisdictionId = userInfo.JurisdictionId;
                        applicationUser.ZipCode = userInfo.ZipCode;
                        applicationUser.PhoneNumber = userInfo.PhoneNumber;
                        applicationUser.PhoneExt = userInfo.PhoneExt;
                        applicationUser.PasswordHash = passwordHash;
                        applicationUser.EmailConfirmed = true;

                        // Clear KBQ questions and Security Questions for existing user re-registration 
                        _questionAnswerService.DeleteUserQuestionAndAnswers(userProfileId:applicationUser.UserProfileId);

                        // Set IsRestRequired to be false  
                        applicationUser.IsAccountResetRequired = false;
                        applicationUser.TermConditionAgreedDateTimeUtc = DateTimeOffset.Now;
                        applicationUser.TermConditionId = termConditionId;
                    }

                    #endregion

                    #region Save into passwordHistory and KBQ question, and Security Question 

                    if (registrationType == RegistrationType.NewRegistration || registrationType == RegistrationType.ResetRegistration)
                    {
                        // Create a new row in userProfile password history table 
                        _dbContext.UserPasswordHistories.Add(entity:new UserPasswordHistory
                                                                    {
                                                                        LastModificationDateTimeUtc = DateTimeOffset.UtcNow,
                                                                        PasswordHash = applicationUser.PasswordHash,
                                                                        UserProfileId = applicationUser.UserProfileId
                                                                    });

                        // Save Security questions and kbq questions
                        var combined = securityQuestions.Concat(second:kbqQuestions);
                        _questionAnswerService.CreateUserQuestionAnswers(userProfileId:applicationUser.UserProfileId, questionAnswers:combined);
                    }

                    #endregion

                    var inviterOrganizationRegulatoryProgram =
                        _programService.GetOrganizationRegulatoryProgram(organizationRegulatoryProgramId:invitationDto.SenderOrganizationRegulatoryProgramId);

                    var recipientOrganizationRegulatoryProgram =
                        _programService.GetOrganizationRegulatoryProgram(organizationRegulatoryProgramId:invitationDto.RecipientOrganizationRegulatoryProgramId);

                    //  Program is disabled or not found
                    //  UC-42 2.c, 2.d, 2.e
                    if (recipientOrganizationRegulatoryProgram == null
                        || inviterOrganizationRegulatoryProgram == null
                        || !recipientOrganizationRegulatoryProgram.IsEnabled
                        || !inviterOrganizationRegulatoryProgram.IsEnabled
                        || recipientOrganizationRegulatoryProgram.OrganizationDto == null
                    )
                    {
                        registrationResult.Result = RegistrationResult.Failed;
                        return registrationResult;
                    }

                    if (!recipientOrganizationRegulatoryProgram.RegulatorOrganizationId.HasValue && inviterOrganizationRegulatoryProgram.RegulatorOrganizationId.HasValue)
                    {
                        // IU invited the authority user; AU can only invite authority user
                        registrationResult.Result = RegistrationResult.Failed;
                        return registrationResult;
                    }

                    // UC-42 6
                    // 2 Create organization regulatory program user, and set the approved statue to false  
                    var emailEntries =
                        CreateOrUpdateOrganizationRegulatoryProgramUserDuringRegistration(registeredUser:applicationUser,
                                                                                          registeredOrganizationRegulatoryProgram:recipientOrganizationRegulatoryProgram,
                                                                                          inviterOrganizationRegulatoryProgram:inviterOrganizationRegulatoryProgram,
                                                                                          registrationType:registrationType);

                    // All succeed
                    // 4 Remove the invitation from table 
                    _invitationService.DeleteInvitation(invitationId:invitationDto.InvitationId, isSystemAction:true);

                    _dbContext.SaveChanges();

                    // Do email audit log.
                    _linkoExchangeEmailService.WriteEmailAuditLogs(emailEntries:emailEntries);

                    transaction.Commit();

                    //Send pending registration emails  
                    _linkoExchangeEmailService.SendEmails(emailEntries:emailEntries);

                    registrationResult.Result = RegistrationResult.Success;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }

            _logger.Info(message:$"Register. userName={userInfo.UserName}, email={registrationToken}, registrationResult{registrationResult}");
            return registrationResult;
        }

        public bool CheckPasswordResetUrlNotExpired(string token)
        {
            bool isNotExpiredYet;
            var resetPasswordTokenValidateInterval = Convert.ToInt32(value:ConfigurationManager.AppSettings[name:"ResetPasswordTokenValidateInterval"]);
            var emailAuditLog = _dbContext.EmailAuditLogs.FirstOrDefault(e => e.Token == token);
            if (emailAuditLog == null)
            {
                return false;

                //throw new Exception($"ERROR: Cannot find email audit log associated with token={token}");
            }

            var tokenCreated = emailAuditLog.SentDateTimeUtc;
            if (DateTimeOffset.Now.AddHours(hours:-resetPasswordTokenValidateInterval) > tokenCreated)
            {
                //Check token expiry (5.1.a)
                isNotExpiredYet = false;
                if (emailAuditLog.RecipientUserProfileId.HasValue)
                {
                    var userProfileId = emailAuditLog.RecipientUserProfileId.Value;
                    foreach (var orgRegProgDto in _organizationService.GetUserOrganizations(userId:userProfileId))
                    {
                        var userDto = _userService.GetUserProfileById(userProfileId:userProfileId);
                        _crommerAuditLogService.SimpleLog(eventType:CromerrEvent.ForgotPassword_PasswordResetExpired, orgRegProgram:orgRegProgDto, user:userDto);
                    }
                }
            }
            else
            {
                isNotExpiredYet = true;
            }

            return isNotExpiredYet;
        }

        public async Task<AuthenticationResultDto> ResetPasswordAsync(string resetPasswordToken, int userQuestionAnswerId, string answer, int failedCount, string newPassword)
        {
            var authenticationResult = new AuthenticationResultDto();

            // Use PasswordValidator
            var validateResult = _userManager.PasswordValidator.ValidateAsync(item:newPassword).Result;
            if (validateResult.Succeeded == false)
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.PasswordRequirementsNotMet;
                authenticationResult.Errors = validateResult.Errors;
                return authenticationResult;
            }

            var resetPasswordTokenValidateInterval = Convert.ToInt32(value:ConfigurationManager.AppSettings[name:"ResetPasswordTokenValidateInterval"]);

            var emailAuditLog = _dbContext.EmailAuditLogs.FirstOrDefault(e => e.Token == resetPasswordToken);

            if (emailAuditLog == null)
            {
                throw new Exception(message:$"ERROR: Cannot find email audit log associated with token={resetPasswordToken}");
            }

            var tokenCreated = emailAuditLog.SentDateTimeUtc;

            if (DateTimeOffset.Now.AddHours(hours:-resetPasswordTokenValidateInterval) > tokenCreated)
            {
                //Check token expiry (5.1.a)

                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.ExpiredRegistrationToken;
                authenticationResult.Errors = new[] {"The password reset link has expired. Please use Forgot Password."};

                var userProfileId = _dbContext.UserQuestionAnswers.Single(u => u.UserQuestionAnswerId == userQuestionAnswerId).UserProfileId;
                foreach (var orgRegProgDto in _organizationService.GetUserOrganizations(userId:userProfileId))
                {
                    var userDto = _userService.GetUserProfileById(userProfileId:userProfileId);
                    await _crommerAuditLogService.SimpleLog(eventType:CromerrEvent.ForgotPassword_PasswordResetExpired, orgRegProgram:orgRegProgDto, user:userDto);
                }

                return authenticationResult;
            }

            var resetPasswordResult = await ResetPasswordAsync(userQuestionAnswerId:userQuestionAnswerId, answer:answer, failedCount:failedCount, newPassword:newPassword);
            if (resetPasswordResult.Result == AuthenticationResult.Success)
            {
                emailAuditLog.Token = string.Empty;
            }
            _dbContext.SaveChanges();
            return resetPasswordResult;
        }

        public async Task<AuthenticationResultDto> ResetPasswordAsync(int userQuestionAnswerId,
                                                                      string answer, int failedCount, string newPassword)
        {
            var userProfileId = _dbContext.UserQuestionAnswers.Single(u => u.UserQuestionAnswerId == userQuestionAnswerId).UserProfileId;
            var passwordHash = _passwordHasher.HashPassword(password:newPassword);
            var correctSavedHashedAnswer = _dbContext.UserQuestionAnswers.Single(a => a.UserQuestionAnswerId == userQuestionAnswerId).Content;
            var authorityOrganizationIds = GetUserAuthorityOrganizationIds(userid:userProfileId);
            var organizationSettings = _settingService.GetOrganizationSettingsByIds(organizationIds:authorityOrganizationIds).SelectMany(i => i.Settings).ToList();

            var authenticationResult = new AuthenticationResultDto();

            //KBQ ANSWERS ARE CASE-INSENSITIVE; PERSISTED AS ALL LOWER CASE
            if (_userManager.PasswordHasher.VerifyHashedPassword(hashedPassword:correctSavedHashedAnswer, providedPassword:answer.Trim().ToLower())
                != PasswordVerificationResult.Success)
            {
                //Check hashed answer (5.3.a)

                authenticationResult.Success = false;

                //3rd incorrect attempt (5.3.b) => lockout
                var maxAnswerAttempts =
                    Convert.ToInt32(value:_settingService.GetOrganizationSettingValueByUserId(userProfileId:userProfileId, settingType:SettingType.FailedKBQAttemptMaxCount,
                                                                                              isChooseMin:true, isChooseMax:null));
                if (failedCount + 1 >= maxAnswerAttempts) // from web.config
                {
                    _userService.LockUnlockUserAccount(userProfileId:userProfileId, isAttemptingLock:true, reason:AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringPasswordReset);

                    //Get all associated authorities
                    var userOrgs = _organizationService.GetUserRegulators(userId:userProfileId).ToList();
                    authenticationResult.RegulatoryList = userOrgs;

                    var errorString = "<div class=\"table - responsive\">";
                    errorString += "<table class=\"table no-margin\">";
                    errorString += "<tbody>";

                    foreach (var org in userOrgs)
                    {
                        errorString += "<tr><td>"
                                       + org.EmailContactInfoName
                                       + "</td><td>"
                                       + org.EmailContactInfoEmailAddress
                                       + "</td><td>"
                                       + org.EmailContactInfoPhone
                                       + " </td></tr>";
                    }

                    errorString += "</tbody>";
                    errorString += "</table>";
                    errorString += "</div>";
                    errorString += "</table>";

                    authenticationResult.Result = AuthenticationResult.UserIsLocked;
                    authenticationResult.Errors = new[] {errorString};
                }
                else
                {
                    authenticationResult.Result = AuthenticationResult.IncorrectAnswerToQuestion;
                    authenticationResult.Errors = new[] {"The answer is incorrect.  Please try again."};
                }
            }
            else if (!IsValidPasswordCheckInHistory(password:newPassword, userProfileId:userProfileId, organizationSettings:organizationSettings))
            {
                //Password used before (6.a)
                var numberOfPasswordsInHistory = GetStrictestPasswordHistoryCounts(organizationSettings:organizationSettings, orgTypeName:null);

                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.CanNotUseOldPassword;
                authenticationResult.Errors = new[] {$"You cannot use the last {numberOfPasswordsInHistory} passwords."};
            }
            else
            {
                using (var transaction = _dbContext.BeginTransaction())
                {
                    try
                    {
                        //create history record
                        var history = _dbContext.UserPasswordHistories.Create();
                        history.UserProfileId = userProfileId;
                        history.PasswordHash = passwordHash;
                        history.LastModificationDateTimeUtc = DateTimeOffset.Now;
                        _dbContext.UserPasswordHistories.Add(entity:history);
                        _dbContext.SaveChanges();

                        //Set new password
                        _userService.SetHashedPassword(userProfileId:userProfileId, passwordHash:passwordHash);

                        //Unlock user
                        var userOwinId = _dbContext.Users.Single(u => u.UserProfileId == userProfileId).Id;
                        await _userManager.UnlockUserAccount(userId:userOwinId);

                        authenticationResult.Success = true;
                        authenticationResult.Result = AuthenticationResult.Success;

                        foreach (var orgRegProgDto in _organizationService.GetUserOrganizations(userId:userProfileId))
                        {
                            var userDto = _userService.GetUserProfileById(userProfileId:userProfileId);
                            _crommerAuditLogService.SimpleLog(eventType:CromerrEvent.ForgotPassword_Success, orgRegProgram:orgRegProgDto, user:userDto).Wait();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return authenticationResult;
        }

        /// <summary>
        ///     To request a password reset. This will do follow:
        ///     1. generate a reset password token
        ///     2. send a reset password email
        ///     3. log to system
        /// </summary>
        /// <param name="username"> The user name </param>
        /// <returns> </returns>
        public async Task<AuthenticationResultDto> RequestResetPassword(string username)
        {
            var authenticationResult = new AuthenticationResultDto();

            var user = _dbContext.Users.SingleOrDefault(u => u.UserName == username);
            if (user == null)
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.UserNotFound;
                authenticationResult.Errors = new[] {"UserNotFound"};
            }
            else if (!await _userManager.IsEmailConfirmedAsync(userId:user.Id))
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.EmailIsNotConfirmed;
                authenticationResult.Errors = new[] {"EmailIsNotConfirmed"};
            }
            else
            {
                authenticationResult.Success = true;
                authenticationResult.Result = AuthenticationResult.Success;
                SendResetPasswordConfirmationEmail(userProfile:user);
            }

            return authenticationResult;
        }

        public async Task<AuthenticationResultDto> RequestUsernameEmail(string email)
        {
            var authenticationResult = new AuthenticationResultDto();

            var user = _userManager.FindByEmail(email:email);
            if (user == null)
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.UserNotFound;
                authenticationResult.Errors = new[] {"UserNotFound"};
            }
            else if (!await _userManager.IsEmailConfirmedAsync(userId:user.Id))
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.EmailIsNotConfirmed;
                authenticationResult.Errors = new[] {"EmailIsNotConfirmed"};
            }
            else
            {
                authenticationResult.Success = true;
                authenticationResult.Result = AuthenticationResult.Success;
                SendRequestUsernameEmail(userProfile:user);
            }

            return authenticationResult;
        }

        /// <summary>
        ///     Sign in by user name and password.  "isPersistent" indicates to keep the cookie or now.
        /// </summary>
        /// <param name="userName"> The user name used when sign in </param>
        /// <param name="password"> The password used when sign in </param>
        /// <param name="isPersistent"> The flag indicates persistent the sign or not </param>
        /// <returns> </returns>
        public Task<SignInResultDto> SignInByUserName(string userName, string password, bool isPersistent)
        {
            _logger.Info(message:"SignInByUserName. userName={0}", argument:userName);

            var signInResultDto = new SignInResultDto();

            var applicationUser = _userManager.FindByName(userName:userName);
            if (applicationUser == null)
            {
                signInResultDto.AutehticationResult = AuthenticationResult.UserNotFound;
                return Task.FromResult(result:signInResultDto);
            }

            signInResultDto.RegulatoryList = _organizationService.GetUserRegulators(userId:applicationUser.UserProfileId, isIncludeRemoved:true).ToList();

            // clear claims from db if there are   
            ClearClaims(userId:applicationUser.Id);
            applicationUser.Claims.Clear();

            var userId = applicationUser.UserProfileId;
            var authorityOrganizationIds = GetUserAuthorityOrganizationIds(userid:userId);

            var organizationSettings = _settingService.GetOrganizationSettingsByIds(organizationIds:authorityOrganizationIds).SelectMany(i => i.Settings).ToList();

            SetPasswordPolicy(organizationSettings:organizationSettings);

            _signInManager.UserManager = _userManager;

            // UC-29, 2.c
            // Check if the user is in 'password lock' status
            if (_userManager.IsLockedOut(userId:applicationUser.Id))
            {
                LogProhibitedSignInActivityToCromerr(user:applicationUser, cromerrEvent:CromerrEvent.Login_AccountLocked);
                signInResultDto.AutehticationResult = AuthenticationResult.PasswordLockedOut;
                return Task.FromResult(result:signInResultDto);
            }

            var signInStatus = _signInManager.PasswordSignIn(userName:userName, password:password, isPersistent:isPersistent, shouldLockout:true);

            if (signInStatus == SignInStatus.Success)
            {
                var claims = GetUserIdentity(userProfile:applicationUser);

                // Save claim
                SaveClaims(userId:applicationUser.Id, claims:claims);

                var identity = new ClaimsIdentity(identity:_httpContext.Current.User.Identity);
                identity.AddClaims(claims:claims);
                _authenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant
                    (identity:identity, properties:new AuthenticationProperties {IsPersistent = isPersistent});

                _authenticationManager.SignOut();

                if (applicationUser.IsAccountResetRequired)
                {
                    LogProhibitedSignInActivityToCromerr(user:applicationUser, cromerrEvent:CromerrEvent.Login_AccountResetRequired);

                    signInResultDto.AutehticationResult = AuthenticationResult.AccountResetRequired;
                    return Task.FromResult(result:signInResultDto);
                }

                // UC-29, 3.a
                // Check if the user has been locked "Account Lockout"  by an authority
                if (applicationUser.IsAccountLocked)
                {
                    LogProhibitedSignInActivityToCromerr(user:applicationUser, cromerrEvent:CromerrEvent.Login_AccountLocked);

                    signInResultDto.AutehticationResult = AuthenticationResult.UserIsLocked;
                    return Task.FromResult(result:signInResultDto);
                }

                // UC-29, 4.a, 5.a, 6.a
                if (!ValidateUserAccess(userProfile:applicationUser, signInResultDto:signInResultDto))
                {
                    return Task.FromResult(result:signInResultDto);
                }

                // UC-29 7.a
                // Check if user's password is expired or not   
                if (IsUserPasswordExpired(userProfileId:userId, organizationSettings:organizationSettings))
                {
                    // Put user profile Id into session, to request user change their password. 
                    //_sessionCache.SetValue(CacheKey.UserProfileId, applicationUser.UserProfileId);
                    signInResultDto.OwinUserId = applicationUser.Id;
                    signInResultDto.UserProfileId = applicationUser.UserProfileId;
                    signInResultDto.AutehticationResult = AuthenticationResult.PasswordExpired;
                    return Task.FromResult(result:signInResultDto);
                }

                signInResultDto.AutehticationResult = AuthenticationResult.Success;

                _sessionCache.SetValue(key:CacheKey.UserProfileId, value:applicationUser.UserProfileId);
                _sessionCache.SetValue(key:CacheKey.OwinUserId, value:applicationUser.Id);

                _authenticationManager.SignIn(new AuthenticationProperties {IsPersistent = isPersistent}, identity);
                _signInManager.PasswordSignIn(userName:userName, password:password, isPersistent:isPersistent, shouldLockout:true);
            }
            else if (signInStatus == SignInStatus.Failure)
            {
                signInResultDto.AutehticationResult = AuthenticationResult.InvalidUserNameOrPassword;
            }
            else if (signInStatus == SignInStatus.LockedOut)
            {
                signInResultDto.AutehticationResult = AuthenticationResult.PasswordLockedOut;

                //Log to Cromerr
                LogProhibitedSignInActivityToCromerr(user:applicationUser, cromerrEvent:CromerrEvent.Login_PasswordLockout);
            }

            _logger.Info(message:"SignInByUserName. signInStatus={0}", argument:signInStatus);
            return Task.FromResult(result:signInResultDto);
        }

        public PasswordAndKbqValidationResult ValidatePasswordAndKbq(string password, int userQuestionAnswerId, string kbqAnswer, int failedPasswordCount, int failedKbqCount,
                                                                     ReportOperation reportOperation, int? reportPackageId = null)
        {
            _logger.Info(message:"Enter AuthenticationService.PasswordAndKbqValidationResult");

            var userProfileId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.UserProfileId));
            var orgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var authority = _settingService.GetAuthority(orgRegProgramId:orgRegProgramId);
            var authoritySettings = _settingService.GetOrganizationSettingsById(organizationId:authority.OrganizationId).Settings;
            var failedPasswordAttemptMaxCount =
                ValueParser.TryParseInt(value:authoritySettings.Where(s => s.TemplateName.Equals(obj:SettingType.FailedPasswordAttemptMaxCount)).Select(s => s.Value).First(),
                                        defaultValue:3);
            var failedKbqAttemptMaxCount =
                ValueParser.TryParseInt(value:authoritySettings.Where(s => s.TemplateName.Equals(obj:SettingType.FailedKBQAttemptMaxCount)).Select(s => s.Value).First(),
                                        defaultValue:3);

            if (failedPasswordAttemptMaxCount <= failedPasswordCount)
            {
                SignOff();
                return PasswordAndKbqValidationResult.InvalidPassword;
            }

            if (failedKbqAttemptMaxCount <= failedKbqCount)
            {
                SignOff();
                return PasswordAndKbqValidationResult.IncorrectKbqAnswer;
            }

            var userProfile = _dbContext.Users.Single(i => i.UserProfileId == userProfileId);

            // check if user is a valid user
            // 1: IsAccountLocked = false, IsAccountResetRequired = false 
            if (userProfile.IsAccountLocked)
            {
                ThrowUserStatusRuleValiation(message:"User is locked");
            }

            if (userProfile.IsAccountResetRequired)
            {
                ThrowUserStatusRuleValiation(message:"User is required to reset account");
            }

            // Check to see if password matches. 
            if (!IsValidPassword(passwordHash:userProfile.PasswordHash, password:password))
            {
                if (failedPasswordAttemptMaxCount <= failedPasswordCount + 1)
                {
                    SignOff();

                    if (reportOperation == ReportOperation.SignAndSubmit)
                    {
                        _userService.LockUnlockUserAccount(userProfileId:userProfileId, isAttemptingLock:true,
                                                           reason:AccountLockEvent.ExceededPasswordMaxAttemptsDuringSignatureCeremony, reportPackageId:reportPackageId);
                    }
                    else if (reportOperation == ReportOperation.Repudiation)
                    {
                        _userService.LockUnlockUserAccount(userProfileId:userProfileId, isAttemptingLock:true,
                                                           reason:AccountLockEvent.ExceededPasswordMaxAttemptsDuringRepudiationCeremony, reportPackageId:reportPackageId);
                    }

                    return PasswordAndKbqValidationResult.UserLocked_Password;
                }

                return PasswordAndKbqValidationResult.InvalidPassword;
            }

            // Check to see if KBQ answer matches
            if (!_questionAnswerService.ConfirmCorrectAnswer(userQuestionAnswerId:userQuestionAnswerId, answer:kbqAnswer.ToLower()))
            {
                if (failedKbqAttemptMaxCount <= failedKbqCount + 1)
                {
                    SignOff();
                    if (reportOperation == ReportOperation.SignAndSubmit)
                    {
                        _userService.LockUnlockUserAccount(userProfileId:userProfileId, isAttemptingLock:true,
                                                           reason:AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringSignatureCeremony, reportPackageId:reportPackageId);
                    }
                    else if (reportOperation == ReportOperation.Repudiation)
                    {
                        _userService.LockUnlockUserAccount(userProfileId:userProfileId, isAttemptingLock:true,
                                                           reason:AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringRepudiationCeremony, reportPackageId:reportPackageId);
                    }

                    return PasswordAndKbqValidationResult.UserLocked_KBQ;
                }

                return PasswordAndKbqValidationResult.IncorrectKbqAnswer;
            }

            // Check user is validate user or not
            // 2: is not disabled, 
            // 3: have access to the regulatory program
            var programUser = _dbContext.OrganizationRegulatoryProgramUsers
                                        .FirstOrDefault(i => i.UserProfileId == userProfileId && i.OrganizationRegulatoryProgramId == orgRegProgramId);

            if (programUser == null || programUser.IsRegistrationApproved == false || programUser.IsRegistrationDenied || programUser.IsRemoved)
            {
                ThrowUserStatusRuleValiation(message:"User does not have access to current program");
            }
            else if (!programUser.IsEnabled)
            {
                ThrowUserStatusRuleValiation(message:"User is disabled");
            }

            return PasswordAndKbqValidationResult.Success;
        }

        public string GetClaimsValue(string claimType)
        {
            string claimsValue = null;

            var claims = GetClaims();

            //return claims?.ToList();
            var claim = claims?.FirstOrDefault(c => c.Type == claimType);
            if (claim != null)
            {
                claimsValue = claim.Value;
            }

            return claimsValue;
        }

        public void SetClaimsForOrgRegProgramSelection(int orgRegProgId)
        {
            //We already have: UserId, UserProfileId, UserFullName,
            var userProfileId = Convert.ToInt32(value:GetClaimsValue(claimType:CacheKey.UserProfileId));

            //Now we set UserRole, OrganizationRegulatoryProgramId, OrganizationName, OrgRegProgUserId, PortalName
            //UserRole <=> PermissionGroup.Name
            //PortalName <=> OrganizationType.Name

            var orgRegProgUser = _dbContext.OrganizationRegulatoryProgramUsers
                                           .Include(path:"OrganizationRegulatoryProgram.RegulatoryProgram")
                                           .SingleOrDefault(o => o.UserProfileId == userProfileId && o.OrganizationRegulatoryProgramId == orgRegProgId);
            if (orgRegProgUser == null)
            {
                throw new Exception(message:string.Format(format:"ERROR: UserProfileId={0} does not have access to Organization Regulatory Program={1}.", arg0:userProfileId,
                                                          arg1:orgRegProgId));
            }

            var permissionGroupId = orgRegProgUser.PermissionGroupId;
            var orgRegProgUserId = orgRegProgUser.OrganizationRegulatoryProgramUserId;
            var regProgramName = orgRegProgUser.OrganizationRegulatoryProgram.RegulatoryProgram.Name;
            var userRole = _dbContext.PermissionGroups.Single(p => p.PermissionGroupId == permissionGroupId).Name;
            var organization = _dbContext.OrganizationRegulatoryPrograms.Include(path:"Organization.OrganizationType")
                                         .Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId).Organization;
            var organizationName = organization.Name;
            var organizationId = organization.OrganizationId;
            var portalName = organization.OrganizationType.Name;

            var claims = new Dictionary<string, string>
                         {
                             {CacheKey.UserRole, userRole},
                             {CacheKey.RegulatoryProgramName, regProgramName},
                             {CacheKey.OrganizationRegulatoryProgramUserId, orgRegProgUserId.ToString()},
                             {CacheKey.OrganizationRegulatoryProgramId, orgRegProgId.ToString()},
                             {CacheKey.OrganizationName, organizationName},
                             {CacheKey.OrganizationId, organizationId.ToString()},
                             {CacheKey.PortalName, portalName}
                         };

            SetCurrentUserClaims(claims:claims);

            //Log to Cromerr

            var programUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:orgRegProgUser);
            var user = _userService.GetUserProfileById(userProfileId:programUserDto.UserProfileId);

            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                          {
                                              RegulatoryProgramId = orgRegProgUser.OrganizationRegulatoryProgram.RegulatoryProgramId,
                                              OrganizationId = orgRegProgUser.OrganizationRegulatoryProgram.OrganizationId
                                          };
            cromerrAuditLogEntryDto.RegulatorOrganizationId = orgRegProgUser.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = orgRegProgUser.UserProfileId;
            cromerrAuditLogEntryDto.UserName = user.UserName;
            cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
            cromerrAuditLogEntryDto.UserLastName = user.LastName;
            cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
            cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
            var contentReplacements = new Dictionary<string, string>
                                      {
                                          {"organizationName", programUserDto.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName},
                                          {"firstName", user.FirstName},
                                          {"lastName", user.LastName},
                                          {"userName", user.UserName},
                                          {"emailAddress", user.Email}
                                      };

            _crommerAuditLogService.Log(eventType:CromerrEvent.Login_Success, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);
        }

        public void SignOff()
        {
            var owinUserId = _sessionCache.GetValue(key:CacheKey.OwinUserId) != null ? _sessionCache.GetValue(key:CacheKey.OwinUserId).ToString() : "";
            ClearClaims(userId:owinUserId);
            _sessionCache.Clear();
            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }

        #endregion

        private List<EmailEntry> CreateOrUpdateOrganizationRegulatoryProgramUserDuringRegistration(UserProfile registeredUser,
                                                                                                   OrganizationRegulatoryProgramDto registeredOrganizationRegulatoryProgram,
                                                                                                   OrganizationRegulatoryProgramDto inviterOrganizationRegulatoryProgram,
                                                                                                   RegistrationType registrationType)
        {
            var emailEntries = new List<EmailEntry>();

            if (registrationType == RegistrationType.ResetRegistration)
            {
                // Set IsRegistrationApproved value as false to enforce all the users need to be approved again for the all programs where they were approved before.
                // Only Authority can approve again 
                var orpus = _dbContext.OrganizationRegulatoryProgramUsers.Include(o => o.OrganizationRegulatoryProgram)
                                      .Where(i => i.UserProfileId == registeredUser.UserProfileId
                                                  && i.IsRemoved == false
                                                  && i.IsRegistrationApproved
                                                  && i.InviterOrganizationRegulatoryProgram.IsRemoved == false
                                                  && i.OrganizationRegulatoryProgram.IsEnabled)
                                      .ToList();

                foreach (var orpu in orpus)
                {
                    orpu.IsRegistrationApproved = false;

                    // Update to new re-registration time-stamp
                    orpu.RegistrationDateTimeUtc = DateTimeOffset.Now;

                    // Update old InviterOrganizationRegulatoryProgramId with authority OrganizationRegulatoryProgramId to show registration approval pending in 
                    // the authority portal as only Authority User can approve

                    var prevRegisteredOrgRegProg = _programService.GetOrganizationRegulatoryProgram(organizationRegulatoryProgramId:orpu.OrganizationRegulatoryProgramId);
                    var authorityOrgRegProg = _organizationService.GetAuthority(orgRegProgramId:prevRegisteredOrgRegProg.OrganizationRegulatoryProgramId);
                    orpu.InviterOrganizationRegulatoryProgramId = authorityOrgRegProg.OrganizationRegulatoryProgramId;
                    orpu.LastModificationDateTimeUtc = DateTimeOffset.Now;

                    _dbContext.SaveChanges();

                    if (!orpu.OrganizationRegulatoryProgram.IsEnabled || orpu.OrganizationRegulatoryProgram.IsRemoved)
                    {
                        continue;
                    }

                    // Prepare Registration Approval Emails
                    emailEntries.AddRange(collection:PrepareApprovalEmailForRegistration(registeredUser:registeredUser,
                                                                                         registeredOrganizationRegulatoryProgram:prevRegisteredOrgRegProg,
                                                                                         inviterOrganizationRegulatoryProgram:authorityOrgRegProg,
                                                                                         authorityOrg:authorityOrgRegProg.OrganizationDto));

                    // Do COMERR Log
                    DoComerrLogForRegistration(registrationType:registrationType, registeredUser:registeredUser, registeredOrganizationRegulatoryProgram:prevRegisteredOrgRegProg,
                                               authorityOrg:authorityOrgRegProg.OrganizationDto).Wait();
                }
            }
            else
            {
                var orpu = _dbContext.OrganizationRegulatoryProgramUsers
                                     .SingleOrDefault(i => i.UserProfileId == registeredUser.UserProfileId
                                                           && i.OrganizationRegulatoryProgramId == registeredOrganizationRegulatoryProgram.OrganizationRegulatoryProgramId);

                if (orpu == null) // totally new user or invited to new organization
                {
                    // To create a new OrgRegProgramUser
                    orpu = new OrganizationRegulatoryProgramUser
                           {
                               IsEnabled = true,
                               IsRegistrationApproved = false,
                               IsRegistrationDenied = false,
                               IsSignatory = false,
                               UserProfileId = registeredUser.UserProfileId,
                               IsRemoved = false,
                               CreationDateTimeUtc = DateTimeOffset.Now,
                               RegistrationDateTimeUtc = DateTimeOffset.Now,
                               OrganizationRegulatoryProgramId = registeredOrganizationRegulatoryProgram.OrganizationRegulatoryProgramId,
                               InviterOrganizationRegulatoryProgramId = inviterOrganizationRegulatoryProgram.OrganizationRegulatoryProgramId
                           };

                    _dbContext.OrganizationRegulatoryProgramUsers.Add(entity:orpu);
                }
                else // re-register after removed
                {
                    // To update the existing one.  
                    orpu.IsRegistrationApproved = false;
                    orpu.IsRegistrationDenied = false;
                    orpu.IsEnabled = true;

                    //Update to new re-reg time-stamp
                    orpu.RegistrationDateTimeUtc = DateTimeOffset.Now;

                    //Update because the new "Inviter" is now the Authority
                    //(need to do this so that this pending registration show up under the Authority)
                    orpu.InviterOrganizationRegulatoryProgramId = inviterOrganizationRegulatoryProgram.OrganizationRegulatoryProgramId;
                    orpu.LastModificationDateTimeUtc = DateTimeOffset.Now;
                }

                if (orpu.OrganizationRegulatoryProgram.IsEnabled && !orpu.OrganizationRegulatoryProgram.IsRemoved)
                {
                    var authorityOrg = registeredOrganizationRegulatoryProgram.RegulatorOrganization ?? registeredOrganizationRegulatoryProgram.OrganizationDto;

                    // Prepare Registration Approval Emails
                    emailEntries.AddRange(collection:PrepareApprovalEmailForRegistration(registeredUser:registeredUser,
                                                                                         registeredOrganizationRegulatoryProgram:registeredOrganizationRegulatoryProgram,
                                                                                         inviterOrganizationRegulatoryProgram:inviterOrganizationRegulatoryProgram,
                                                                                         authorityOrg:authorityOrg));

                    // Do COMERR Log
                    DoComerrLogForRegistration(registrationType:registrationType, registeredUser:registeredUser,
                                               registeredOrganizationRegulatoryProgram:registeredOrganizationRegulatoryProgram,
                                               authorityOrg:authorityOrg).Wait();
                }
            }

            _dbContext.SaveChanges();
            return emailEntries;
        }

        private List<EmailEntry> PrepareApprovalEmailForRegistration(UserProfile registeredUser, OrganizationRegulatoryProgramDto registeredOrganizationRegulatoryProgram,
                                                                     OrganizationRegulatoryProgramDto inviterOrganizationRegulatoryProgram, OrganizationDto authorityOrg)
        {
            //  Determine if user is authority user or is industry user;
            //  To change here when we have multiple level authorities
            var isInvitedToIndustry = registeredOrganizationRegulatoryProgram.RegulatorOrganizationId.HasValue;

            // UC-42 7, 8
            // Find out who have approval permission    
            var approvalPeople = _permissionService.GetApprovalPeople(organizationRegulatoryProgram:inviterOrganizationRegulatoryProgram, isInvitedToIndustry:isInvitedToIndustry)
                                                   .ToList();

            var emailAddressOnEmail = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:inviterOrganizationRegulatoryProgram.RegulatoryProgramId,
                                                                                   settingType:SettingType.EmailContactInfoEmailAddress);
            var phoneNumberOnEmail = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:inviterOrganizationRegulatoryProgram.RegulatoryProgramId,
                                                                                  settingType:SettingType.EmailContactInfoPhone);
            var authorityName = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:inviterOrganizationRegulatoryProgram.RegulatoryProgramId,
                                                                             settingType:SettingType.EmailContactInfoName);

            var emailContentReplacements = new Dictionary<string, string>
                                           {
                                               {"firstName", registeredUser.FirstName},
                                               {"lastName", registeredUser.LastName},
                                               {"supportEmail", emailAddressOnEmail},
                                               {"supportPhoneNumber", phoneNumberOnEmail},
                                               {"authorityName", authorityName},
                                               {"authorityOrganizationName", authorityOrg.OrganizationName},
                                               {"organizationName", registeredOrganizationRegulatoryProgram.OrganizationDto.OrganizationName}
                                           };

            if (!approvalPeople.Any())
            {
                var support = _dbContext.Users.SingleOrDefault(i => i.Email == emailAddressOnEmail && i.IsAccountResetRequired == false);
                if (support == null)
                {
                    support = new UserProfile
                              {
                                  Email = emailAddressOnEmail
                              };
                }

                approvalPeople.Add(item:_mapHelper.GetUserDtoFromUserProfile(userProfile:support)); // send email to authority support email when no approval email found
            }

            if (isInvitedToIndustry)
            {
                var receipientOrg = _organizationService.GetOrganization(organizationId:registeredOrganizationRegulatoryProgram.OrganizationId);

                emailContentReplacements.Add(key:"addressLine1", value:receipientOrg.AddressLine1);
                emailContentReplacements.Add(key:"cityName", value:receipientOrg.CityName);
                emailContentReplacements.Add(key:"stateName", value:receipientOrg.State);
            }

            return approvalPeople.Select(i =>
                                         {
                                             var emailEntry = new EmailEntry
                                                              {
                                                                  RecipientEmailAddress = i.Email,
                                                                  RecipientUserProfileId = i.UserProfileId,
                                                                  RecipientFirstName = i.FirstName,
                                                                  RecipientLastName = i.LastName,
                                                                  RecipientUserName = i.UserName,
                                                                  ContentReplacements = emailContentReplacements,
                                                                  RecipientOrgulatoryProgramId = inviterOrganizationRegulatoryProgram.RegulatoryProgramId,
                                                                  RecipientOrganizationId = inviterOrganizationRegulatoryProgram.OrganizationId,
                                                                  RecipientRegulatorOrganizationId = inviterOrganizationRegulatoryProgram.RegulatorOrganizationId,
                                                                  EmailType = isInvitedToIndustry
                                                                                  ? EmailType.Registration_IndustryUserRegistrationPendingToApprovers
                                                                                  : EmailType.Registration_AuthorityUserRegistrationPendingToApprovers
                                                              };

                                             return emailEntry;
                                         }).ToList();
        }

        private async Task DoComerrLogForRegistration(RegistrationType registrationType, UserProfile registeredUser,
                                                      OrganizationRegulatoryProgramDto registeredOrganizationRegulatoryProgram,
                                                      OrganizationDto authorityOrg)
        {
            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                          {
                                              RegulatoryProgramId = registeredOrganizationRegulatoryProgram.RegulatoryProgramId,
                                              OrganizationId = registeredOrganizationRegulatoryProgram.OrganizationId,
                                              RegulatorOrganizationId = registeredOrganizationRegulatoryProgram.RegulatorOrganizationId
                                                                        ?? registeredOrganizationRegulatoryProgram.OrganizationId,
                                              UserProfileId = registeredUser.UserProfileId,
                                              UserName = registeredUser.UserName,
                                              UserFirstName = registeredUser.FirstName,
                                              UserLastName = registeredUser.LastName,
                                              UserEmailAddress = registeredUser.Email,
                                              IPAddress = _httpContext.CurrentUserIPAddress(),
                                              HostName = _httpContext.CurrentUserHostName()
                                          };

            var contentReplacements = new Dictionary<string, string>
                                      {
                                          {"authorityName", authorityOrg.OrganizationName},
                                          {"organizationName", registeredOrganizationRegulatoryProgram.OrganizationDto.OrganizationName},
                                          {"regulatoryProgram", registeredOrganizationRegulatoryProgram.RegulatoryProgramDto.Name},
                                          {"firstName", registeredUser.FirstName},
                                          {"lastName", registeredUser.LastName},
                                          {"userName", registeredUser.UserName},
                                          {"emailAddress", registeredUser.Email},
                                          {"actorFirstName", registeredUser.FirstName},
                                          {"actorLastName", registeredUser.LastName},
                                          {"actorUserName", registeredUser.UserName},
                                          {"actorEmailAddress", registeredUser.Email}
                                      };

            await _crommerAuditLogService.Log(eventType:CromerrEvent.Registration_RegistrationPending, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);

            if (registrationType == RegistrationType.ResetRegistration)
            {
                cromerrAuditLogEntryDto.RegulatoryProgramId = registeredOrganizationRegulatoryProgram.RegulatoryProgramId;
                cromerrAuditLogEntryDto.OrganizationId = registeredOrganizationRegulatoryProgram.OrganizationId;
                cromerrAuditLogEntryDto.RegulatorOrganizationId = registeredOrganizationRegulatoryProgram.RegulatorOrganizationId
                                                                  ?? registeredOrganizationRegulatoryProgram.OrganizationId;

                await _crommerAuditLogService.Log(eventType:CromerrEvent.UserAccess_AccountResetSuccessful, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);
            }
        }

        private void ThrowUserStatusRuleValiation(string message)
        {
            SignOff();
            var validationIssues = new List<RuleViolation>();
            validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
            throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
        }

        /// <summary>
        ///     Log to Cromerr events:
        ///     1) attempting to log into an account that is password locked
        ///     2) attempting to log into an account that is manually locked by Authority
        ///     3) getting locked out due to too many password attempts
        ///     4) attempting to log into an account that has been reset
        /// </summary>
        /// <param name="user"> Actor attempting the signin activity </param>
        /// <param name="cromerrEvent"> Used to determine if user just got locked out OR they were previously locked out, or reset </param>
        private void LogProhibitedSignInActivityToCromerr(UserProfile user, CromerrEvent cromerrEvent)
        {
            //Need to log for all associated regulatory program orgs
            var orgRegProgUsers = _dbContext.OrganizationRegulatoryProgramUsers
                                            .Include(path:"OrganizationRegulatoryProgram")
                                            .Where(u => u.UserProfileId == user.UserProfileId).ToList();
            foreach (var orgRegProgUser in orgRegProgUsers)
            {
                var orgRegProgram = orgRegProgUser.OrganizationRegulatoryProgram;

                var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                              {
                                                  RegulatoryProgramId = orgRegProgram.RegulatoryProgramId,
                                                  OrganizationId = orgRegProgram.OrganizationId
                                              };
                cromerrAuditLogEntryDto.RegulatorOrganizationId = orgRegProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                cromerrAuditLogEntryDto.UserProfileId = user.UserProfileId;
                cromerrAuditLogEntryDto.UserName = user.UserName;
                cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
                cromerrAuditLogEntryDto.UserLastName = user.LastName;
                cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
                cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
                var contentReplacements = new Dictionary<string, string>
                                          {
                                              {"firstName", user.FirstName},
                                              {"lastName", user.LastName},
                                              {"userName", user.UserName},
                                              {"emailAddress", user.Email}
                                          };

                _crommerAuditLogService.Log(eventType:cromerrEvent, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);
            }
        }

        // Validate user access for UC-29(4.a, 5.a, 6.a)
        private bool ValidateUserAccess(UserProfile userProfile, SignInResultDto signInResultDto)
        {
            var orpus = _programService.GetUserRegulatoryPrograms(email:userProfile.Email, isIncludeRemoved:true, isIncludeDisabled:true).ToList();
            if (orpus.Any())
            {
                // User at least has one approved program
                if (orpus.Any(i => i.IsRegistrationApproved && i.IsRegistrationDenied == false && i.IsEnabled && i.OrganizationRegulatoryProgramDto.IsEnabled))
                {
                    return true;
                }

                // UC-29 4.a
                // System confirms user has status “Registration Pending” (and no access to any other portal where registration is not pending or portal is not disabled)
                if (orpus.Any(i => i.IsRegistrationApproved == false && i.IsRegistrationDenied == false && i.IsEnabled && i.OrganizationRegulatoryProgramDto.IsEnabled))
                {
                    LogToCromerrThisEvent(programUsers:orpus, cromerrEvent:CromerrEvent.Login_RegistrationPending);
                    signInResultDto.AutehticationResult = AuthenticationResult.RegistrationApprovalPending;
                    return false;
                }

                // UC-29 5.a, User account is disabled for all industry, or authority or application administrator
                // If the user is disabled for all programs
                if (orpus.All(u => u.IsEnabled == false && u.IsRemoved == false)
                    && //--- user is disabled for all industry and authority 
                    userProfile.IsInternalAccount == false //--- user is disabled for Application administrator.
                )
                {
                    LogToCromerrThisEvent(programUsers:orpus, cromerrEvent:CromerrEvent.Login_UserDisabled);
                    signInResultDto.AutehticationResult = AuthenticationResult.UserIsDisabled;
                    return false;
                }

                // 6.a determine user doesn't have access to any enabled industry or authority 
                if (orpus.Any(i => i.IsRegistrationApproved && i.IsEnabled && !i.IsRemoved && i.OrganizationRegulatoryProgramDto.IsEnabled) == false)
                {
                    LogToCromerrThisEvent(programUsers:orpus, cromerrEvent:CromerrEvent.Login_NoAssociation);
                    signInResultDto.AutehticationResult = AuthenticationResult.AccountIsNotAssociated;
                    return false;
                }

                // 6.b returned in the intermediate page call from here. 
            }
            else
            {
                // If user doesn't have any program, return below message
                LogToCromerrThisEvent(programUsers:orpus, cromerrEvent:CromerrEvent.Login_NoAssociation);
                signInResultDto.AutehticationResult = AuthenticationResult.AccountIsNotAssociated;
                return false;
            }

            return true;
        }

        private void LogToCromerrThisEvent(IEnumerable<OrganizationRegulatoryProgramUserDto> programUsers, CromerrEvent cromerrEvent)
        {
            foreach (var programUser in programUsers)
            {
                var user = programUser.UserProfileDto;
                var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                              {
                                                  RegulatoryProgramId = programUser.OrganizationRegulatoryProgramDto.RegulatoryProgramId,
                                                  OrganizationId = programUser.OrganizationRegulatoryProgramDto.OrganizationId
                                              };
                cromerrAuditLogEntryDto.RegulatorOrganizationId = programUser.OrganizationRegulatoryProgramDto.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                cromerrAuditLogEntryDto.UserProfileId = programUser.UserProfileId;
                cromerrAuditLogEntryDto.UserName = user.UserName;
                cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
                cromerrAuditLogEntryDto.UserLastName = user.LastName;
                cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
                cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
                var contentReplacements = new Dictionary<string, string>
                                          {
                                              {"firstName", user.FirstName},
                                              {"lastName", user.LastName},
                                              {"userName", user.UserName},
                                              {"emailAddress", user.Email}
                                          };

                _crommerAuditLogService.Log(eventType:cromerrEvent, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);
            }
        }

        #region private section

        // Check if password is in one of the last # passwords stores in UserPasswordHistory table
        // Return false means the new password is not validate because it has been used in the last #number of times
        // Return true means the new password is validate to use
        private bool IsValidPasswordCheckInHistory(string password, int userProfileId, IEnumerable<SettingDto> organizationSettings)
        {
            var numberOfPasswordsInHistory = GetStrictestPasswordHistoryCounts(organizationSettings:organizationSettings, orgTypeName:null);

            var lastNumberPasswordInHistory = _dbContext.UserPasswordHistories
                                                        .Where(i => i.UserProfileId == userProfileId)
                                                        .OrderByDescending(i => i.LastModificationDateTimeUtc).Take(count:numberOfPasswordsInHistory)
                                                        .ToList();

            if (lastNumberPasswordInHistory.Any(i => IsValidPassword(passwordHash:i.PasswordHash, password:password)))
            {
                return false;
            }

            return true;
        }

        private bool IsValidPassword(string passwordHash, string password)
        {
            return _userManager.PasswordHasher.VerifyHashedPassword(hashedPassword:passwordHash, providedPassword:password) == PasswordVerificationResult.Success;
        }

        private bool IsUserPasswordExpired(int userProfileId, IEnumerable<SettingDto> organizationSettings)
        {
            var lastestUserPassword = _dbContext.UserPasswordHistories
                                                .Where(i => i.UserProfileId == userProfileId)
                                                .OrderByDescending(i => i.LastModificationDateTimeUtc).FirstOrDefault();

            if (lastestUserPassword == null || lastestUserPassword.UserProfileId == 0)
            {
                return false;
            }

            // Get password expiration setting
            var passwordExpiredDays = GetStrictestLengthPasswordExpiredDays(organizationSettings:organizationSettings, orgTypeName:null);
            if (DateTimeOffset.UtcNow > lastestUserPassword.LastModificationDateTimeUtc.AddDays(days:passwordExpiredDays))
            {
                return true;
            }

            return false;
        }

        private void ClearClaims(string userId)
        {
            if (string.IsNullOrWhiteSpace(value:userId))
            {
                return;
            }

            var user = _userManager.FindById(userId:userId);
            if (user == null)
            {
                return;
            }

            var claims = _userManager.GetClaims(userId:userId).ToList();
            foreach (var claim in claims)
            {
                _userManager.RemoveClaim(userId:userId, claim:claim);
            }
        }

        private UserProfile AssignUser(string userName, string email)
        {
            return new UserProfile
                   {
                       UserName = userName,
                       Email = email
                   };
        }

        private List<Claim> GetUserIdentity(UserProfile userProfile)
        {
            // get userDto's role, organizations, programs, current organization, current program.....

            var claims = new List<Claim>
                         {
                             new Claim(type:ClaimTypes.NameIdentifier, value:userProfile.Id),
                             new Claim(type:CacheKey.OwinUserId, value:userProfile.Id),
                             new Claim(type:CacheKey.UserProfileId, value:userProfile.UserProfileId.ToString()),
                             new Claim(type:CacheKey.FirstName, value:userProfile.FirstName),
                             new Claim(type:CacheKey.LastName, value:userProfile.LastName),
                             new Claim(type:CacheKey.UserName, value:userProfile.UserName),
                             new Claim(type:CacheKey.Email, value:userProfile.Email),
                             new Claim(type:CacheKey.SessionId, value:_httpContext.Current.Session.SessionID)
                         };

            return claims;
        }

        private void SaveClaims(string userId, IList<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims:claims, authenticationType:DefaultAuthenticationTypes.ApplicationCookie);

            var authProperties = new AuthenticationProperties();

            var existingClaims = _userManager.GetClaims(userId:userId);

            foreach (var claim in claims)
            {
                if (!IsHavingClaim(claims:existingClaims, claim:claim))
                {
                    _userManager.AddClaim(userId:userId, claim:claim);
                }
            }

            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            _authenticationManager.SignIn(authProperties, identity);
        }

        private bool IsHavingClaim(IList<Claim> claims, Claim claim)
        {
            foreach (var cl in claims)
            {
                if (cl.Type.Equals(value:claim.Type))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetPasswordPolicy(IEnumerable<SettingDto> organizationSettings)
        {
            // Password policy is only defined on system global level 
            // Failed trial times is defined on organization level 

            // If one setting has multiple definitions, choose the strictest one
            _userManager.UserLockoutEnabledByDefault = true;
            _userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromHours(value:_settingService.PasswordLockoutHours());

            _userManager.MaxFailedAccessAttemptsBeforeLockout = MaxFailedPasswordAttempts(organizationSettings:organizationSettings, orgTypeName:null);
        }

        #region organization setting;

        private int GetSettingIntValue(SettingType settingType, IEnumerable<SettingDto> organizationSettingsValue, OrganizationTypeName? orgTypeName, bool isMax = true)
        {
            var defaultValueStr = _settingService.GetSettingTemplateValue(settingType:settingType, orgType:orgTypeName);
            var defaultValue = ValueParser.TryParseInt(value:defaultValueStr, defaultValue:0);
            var organizationSettings = organizationSettingsValue.ToList();
            if (organizationSettings.Any())
            {
                if (orgTypeName != null)
                {
                    defaultValue = isMax
                                       ? organizationSettings
                                           .Where(i => i.TemplateName == settingType && i.OrgTypeName == orgTypeName)
                                           .Max(i => ValueParser.TryParseInt(value:i.Value, defaultValue:defaultValue))
                                       : organizationSettings.Where(i => i.TemplateName == settingType && i.OrgTypeName == orgTypeName)
                                                             .Min(i => ValueParser.TryParseInt(value:i.Value, defaultValue:defaultValue));
                }
                else
                {
                    defaultValue = isMax
                                       ? organizationSettings
                                           .Where(i => i.TemplateName == settingType).Max(i => ValueParser.TryParseInt(value:i.Value, defaultValue:defaultValue))
                                       : organizationSettings.Where(i => i.TemplateName == settingType)
                                                             .Min(i => ValueParser.TryParseInt(value:i.Value, defaultValue:defaultValue));
                }
            }

            return defaultValue;
        }

        private int GetStrictestPasswordHistoryCounts(IEnumerable<SettingDto> organizationSettings, OrganizationTypeName? orgTypeName)
        {
            return GetSettingIntValue(settingType:SettingType.PasswordHistoryMaxCount, organizationSettingsValue:organizationSettings, orgTypeName:orgTypeName, isMax:false);
        }

        private int GetStrictestLengthPasswordExpiredDays(IEnumerable<SettingDto> organizationSettings, OrganizationTypeName? orgTypeName)
        {
            return GetSettingIntValue(settingType:SettingType.PasswordChangeRequiredDays, organizationSettingsValue:organizationSettings, orgTypeName:orgTypeName, isMax:false);
        }

        private int MaxFailedPasswordAttempts(IEnumerable<SettingDto> organizationSettings, OrganizationTypeName? orgTypeName)
        {
            return GetSettingIntValue(settingType:SettingType.FailedPasswordAttemptMaxCount, organizationSettingsValue:organizationSettings, orgTypeName:orgTypeName, isMax:false);
        }

        #endregion

        private IEnumerable<int> GetUserAuthorityOrganizationIds(int userid)
        {
            var authorityOrgIds = new List<int>();
            var orgRegPrograms = _organizationService.GetUserOrganizations(userId:userid);
            foreach (var orgRegProgram in orgRegPrograms)
            {
                authorityOrgIds.Add(item:orgRegProgram.RegulatorOrganizationId ?? orgRegProgram.OrganizationId);
            }

            return authorityOrgIds;
        }

        private void SendResetPasswordConfirmationEmail(UserProfile userProfile)
        {
            //var token = _userManager.GeneratePasswordResetTokenAsync(userProfile.Id).Result; 
            var token = Guid.NewGuid().ToString();

            var baseUrl = _httpContext.GetRequestBaseUrl();
            var link = baseUrl + "Account/ResetPassword/?token=" + token;

            var supportPhoneNumber = _globalSettings[key:SystemSettingType.SupportPhoneNumber];
            var supportEmail = _globalSettings[key:SystemSettingType.SupportEmailAddress];

            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add(key:"link", value:link);
            contentReplacements.Add(key:"supportPhoneNumber", value:supportPhoneNumber);
            contentReplacements.Add(key:"supportEmail", value:supportEmail);

            _requestCache.SetValue(key:CacheKey.Token, value:token);

            //ForgotPassword_ForgotPassword: Email Audit logs to all programs  
            var emailEntries =
                _linkoExchangeEmailService.GetAllProgramEmailEntiresForUser(userProfile:userProfile, emailType:EmailType.ForgotPassword_ForgotPassword,
                                                                            contentReplacements:contentReplacements);

            // Do email audit log.
            _linkoExchangeEmailService.WriteEmailAuditLogs(emailEntries:emailEntries);

            // Send emails.
            _linkoExchangeEmailService.SendEmails(emailEntries:emailEntries);
        }

        private void SendRequestUsernameEmail(UserProfile userProfile)
        {
            var baseUrl = _httpContext.GetRequestBaseUrl();
            var link = baseUrl + "Account/SignIn";

            var supportPhoneNumber = _globalSettings[key:SystemSettingType.SupportPhoneNumber];
            var supportEmail = _globalSettings[key:SystemSettingType.SupportEmailAddress];

            var contentReplacements = new Dictionary<string, string>
                                      {
                                          {"userName", userProfile.UserName},
                                          {"link", link},
                                          {"supportPhoneNumber", supportPhoneNumber},
                                          {"supportEmail", supportEmail}
                                      };

            // ForgotUserName_ForgotUserName email logs for all programs
            var emailEntries = _linkoExchangeEmailService.GetAllProgramEmailEntiresForUser(userProfile:userProfile, emailType:EmailType.ForgotUserName_ForgotUserName,
                                                                                           contentReplacements:contentReplacements);

            // Do email audit log.
            _linkoExchangeEmailService.WriteEmailAuditLogs(emailEntries:emailEntries);

            // Send emails.
            _linkoExchangeEmailService.SendEmails(emailEntries:emailEntries);
        }

        private RegistrationResult ValidateRegistrationData(UserDto userProfile, IEnumerable<AnswerDto> securityQuestions, IEnumerable<AnswerDto> kbqQuestions)
        {
            if (userProfile.AgreeTermsAndConditions == false)
            {
                return RegistrationResult.NotAgreedTermsAndConditions;
            }

            // To verify user's password  
            var passwordRequiredLengthFromWebConfig = ValueParser.TryParseInt(value:ConfigurationManager.AppSettings[name:"PasswordRequiredLength"], defaultValue:8);
            var passwordRequiredLength = ValueParser.TryParseInt(value:_globalSettings[key:SystemSettingType.PasswordRequiredLength],
                                                                 defaultValue:passwordRequiredLengthFromWebConfig);
            var passwordRequiredMaxLength = ValueParser.TryParseInt(value:_globalSettings[key:SystemSettingType.PasswordRequiredMaxLength], defaultValue:16);

            if (userProfile.Password.Length < passwordRequiredLength || userProfile.Password.Length > passwordRequiredMaxLength)
            {
                return RegistrationResult.BadPassword;
            }

            var validPassword = _userManager.PasswordValidator.ValidateAsync(item:userProfile.Password).Result;
            if (validPassword.Succeeded == false)
            {
                return RegistrationResult.BadPassword;
            }

            return _userService.ValidateRegistrationUserData(userProfile:userProfile, securityQuestions:securityQuestions, kbqQuestions:kbqQuestions);
        }

        public void UpdateClaim(string key, string value)
        {
            var currentClaims = GetClaims();
            if (currentClaims != null)
            {
                var claim = currentClaims.FirstOrDefault(i => i.Type == key);
                if (claim != null)
                {
                    currentClaims.Remove(item:claim);
                }

                currentClaims.Add(item:new Claim(type:key, value:value));
            }

            var owinUserId = GetClaimsValue(claimType:CacheKey.OwinUserId);

            ClearClaims(userId:owinUserId);
            SaveClaims(userId:owinUserId, claims:currentClaims);
        }

        #endregion
    }
}