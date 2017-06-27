using System;
using System.Collections.Generic;
using System.Configuration;
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
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.User;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NLog;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.TermCondition;

namespace Linko.LinkoExchange.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly LinkoExchangeContext _dbContext;

        private readonly ApplicationSignInManager _signInManager;
        private readonly ApplicationUserManager _userManager;

        private readonly ISettingService _settingService;
        private readonly IOrganizationService _organizationService;
        private readonly IProgramService _programService;
        private readonly IInvitationService _invitationService;
        private readonly IAuthenticationManager _authenticationManager;
        private readonly IPermissionService _permissionService;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;
        private readonly ISessionCache _sessionCache;
        private readonly IRequestCache _requestCache;
        private readonly IPasswordHasher _passwordHasher;

        private readonly IDictionary<SystemSettingType, string> _globalSettings;
        private readonly IHttpContextService _httpContext;
        private readonly ILogger _logger;
        private readonly IQuestionAnswerService _questionAnswerService;
        private readonly IMapHelper _mapHelper;
        private readonly ICromerrAuditLogService _crommerAuditLogService;
        private readonly ITermConditionService _termConditionService;

        public AuthenticationService(ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            IAuthenticationManager authenticationManager,
            ISettingService settingService,
            IOrganizationService organizationService,
            IProgramService programService,
            IInvitationService invitationService,
            IEmailService emailService,
            IPermissionService permissionService,
            LinkoExchangeContext linkoExchangeContext,
            IUserService userService
           , ISessionCache sessionCache
           , IRequestCache requestCache
           , IPasswordHasher passwordHasher
           , IHttpContextService httpContext
           , ILogger logger
           , IQuestionAnswerService questionAnswerService
           , IMapHelper mapHelper
           , ICromerrAuditLogService crommerAuditLogService
           , ITermConditionService termConditionService
            )
        {
            if (linkoExchangeContext == null) throw new ArgumentNullException(paramName: "linkoExchangeContext");
            if (userManager == null) throw new ArgumentNullException(paramName: "userManager");
            if (signInManager == null) throw new ArgumentNullException(paramName: "signInManager");
            if (authenticationManager == null) throw new ArgumentNullException(paramName: "authenticationManager");
            if (settingService == null) throw new ArgumentNullException(paramName: "settingService");
            if (organizationService == null) throw new ArgumentNullException(paramName: "organizationService");
            if (programService == null) throw new ArgumentNullException(paramName: "programService");
            if (invitationService == null) throw new ArgumentNullException(paramName: "invitationService");
            if (emailService == null) throw new ArgumentNullException(paramName: "emailService");
            if (permissionService == null) throw new ArgumentNullException(paramName: "permissionService");
            if (userService == null) throw new ArgumentNullException("userService");
            if (sessionCache == null) throw new ArgumentNullException("sessionCache");
            if (requestCache == null) throw new ArgumentNullException("requestCache");
            if (httpContext == null) throw new ArgumentNullException("httpContext");
            if (logger == null) throw new ArgumentNullException("logger");
            if (questionAnswerService == null) throw new ArgumentNullException("questionAnswerService");
            if (mapHelper == null) throw new ArgumentNullException("mapHelper");
            if (crommerAuditLogService == null) throw new ArgumentNullException("crommerAuditLogService");
            if (termConditionService == null) throw new ArgumentNullException("termConditionService");

            _dbContext = linkoExchangeContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _authenticationManager = authenticationManager;
            _settingService = settingService;
            _organizationService = organizationService;
            _programService = programService;
            _invitationService = invitationService;
            _globalSettings = _settingService.GetGlobalSettings();
            _emailService = emailService;
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
        }

        public IList<Claim> GetClaims()
        {
            if (_httpContext.Current().User.Identity.IsAuthenticated)
            {
                var identity = _httpContext.Current().User.Identity as ClaimsIdentity;
                if (identity == null)
                {
                    return null;
                }

                var claims = identity.Claims.ToList();

                var uClaims = new Dictionary<string, Claim>();

                foreach (var claim in claims)
                {
                    if (!uClaims.ContainsKey(claim.Type))
                    {
                        uClaims.Add(claim.Type, claim);
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
        /// Set current user's additional claims, such as current organizationId, current authorityId, current programId
        /// </summary>
        /// <param name="claims">The claims to set</param>
        public void SetCurrentUserClaims(IDictionary<string, string> claims)
        {
            if (claims == null || claims.Count < 1)
                return;

            var currentClaims = GetClaims();
            if (currentClaims != null)
            {
                var owinUserId = currentClaims.FirstOrDefault(i => i.Type == CacheKey.OwinUserId).Value;
                var itor = claims.GetEnumerator();

                while (itor.MoveNext())
                {
                    currentClaims.Add(new Claim(itor.Current.Key, itor.Current.Value));
                }

                ClearClaims(owinUserId);
                SaveClaims(owinUserId, currentClaims);
            }
        }

        // Change or reset password
        /// <summary>
        /// Change password happens after a user login, and change his password.
        /// New password must meet the following criteria
        /// 1. Meet the strictest password policies when the user have multiple access to organizations and programs 
        /// 2. Can not be the same as the last X number of passwords saved in UserPasswordHistory table.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="newPassword">The new password</param>
        /// <returns></returns>
        public Task<AuthenticationResultDto> ChangePasswordAsync(string userId, string newPassword)
        {
            var authenticationResult = new AuthenticationResultDto();
            try
            {
                var applicationUser = _userManager.FindById(userId);
                if (applicationUser == null)
                {
                    authenticationResult.Success = false;
                    authenticationResult.Result = AuthenticationResult.UserNotFound;
                    return Task.FromResult(authenticationResult);
                }

                var authorityOrganizationIds = GetUserAuthorityOrganizationIds(applicationUser.UserProfileId);
                var organizationSettings = _settingService.GetOrganizationSettingsByIds(authorityOrganizationIds).SelectMany(i => i.Settings).ToList();

                SetPasswordPolicy(organizationSettings);
                // Use PasswordValidator
                var validateResult = _userManager.PasswordValidator.ValidateAsync(newPassword).Result;
                if (validateResult.Succeeded == false)
                {
                    authenticationResult.Success = false;
                    authenticationResult.Errors = validateResult.Errors;
                    return Task.FromResult(authenticationResult);
                }

                // Check if the new password is one of the password used last # numbers

                if (!IsValidPasswordCheckInHistory(newPassword, applicationUser.UserProfileId, organizationSettings))
                {
                    var numberOfPasswordsInHistory = GetStrictestPasswordHistoryCounts(organizationSettings, null);
                    authenticationResult.Success = false;
                    authenticationResult.Result = AuthenticationResult.CanNotUseOldPassword;
                    authenticationResult.Errors = new[] { $"You cannot use the last {numberOfPasswordsInHistory} passwords." };
                    return Task.FromResult(authenticationResult);
                }

                _userManager.RemovePassword(userId);
                _userManager.AddPassword(userId, newPassword);

                //create history record
                UserPasswordHistory history = _dbContext.UserPasswordHistories.Create();
                history.UserProfileId = applicationUser.UserProfileId;
                history.PasswordHash = _passwordHasher.HashPassword(newPassword);
                history.LastModificationDateTimeUtc = DateTimeOffset.Now;
                _dbContext.UserPasswordHistories.Add(history);
                _dbContext.SaveChanges();

                //Send Email
                var contentReplacements = new Dictionary<string, string>();
                string supportPhoneNumber = _globalSettings[SystemSettingType.SupportPhoneNumber];
                string supportEmail = _globalSettings[SystemSettingType.SupportEmailAddress];

                var authorityList = _organizationService.GetUserAuthorityListForEmailContent(applicationUser.UserProfileId);
                contentReplacements.Add("firstName", applicationUser.FirstName);
                contentReplacements.Add("lastName", applicationUser.LastName);
                contentReplacements.Add("authorityList", authorityList);
                contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
                contentReplacements.Add("supportEmail", supportEmail);

                _emailService.SendEmail(new[] { applicationUser.Email }, EmailType.Profile_PasswordChanged, contentReplacements);

                //Cromerr
                //Need to log for all associated regulatory program orgs
                var orgRegProgUsers = _dbContext.OrganizationRegulatoryProgramUsers
                                        .Include("OrganizationRegulatoryProgram")
                                        .Where(u => u.UserProfileId == applicationUser.UserProfileId).ToList();
                foreach (var actorProgramUser in orgRegProgUsers)
                {
                    _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(actorProgramUser);

                    var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
                    cromerrAuditLogEntryDto.RegulatoryProgramId = actorProgramUser.OrganizationRegulatoryProgram.RegulatoryProgramId;
                    cromerrAuditLogEntryDto.OrganizationId = actorProgramUser.OrganizationRegulatoryProgram.OrganizationId;
                    cromerrAuditLogEntryDto.RegulatorOrganizationId = actorProgramUser.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                    cromerrAuditLogEntryDto.UserProfileId = actorProgramUser.UserProfileId;
                    cromerrAuditLogEntryDto.UserName = applicationUser.UserName;
                    cromerrAuditLogEntryDto.UserFirstName = applicationUser.FirstName;
                    cromerrAuditLogEntryDto.UserLastName = applicationUser.LastName;
                    cromerrAuditLogEntryDto.UserEmailAddress = applicationUser.Email;
                    cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                    cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
                    contentReplacements = new Dictionary<string, string>();
                    contentReplacements.Add("firstName", applicationUser.FirstName);
                    contentReplacements.Add("lastName", applicationUser.LastName);
                    contentReplacements.Add("userName", applicationUser.UserName);
                    contentReplacements.Add("emailAddress", applicationUser.Email);

                    _crommerAuditLogService.Log(CromerrEvent.Profile_PasswordChanged, cromerrAuditLogEntryDto, contentReplacements);
                }

            }
            catch (Exception ex)
            {
                authenticationResult.Success = false;
                var errors = new List<string> { ex.Message };
                authenticationResult.Errors = errors;
            }
            return Task.FromResult(authenticationResult);
        }

        public ICollection<RegistrationResult> ValidateUserProfileForRegistration(UserDto userInfo, RegistrationType registrationType)
        {
            List<RegistrationResult> inValidUserProfileMessages = new List<RegistrationResult>();  

            var result = _userService.ValidateUserProfileData(userInfo);
            if (result != RegistrationResult.Success)
            {
                inValidUserProfileMessages.Add(RegistrationResult.BadUserProfileData);
            }

            if (userInfo == null)
            {
                inValidUserProfileMessages.Add(RegistrationResult.BadUserProfileData);
            }

            if (string.IsNullOrWhiteSpace(userInfo.UserName))
            {
              inValidUserProfileMessages.Add(RegistrationResult.MissingUserName);
            }

            if(string.IsNullOrWhiteSpace(userInfo.Email))
            { 
              inValidUserProfileMessages.Add(RegistrationResult.MissingEmailAddress); 
            }

            return inValidUserProfileMessages; 
        }

        /// <summary>
        /// Create a new user for registration
        /// Confirmed: No possible for one user being invited to a program that he is in already. 
        /// </summary>
        /// <param name="userInfo">The registration user information.</param>
        /// <param name="registrationToken">Registration token</param>
        /// <param name="securityQuestions">Security questions</param>
        /// <param name="kbqQuestions">KBQ questions</param>
        /// <param name="registrationType">Registration type</param>
        /// <returns>Registration results.</returns>
        public async Task<RegistrationResultDto> Register(
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
                if (string.IsNullOrWhiteSpace(userInfo.UserName))
                {
                    var errText = $"Username cannot be null or whitespace.";
                    _logger.Error(errText);
                    throw new Exception(errText);
                }
                userInfo.UserName = userInfo.UserName.Trim();

                if (string.IsNullOrWhiteSpace(registrationToken))
                {
                    registrationResult.Result = RegistrationResult.InvalidRegistrationToken;
                    return registrationResult;
                }

                _logger.Info("Register. userName={0}, registrationToken={1}", userInfo.UserName, registrationToken);

                var validatResult = ValidateRegistrationData(userInfo, securityQuestions, kbqQuestions);
                if (validatResult != RegistrationResult.Success)
                {
                    registrationResult.Result = validatResult;
                    return registrationResult;
                }
            }

            var invitationDto = _invitationService.GetInvitation(registrationToken);

            if (invitationDto == null)
            {
                registrationResult.Result = RegistrationResult.InvalidRegistrationToken;
                return registrationResult;
            }

            #region Check invitation expiration 

            // UC-42 1.a 
            // Check token is expired or not? from organization settings
            var invitationRecipientProgram =
                _programService.GetOrganizationRegulatoryProgram(invitationDto.RecipientOrganizationRegulatoryProgramId);
            var inivitationRecipintOrganizationSettings =
                _settingService.GetOrganizationSettingsById(invitationRecipientProgram.RegulatorOrganizationId ?? invitationRecipientProgram.OrganizationId); // always get the authority settings as currently industry don't have settings 

            var invitationExpirationHours = ValueParser.TryParseInt(ConfigurationManager.AppSettings["DefaultInviteExpirationHours"], 72);
            if (inivitationRecipintOrganizationSettings.Settings.Any())
            {
                invitationExpirationHours = ValueParser.TryParseInt(inivitationRecipintOrganizationSettings
                  .Settings.Single(i => i.TemplateName == SettingType.InvitationExpiredHours).Value, invitationExpirationHours);
            }

            if (DateTimeOffset.UtcNow > invitationDto.InvitationDateTimeUtc.AddHours(invitationExpirationHours))
            {
                registrationResult.Result = RegistrationResult.InvitationExpired;
                return registrationResult;
            }

            #endregion  End of Checking invitation expiration 

            // TODO: Need to check invitation email address same as user info email address. Otherwise, in future if user can update email address then it might update wrong user info

            // Email should be unique.
            UserProfile applicationUser = _userManager.FindByEmail(userInfo.Email);

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
                    var testUser = _userManager.FindByName(userInfo.UserName);
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
                        applicationUser = AssignUser(userInfo.UserName, userInfo.Email);

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

                        var result = _userManager.Create(applicationUser, userInfo.Password);
                        if (result == IdentityResult.Success)
                        {
                            // Retrieve user again to get userProfile Id. 
                            applicationUser = _userManager.FindById(applicationUser.Id);
                        }
                        else
                        {
                            var errText = $"Creating user failed. Email={userInfo.Email}, userName={userInfo.UserName}";
                            _logger.Error(errText);
                            throw new Exception(errText);
                        }
                    }

                    #endregion

                    #region User is from re-registration 

                    // Existing user re-register
                    // Check if the password has been in # password in history
                    if (registrationType == RegistrationType.ResetRegistration)
                    {
                        string passwordHash = _passwordHasher.HashPassword(userInfo.Password);

                        var authorityOrganizationIds = GetUserAuthorityOrganizationIds(applicationUser.UserProfileId);
                        var organizationSettings = _settingService.GetOrganizationSettingsByIds(authorityOrganizationIds).SelectMany(i => i.Settings).ToList();

                        if (!IsValidPasswordCheckInHistory(userInfo.Password, applicationUser.UserProfileId, organizationSettings))
                        {
                            var numberOfPasswordsInHistory = GetStrictestPasswordHistoryCounts(organizationSettings, null);
                            registrationResult.Result = RegistrationResult.CanNotUseLastNumberOfPasswords;
                            registrationResult.Errors = new string[] {
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
                        _questionAnswerService.DeleteUserQuestionAndAnswers(applicationUser.UserProfileId);

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
                        _dbContext.UserPasswordHistories.Add(new UserPasswordHistory
                        {
                            LastModificationDateTimeUtc = DateTimeOffset.UtcNow,
                            PasswordHash = applicationUser.PasswordHash,
                            UserProfileId = applicationUser.UserProfileId
                        });

                        // Save Security questions and kbq questions
                        var combined = securityQuestions.Concat(kbqQuestions);
                        _questionAnswerService.CreateUserQuestionAnswers(applicationUser.UserProfileId, combined);
                    }

                    #endregion 

                    // UC-42 6
                    // 2 Create organization regulatory program userProfile, and set the approved statue to false  
                    var orpu = _programService.CreateOrganizationRegulatoryProgramForUser(applicationUser.UserProfileId, invitationDto.RecipientOrganizationRegulatoryProgramId, invitationDto.SenderOrganizationRegulatoryProgramId, registrationType);

                    // need to move inside _programService.CreateOrganizationRegulatoryProgramForUser otherwise it will be send email to all approver when registrationType == RegistrationType.ResetRegistration 
                    // UC-42 7, 8
                    // Find out who have approval permission    
                    var sendTo = new List<string>(); 
                    if(registrationType == RegistrationType.ResetRegistration)
                    {
                       sendTo = _permissionService.GetAllAuthoritiesApprovalPeopleForUser(applicationUser.UserProfileId).Select(i=>i.Email).ToList();   
                    } 
                    else
                    {
                        sendTo = _permissionService.GetApprovalPeople(invitationDto.SenderOrganizationRegulatoryProgramId).Select(i => i.Email).ToList();
                    }

                    //  Determine if user is authority user or is industry user; 
                    var senderProgram = _programService.GetOrganizationRegulatoryProgram(invitationDto.SenderOrganizationRegulatoryProgramId);

                    var recipientProgram = _programService.GetOrganizationRegulatoryProgram(invitationDto.RecipientOrganizationRegulatoryProgramId);
                    //  Program is disabled or not found
                    //  UC-42 2.c, 2.d, 2.e
                    if (recipientProgram == null || senderProgram == null ||
                         !recipientProgram.IsEnabled || !senderProgram.IsEnabled ||
                         recipientProgram.OrganizationDto == null
                         )
                    {
                        registrationResult.Result = RegistrationResult.InvitationExpired; //TODO: Is it correct Enum ???
                        return registrationResult;
                    }

                    var isInvitedToIndustry = false;

                    if (recipientProgram.RegulatorOrganizationId.HasValue)
                    {
                        // Invites Industry user
                        isInvitedToIndustry = true;
                    }
                    else
                    {
                        if (senderProgram.RegulatorOrganizationId.HasValue)
                        {
                            // IU invited the authority user; AU can only invite authority user
                            registrationResult.Result = RegistrationResult.Failed;
                            return registrationResult;
                        }
                    }

                    _requestCache.SetValue(CacheKey.Token, registrationToken);

                    var authorityOrg = _organizationService.GetAuthority(recipientProgram.OrganizationRegulatoryProgramId);

                    #region Send registration email to approvals

                    var emailContentReplacements = new Dictionary<string, string>();
                    emailContentReplacements.Add("firstName", applicationUser.FirstName);
                    emailContentReplacements.Add("lastName", applicationUser.LastName);

                    var emailAddressOnEmail = _settingService.GetOrgRegProgramSettingValue(senderProgram.RegulatoryProgramId, SettingType.EmailContactInfoEmailAddress);
                    var phoneNumberOnEmail = _settingService.GetOrgRegProgramSettingValue(senderProgram.RegulatoryProgramId, SettingType.EmailContactInfoPhone);
                    var authorityName = _settingService.GetOrgRegProgramSettingValue(senderProgram.RegulatoryProgramId, SettingType.EmailContactInfoName);

                    emailContentReplacements.Add("supportEmail", emailAddressOnEmail);
                    emailContentReplacements.Add("supportPhoneNumber", phoneNumberOnEmail);
                    emailContentReplacements.Add("authorityName", authorityName);
                    emailContentReplacements.Add("authorityOrganizationName", authorityOrg.OrganizationDto.OrganizationName);
                    emailContentReplacements.Add("organizationName", recipientProgram.OrganizationDto.OrganizationName);

                    if (!sendTo.Any())
                    {
                        sendTo.Add(emailAddressOnEmail);    // send email to authority support email when no approval email found
                    }
                    if (isInvitedToIndustry)
                    {
                        var receipientOrg = _organizationService.GetOrganization(recipientProgram.OrganizationId);

                        emailContentReplacements.Add("addressLine1", receipientOrg.AddressLine1);
                        emailContentReplacements.Add("cityName", receipientOrg.CityName);
                        emailContentReplacements.Add("stateName", receipientOrg.State);
                    }

                    if (isInvitedToIndustry)
                    {
                        await _emailService.SendEmail(recipients: sendTo, emailType: EmailType.Registration_IndustryUserRegistrationPendingToApprovers, contentReplacements: emailContentReplacements);
                    }
                    else
                    {
                        await _emailService.SendEmail(recipients: sendTo, emailType: EmailType.Registration_AuthorityUserRegistrationPendingToApprovers, contentReplacements: emailContentReplacements);
                    }

                    #endregion

                    //Cromerr log
                    int thisUserOrgRegProgUserId = orpu.OrganizationRegulatoryProgramUserId;
                    var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                        .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
                    var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(actorProgramUser);
                    var actorUser = _userService.GetUserProfileById(actorProgramUserDto.UserProfileId);

                    var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                    {
                        RegulatoryProgramId = recipientProgram.RegulatoryProgramId,
                        OrganizationId = recipientProgram.OrganizationId,
                        RegulatorOrganizationId = authorityOrg.OrganizationId,
                        UserProfileId = applicationUser.UserProfileId,
                        UserName = applicationUser.UserName,
                        UserFirstName = applicationUser.FirstName,
                        UserLastName = applicationUser.LastName,
                        UserEmailAddress = applicationUser.Email,
                        IPAddress = _httpContext.CurrentUserIPAddress(),
                        HostName = _httpContext.CurrentUserHostName()
                    };
                    var contentReplacements = new Dictionary<string, string>();
                    contentReplacements.Add("authorityName", authorityOrg.OrganizationDto.OrganizationName);
                    contentReplacements.Add("organizationName", recipientProgram.OrganizationDto.OrganizationName);
                    contentReplacements.Add("regulatoryProgram", recipientProgram.RegulatoryProgramDto.Name);
                    contentReplacements.Add("firstName", applicationUser.FirstName);
                    contentReplacements.Add("lastName", applicationUser.LastName);
                    contentReplacements.Add("userName", applicationUser.UserName);
                    contentReplacements.Add("emailAddress", applicationUser.Email);
                    contentReplacements.Add("actorFirstName", actorUser.FirstName);
                    contentReplacements.Add("actorLastName", actorUser.LastName);
                    contentReplacements.Add("actorUserName", actorUser.UserName);
                    contentReplacements.Add("actorEmailAddress", actorUser.Email);

                    await _crommerAuditLogService.Log(CromerrEvent.Registration_RegistrationPending, cromerrAuditLogEntryDto, contentReplacements);

                    if (registrationType == RegistrationType.ResetRegistration)
                    {
                        //Also log for this additional Cromerr event
                        await _crommerAuditLogService.Log(CromerrEvent.UserAccess_AccountResetSuccessful, cromerrAuditLogEntryDto, contentReplacements);
                    }


                    // All succeed
                    // 4 Remove the invitation from table 
                    _invitationService.DeleteInvitation(invitationDto.InvitationId, true);

                    _dbContext.SaveChanges();
                    transaction.Commit();
                    registrationResult.Result = RegistrationResult.Success;
                }
                catch (Exception ex)
                {
                    var errors = new List<string>() { ex.Message };

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(ex.Message);
                    }

                    _logger.Error("Error happens {0} ", argument: string.Join("," + Environment.NewLine, errors));

                    registrationResult.Result = RegistrationResult.Failed;
                    registrationResult.Errors = errors;
                    transaction.Rollback();
                    throw;
                }
            }

            _logger.Info("Register. userName={0}, email={1}, registrationResult{2}", argument1: userInfo.UserName, argument2: registrationToken, argument3: registrationResult);

            return registrationResult;
        }

        public bool CheckPasswordResetUrlNotExpired(string token)
        {
            bool isNotExpiredYet;
            int resetPasswordTokenValidateInterval = Convert.ToInt32(ConfigurationManager.AppSettings["ResetPasswordTokenValidateInterval"]);
            var emailAuditLog = _dbContext.EmailAuditLogs.FirstOrDefault(e => e.Token == token);
            if (emailAuditLog == null)
            {
                throw new Exception($"ERROR: Cannot find email audit log associated with token={token}");
            }

            DateTimeOffset tokenCreated = emailAuditLog.SentDateTimeUtc;
            if (DateTimeOffset.Now.AddHours(-resetPasswordTokenValidateInterval) > tokenCreated)
            {
                //Check token expiry (5.1.a)
                isNotExpiredYet = false;
                if (emailAuditLog.RecipientUserProfileId.HasValue)
                {
                    var userProfileId = emailAuditLog.RecipientUserProfileId.Value;
                    foreach (var orgRegProgDto in _organizationService.GetUserOrganizations(userProfileId))
                    {
                        var userDto = _userService.GetUserProfileById(userProfileId);
                        _crommerAuditLogService.SimpleLog(CromerrEvent.ForgotPassword_PasswordResetExpired, orgRegProgDto, userDto);
                    }
                }
            }
            else
            {
                isNotExpiredYet = true;
            }

            return isNotExpiredYet;
        }

        public async Task<AuthenticationResultDto> ResetPasswordAsync(string resetPasswordToken, int userQuestionAnswerId,
            string answer, int failedCount, string newPassword)
        {
            var authenticationResult = new AuthenticationResultDto();

            // Use PasswordValidator
            var validateResult = _userManager.PasswordValidator.ValidateAsync(newPassword).Result;
            if (validateResult.Succeeded == false)
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.PasswordRequirementsNotMet;
                authenticationResult.Errors = validateResult.Errors;
                return authenticationResult;
            }

            int resetPasswordTokenValidateInterval = Convert.ToInt32(ConfigurationManager.AppSettings["ResetPasswordTokenValidateInterval"]);

            var emailAuditLog = _dbContext.EmailAuditLogs.FirstOrDefault(e => e.Token == resetPasswordToken);

            if (emailAuditLog == null)
            {
                throw new Exception($"ERROR: Cannot find email audit log associated with token={resetPasswordToken}");
            }

            DateTimeOffset tokenCreated = emailAuditLog.SentDateTimeUtc;

            if (DateTimeOffset.Now.AddHours(-resetPasswordTokenValidateInterval) > tokenCreated)
            {
                //Check token expiry (5.1.a)

                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.ExpiredRegistrationToken;
                authenticationResult.Errors = new[] { "The password reset link has expired. Please use Forgot Password." };

                int userProfileId = _dbContext.UserQuestionAnswers.Single(u => u.UserQuestionAnswerId == userQuestionAnswerId).UserProfileId;
                foreach (var orgRegProgDto in _organizationService.GetUserOrganizations(userProfileId))
                {
                    var userDto = _userService.GetUserProfileById(userProfileId);
                    _crommerAuditLogService.SimpleLog(CromerrEvent.ForgotPassword_PasswordResetExpired, orgRegProgDto, userDto);
                }

                return authenticationResult;
            }

            return await ResetPasswordAsync(userQuestionAnswerId, answer, failedCount, newPassword);
        }

        public async Task<AuthenticationResultDto> ResetPasswordAsync(int userQuestionAnswerId,
            string answer, int failedCount, string newPassword)
        {
            int userProfileId = _dbContext.UserQuestionAnswers.Single(u => u.UserQuestionAnswerId == userQuestionAnswerId).UserProfileId;
            string passwordHash = _passwordHasher.HashPassword(newPassword);
            string correctSavedHashedAnswer = _dbContext.UserQuestionAnswers.Single(a => a.UserQuestionAnswerId == userQuestionAnswerId).Content;
            var authorityOrganizationIds = GetUserAuthorityOrganizationIds(userProfileId);
            var organizationSettings = _settingService.GetOrganizationSettingsByIds(authorityOrganizationIds).SelectMany(i => i.Settings).ToList();

            var authenticationResult = new AuthenticationResultDto();

            //KBQ ANSWERS ARE CASE-INSENSITIVE; PERSISTED AS ALL LOWER CASE
            if (_userManager.PasswordHasher.VerifyHashedPassword(correctSavedHashedAnswer, answer.Trim().ToLower()) != PasswordVerificationResult.Success)
            {
                //Check hashed answer (5.3.a)

                authenticationResult.Success = false;

                //3rd incorrect attempt (5.3.b) => lockout
                int maxAnswerAttempts = Convert.ToInt32(_settingService.GetOrganizationSettingValueByUserId(userProfileId, SettingType.FailedKBQAttemptMaxCount, true, null));
                if ((failedCount + 1) >= maxAnswerAttempts) // from web.config
                {
                    _userService.LockUnlockUserAccount(userProfileId: userProfileId, isAttemptingLock: true, reason: AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringPasswordReset);
                    //Get all associated authorities
                    var userOrgs = _organizationService.GetUserRegulators(userProfileId).ToList();
                    authenticationResult.RegulatoryList = userOrgs;

                    string errorString = "<div class=\"table - responsive\">";
                    errorString += "<table class=\"table no-margin\">";
                    errorString += "<tbody>";

                    foreach (var org in userOrgs)
                    {
                        errorString += "<tr><td>" + org.EmailContactInfoName + "</td><td>" + org.EmailContactInfoEmailAddress + "</td><td>" + org.EmailContactInfoPhone + " </td></tr>";
                    }

                    errorString += "</tbody>";
                    errorString += "</table>";
                    errorString += "</div>";
                    errorString += "</table>";

                    authenticationResult.Result = AuthenticationResult.UserIsLocked;
                    authenticationResult.Errors = new[] { errorString };

                }
                else
                {
                    authenticationResult.Result = AuthenticationResult.IncorrectAnswerToQuestion;
                    authenticationResult.Errors = new[] { "The answer is incorrect.  Please try again." };
                }
            }
            else if (!IsValidPasswordCheckInHistory(newPassword, userProfileId, organizationSettings))
            {
                //Password used before (6.a)
                var numberOfPasswordsInHistory = GetStrictestPasswordHistoryCounts(organizationSettings, null);

                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.CanNotUseOldPassword;
                authenticationResult.Errors = new[] { $"You cannot use the last {numberOfPasswordsInHistory} passwords." };
            }
            else
            {
                //create history record
                UserPasswordHistory history = _dbContext.UserPasswordHistories.Create();
                history.UserProfileId = userProfileId;
                history.PasswordHash = passwordHash;
                history.LastModificationDateTimeUtc = DateTimeOffset.Now;
                _dbContext.UserPasswordHistories.Add(history);
                _dbContext.SaveChanges();

                //Set new password
                _userService.SetHashedPassword(userProfileId, passwordHash);

                //Unlock user
                string userOwinId = _dbContext.Users.Single(u => u.UserProfileId == userProfileId).Id;
                await _userManager.UnlockUserAccount(userOwinId);

                authenticationResult.Success = true;
                authenticationResult.Result = AuthenticationResult.Success;

                foreach (var orgRegProgDto in _organizationService.GetUserOrganizations(userProfileId))
                {
                    var userDto = _userService.GetUserProfileById(userProfileId);
                    _crommerAuditLogService.SimpleLog(CromerrEvent.ForgotPassword_Success, orgRegProgDto, userDto);
                }
            }

            return authenticationResult;
        }

        /// <summary>
        /// To request a password reset. This will do follow:
        /// 1. generate a reset password token
        /// 2. send a reset password email
        /// 3. log to system 
        /// </summary>
        /// <param name="username">The user name </param>
        /// <returns></returns>
        public async Task<AuthenticationResultDto> RequestResetPassword(string username)
        {
            AuthenticationResultDto authenticationResult = new AuthenticationResultDto();

            var user = _dbContext.Users.SingleOrDefault(u => u.UserName == username);
            if (user == null)
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.UserNotFound;
                authenticationResult.Errors = new[] { "UserNotFound" };
            }
            else if (!await _userManager.IsEmailConfirmedAsync(user.Id))
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.EmailIsNotConfirmed;
                authenticationResult.Errors = new[] { "EmailIsNotConfirmed" };
            }
            else
            {
                authenticationResult.Success = true;
                authenticationResult.Result = AuthenticationResult.Success;
                SendResetPasswordConfirmationEmail(user);
            }

            return authenticationResult;
        }

        public async Task<AuthenticationResultDto> RequestUsernameEmail(string email)
        {
            AuthenticationResultDto authenticationResult = new AuthenticationResultDto();

            var user = _userManager.FindByEmail(email);
            if (user == null)
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.UserNotFound;
                authenticationResult.Errors = new[] { "UserNotFound" };
            }
            else if (!await _userManager.IsEmailConfirmedAsync(user.Id))
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.EmailIsNotConfirmed;
                authenticationResult.Errors = new[] { "EmailIsNotConfirmed" };
            }
            else
            {
                authenticationResult.Success = true;
                authenticationResult.Result = AuthenticationResult.Success;
                SendRequestUsernameEmail(user);
            }

            return authenticationResult;
        }

        /// <summary>
        /// Sign in by user name and password.  "isPersistent" indicates to keep the cookie or now. 
        /// </summary>
        /// <param name="userName">The user name used when sign in</param>
        /// <param name="password">The password used when sign in</param>
        /// <param name="isPersistent">The flag indicates persistent the sign or not</param>
        /// <returns></returns>
        public Task<SignInResultDto> SignInByUserName(string userName, string password, bool isPersistent)
        {
            _logger.Info(message: "SignInByUserName. userName={0}", argument: userName);

            SignInResultDto signInResultDto = new SignInResultDto();

            var applicationUser = _userManager.FindByName(userName);
            if (applicationUser == null)
            {
                signInResultDto.AutehticationResult = AuthenticationResult.UserNotFound;
                return Task.FromResult(signInResultDto);
            }

            signInResultDto.RegulatoryList = _organizationService.GetUserRegulators(applicationUser.UserProfileId).ToList();

            // clear claims from db if there are   
            ClearClaims(applicationUser.Id);
            applicationUser.Claims.Clear();

            var userId = applicationUser.UserProfileId;
            var authorityOrganizationIds = GetUserAuthorityOrganizationIds(userId);

            var organizationSettings = _settingService.GetOrganizationSettingsByIds(authorityOrganizationIds).SelectMany(i => i.Settings).ToList();

            SetPasswordPolicy(organizationSettings);

            _signInManager.UserManager = _userManager;

            bool isUserPasswordLockedOutBeforeSignInAttempt = _userManager.IsLockedOut(applicationUser.Id);

            var signInStatus = _signInManager.PasswordSignIn(userName, password, isPersistent, shouldLockout: true);

            if (signInStatus == SignInStatus.Success)
            {
                var claims = GetUserIdentity(applicationUser);

                // Save claim
                SaveClaims(applicationUser.Id, claims);

                var identity = new ClaimsIdentity(_httpContext.Current().User.Identity);
                identity.AddClaims(claims);
                _authenticationManager.AuthenticationResponseGrant = new AuthenticationResponseGrant
                    (identity, new AuthenticationProperties { IsPersistent = isPersistent });

                _authenticationManager.SignOut();

                if (applicationUser.IsAccountResetRequired)
                {
                    LogProhibitedSignInActivityToCromerr(applicationUser, CromerrEvent.Login_AccountResetRequired);

                    signInResultDto.AutehticationResult = AuthenticationResult.AccountResetRequired;
                    return Task.FromResult(signInResultDto);
                }

                // UC-29, 2.c
                // Check if the user is in 'password lock' status
                if (_userManager.IsLockedOut(applicationUser.Id))
                {
                    if (isUserPasswordLockedOutBeforeSignInAttempt)
                        LogProhibitedSignInActivityToCromerr(applicationUser, CromerrEvent.Login_AccountLocked);
                    else
                        LogProhibitedSignInActivityToCromerr(applicationUser, CromerrEvent.Login_PasswordLockout);

                    signInResultDto.AutehticationResult = AuthenticationResult.PasswordLockedOut;
                    return Task.FromResult(signInResultDto);
                }

                // UC-29, 3.a
                // Check if the user has been locked "Account Lockout"  by an authority
                if (applicationUser.IsAccountLocked)
                {
                    LogProhibitedSignInActivityToCromerr(applicationUser, CromerrEvent.Login_AccountLocked);

                    signInResultDto.AutehticationResult = AuthenticationResult.UserIsLocked;
                    return Task.FromResult(signInResultDto);
                }

                // UC-29, 4.a, 5.a, 6.a
                if (!ValidateUserAccess(applicationUser, signInResultDto))
                {
                    return Task.FromResult(signInResultDto);
                }

                // UC-29 7.a
                // Check if user's password is expired or not   
                if (IsUserPasswordExpired(userId, organizationSettings))
                {
                    // Put user profile Id into session, to request user change their password. 
                    //_sessionCache.SetValue(CacheKey.UserProfileId, applicationUser.UserProfileId);
                    signInResultDto.OwinUserId = applicationUser.Id;
                    signInResultDto.UserProfileId = applicationUser.UserProfileId;
                    signInResultDto.AutehticationResult = AuthenticationResult.PasswordExpired;
                    return Task.FromResult(signInResultDto);
                }

                signInResultDto.AutehticationResult = AuthenticationResult.Success;

                _sessionCache.SetValue(CacheKey.UserProfileId, applicationUser.UserProfileId);
                _sessionCache.SetValue(CacheKey.OwinUserId, applicationUser.Id);

                _authenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
                _signInManager.PasswordSignIn(userName, password, isPersistent, shouldLockout: true);
            }
            else if (signInStatus == SignInStatus.Failure)
            {
                signInResultDto.AutehticationResult = AuthenticationResult.InvalidUserNameOrPassword;
            }
            else if (signInStatus == SignInStatus.LockedOut)
            {
                signInResultDto.AutehticationResult = AuthenticationResult.PasswordLockedOut;

                //Log to Cromerr
                if (isUserPasswordLockedOutBeforeSignInAttempt)
                    LogProhibitedSignInActivityToCromerr(applicationUser, CromerrEvent.Login_AccountLocked);
                else
                    LogProhibitedSignInActivityToCromerr(applicationUser, CromerrEvent.Login_PasswordLockout);


            }

            _logger.Info(message: "SignInByUserName. signInStatus={0}", argument: signInStatus.ToString());
            return Task.FromResult(signInResultDto);
        }

        public PasswordAndKbqValidationResult ValidatePasswordAndKbq(string password, int userQuestionAnswerId, string kbqAnswer, int failedPasswordCount, int failedKbqCount, ReportOperation reportOperation, int? reportPackageId = null)
        {
            _logger.Info($"Enter AuthenticationService.PasswordAndKbqValidationResult");

            var userProfileId = int.Parse(s: _httpContext.GetClaimValue(claimType: CacheKey.UserProfileId));
            var orgRegProgramId = int.Parse(s: _httpContext.GetClaimValue(claimType: CacheKey.OrganizationRegulatoryProgramId));
            var authority = _settingService.GetAuthority(orgRegProgramId: orgRegProgramId);
            var authoritySettings = _settingService.GetOrganizationSettingsById(organizationId: authority.OrganizationId).Settings;
            var failedPasswordAttemptMaxCount = ValueParser.TryParseInt(authoritySettings.Where(s => s.TemplateName.Equals(obj: SettingType.FailedPasswordAttemptMaxCount)).Select(s => s.Value).First(), 3);
            var failedKbqAttemptMaxCount = ValueParser.TryParseInt(authoritySettings.Where(s => s.TemplateName.Equals(obj: SettingType.FailedKBQAttemptMaxCount)).Select(s => s.Value).First(), 3);
            
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
                ThrowUserStatusRuleValiation("User is locked");
            }

            if (userProfile.IsAccountResetRequired)
            {
                ThrowUserStatusRuleValiation("User is required to reset account");
            }

            // Check to see if password matches. 
            if (!IsValidPassword(userProfile.PasswordHash, password))
            {
                if (failedPasswordAttemptMaxCount <= failedPasswordCount + 1)
                {
                    SignOff();  

                    if(reportOperation == ReportOperation.SignAndSubmit)
                    {
                        _userService.LockUnlockUserAccount(userProfileId, true, AccountLockEvent.ExceededPasswordMaxAttemptsDuringSignatureCeremony, reportPackageId);
                    }
                    else if(reportOperation == ReportOperation.Repudiation)
                    {
                        _userService.LockUnlockUserAccount(userProfileId, true, AccountLockEvent.ExceededPasswordMaxAttemptsDuringRepudiationCeremony, reportPackageId);
                    }

                    return PasswordAndKbqValidationResult.UserLocked_Password;
                }

                return PasswordAndKbqValidationResult.InvalidPassword;
            }

            // Check to see if KBQ answer matches
            if (!_questionAnswerService.ConfirmCorrectAnswer(userQuestionAnswerId, kbqAnswer.ToLower()))
            {
                if (failedKbqAttemptMaxCount <= failedKbqCount + 1)
                {
                    SignOff(); 
                    if(reportOperation == ReportOperation.SignAndSubmit)
                    {
                        _userService.LockUnlockUserAccount(userProfileId, true, AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringSignatureCeremony, reportPackageId);
                    }
                    else if(reportOperation == ReportOperation.Repudiation)
                    {
                        _userService.LockUnlockUserAccount(userProfileId, true, AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringRepudiationCeremony, reportPackageId);
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
                ThrowUserStatusRuleValiation("User does not have access to current program");
            }
            else if (!programUser.IsEnabled)
            {
                ThrowUserStatusRuleValiation("User is disabled");
            }

            return PasswordAndKbqValidationResult.Success;
        }

        private void ThrowUserStatusRuleValiation(string message)
        {
            SignOff();
            List<RuleViolation> validationIssues = new List<RuleViolation>();
            validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
            throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
        } 

        /// <summary>
        /// Log to Cromerr events: 
        /// 1) attempting to log into an account that is password locked
        /// 2) attempting to log into an account that is manually locked by Authority
        /// 3) getting locked out due to too many password attempts
        /// 4) attempting to log into an account that has been reset
        /// </summary>
        /// <param name="user">Actor attempting the signin activity</param>
        /// <param name="cromerrEvent">Used to determine if user just got locked out OR they were previously locked out, or reset</param>
        private void LogProhibitedSignInActivityToCromerr(UserProfile user, CromerrEvent cromerrEvent)
        {
            //Need to log for all associated regulatory program orgs
            var orgRegProgUsers = _dbContext.OrganizationRegulatoryProgramUsers
                                    .Include("OrganizationRegulatoryProgram")
                                    .Where(u => u.UserProfileId == user.UserProfileId).ToList();
            foreach (var orgRegProgUser in orgRegProgUsers)
            {
                var orgRegProgram = orgRegProgUser.OrganizationRegulatoryProgram;

                var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
                cromerrAuditLogEntryDto.RegulatoryProgramId = orgRegProgram.RegulatoryProgramId;
                cromerrAuditLogEntryDto.OrganizationId = orgRegProgram.OrganizationId;
                cromerrAuditLogEntryDto.RegulatorOrganizationId = orgRegProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                cromerrAuditLogEntryDto.UserProfileId = user.UserProfileId;
                cromerrAuditLogEntryDto.UserName = user.UserName;
                cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
                cromerrAuditLogEntryDto.UserLastName = user.LastName;
                cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
                cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
                var contentReplacements = new Dictionary<string, string>();
                contentReplacements.Add("firstName", user.FirstName);
                contentReplacements.Add("lastName", user.LastName);
                contentReplacements.Add("userName", user.UserName);
                contentReplacements.Add("emailAddress", user.Email);

                _crommerAuditLogService.Log(cromerrEvent, cromerrAuditLogEntryDto, contentReplacements);

            }
        }

        // Validate user access for UC-29(4.a, 5.a, 6.a)
        private bool ValidateUserAccess(UserProfile userProfile, SignInResultDto signInResultDto)
        {
            var orpus = _programService.GetUserRegulatoryPrograms(userProfile.Email).ToList();
            if (orpus.Any())
            {
                // User at least has one approved program
                if(orpus.Any(i=>i.IsRegistrationApproved && i.IsRegistrationDenied == false && i.IsEnabled && i.OrganizationRegulatoryProgramDto.IsEnabled))
                {
                    return true;
                }
                
                // UC-29 4.a
                // System confirms user has status “Registration Pending” (and no access to any other portal where registration is not pending or portal is not disabled)
                if (orpus.Any(i => i.IsRegistrationApproved == false && i.IsRegistrationDenied == false && i.IsEnabled && i.OrganizationRegulatoryProgramDto.IsEnabled))
                {
                    LogToCromerrThisEvent(orpus, CromerrEvent.Login_RegistrationPending);
                    signInResultDto.AutehticationResult = AuthenticationResult.RegistrationApprovalPending;
                    return false;
                }

                // UC-29 5.a, User account is disabled for all industry, or authority or application administrator
                // If the user is disabled for all programs
                if (orpus.All(u => u.IsEnabled == false) &&  //--- user is disabled for all industry and authority 
                    userProfile.IsInternalAccount == false   //--- user is disabled for Application administrator.
                )
                {
                    LogToCromerrThisEvent(orpus, CromerrEvent.Login_UserDisabled);
                    signInResultDto.AutehticationResult = AuthenticationResult.UserIsDisabled;
                    return false;
                }

                // 6.a determine user doesn't have access to any enabled industry or authority 
                if (orpus.Any(i => i.IsRegistrationApproved &&
                                   i.IsEnabled && i.OrganizationRegulatoryProgramDto.IsEnabled) == false)
                {
                    LogToCromerrThisEvent(orpus, CromerrEvent.Login_NoAssociation);
                    signInResultDto.AutehticationResult = AuthenticationResult.AccountIsNotAssociated;
                    return false;
                }

                // 6.b returned in the intermediate page call from here. 
            }
            else
            {
                // If user doesn't have any program, return below message
                LogToCromerrThisEvent(orpus, CromerrEvent.Login_NoAssociation);
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
                var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
                cromerrAuditLogEntryDto.RegulatoryProgramId = programUser.OrganizationRegulatoryProgramDto.RegulatoryProgramId;
                cromerrAuditLogEntryDto.OrganizationId = programUser.OrganizationRegulatoryProgramDto.OrganizationId;
                cromerrAuditLogEntryDto.RegulatorOrganizationId = programUser.OrganizationRegulatoryProgramDto.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                cromerrAuditLogEntryDto.UserProfileId = programUser.UserProfileId;
                cromerrAuditLogEntryDto.UserName = user.UserName;
                cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
                cromerrAuditLogEntryDto.UserLastName = user.LastName;
                cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
                cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
                var contentReplacements = new Dictionary<string, string>();
                contentReplacements.Add("firstName", user.FirstName);
                contentReplacements.Add("lastName", user.LastName);
                contentReplacements.Add("userName", user.UserName);
                contentReplacements.Add("emailAddress", user.Email);

                _crommerAuditLogService.Log(cromerrEvent, cromerrAuditLogEntryDto, contentReplacements);

            }
        }

        public string GetClaimsValue(string claimType)
        {
            string claimsValue = null;

            var claims = GetClaims();

            //return claims?.ToList();
            if (claims != null)
            {
                var claim = claims.FirstOrDefault(c => c.Type == claimType);
                if (claim != null)
                    claimsValue = claim.Value;
            }

            return claimsValue;
        }

        public void SetClaimsForOrgRegProgramSelection(int orgRegProgId)
        {
            //We already have: UserId, UserProfileId, UserFullName,
            var userProfileId = Convert.ToInt32(GetClaimsValue(CacheKey.UserProfileId));

            //Now we set UserRole, OrganizationRegulatoryProgramId, OrganizationName, OrgRegProgUserId, PortalName
            //UserRole <=> PermissionGroup.Name
            //PortalName <=> OrganizationType.Name

            var orgRegProgUser = _dbContext.OrganizationRegulatoryProgramUsers
                .Include("OrganizationRegulatoryProgram.RegulatoryProgram")
                .SingleOrDefault(o => o.UserProfileId == userProfileId && o.OrganizationRegulatoryProgramId == orgRegProgId);
            if (orgRegProgUser == null)
                throw new Exception(string.Format("ERROR: UserProfileId={0} does not have access to Organization Regulatory Program={1}.", userProfileId, orgRegProgId));

            var permissionGroupId = orgRegProgUser.PermissionGroupId;
            var orgRegProgUserId = orgRegProgUser.OrganizationRegulatoryProgramUserId;
            var regProgramName = orgRegProgUser.OrganizationRegulatoryProgram.RegulatoryProgram.Name;
            var userRole = _dbContext.PermissionGroups.Single(p => p.PermissionGroupId == permissionGroupId).Name;
            var organization = _dbContext.OrganizationRegulatoryPrograms.Include("Organization.OrganizationType")
                .Single(o => o.OrganizationRegulatoryProgramId == orgRegProgId).Organization;
            var organizationName = organization.Name;
            var organizationId = organization.OrganizationId;
            var portalName = organization.OrganizationType.Name;

            var claims = new Dictionary<string, string>();
            claims.Add(CacheKey.UserRole, userRole);
            claims.Add(CacheKey.RegulatoryProgramName, regProgramName);
            claims.Add(CacheKey.OrganizationRegulatoryProgramUserId, orgRegProgUserId.ToString());
            claims.Add(CacheKey.OrganizationRegulatoryProgramId, orgRegProgId.ToString());
            claims.Add(CacheKey.OrganizationName, organizationName);
            claims.Add(CacheKey.OrganizationId, organizationId.ToString());
            claims.Add(CacheKey.PortalName, portalName);

            SetCurrentUserClaims(claims);

            //Log to Cromerr

            var programUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(orgRegProgUser);
            var user = _userService.GetUserProfileById(programUserDto.UserProfileId);

            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
            cromerrAuditLogEntryDto.RegulatoryProgramId = orgRegProgUser.OrganizationRegulatoryProgram.RegulatoryProgramId;
            cromerrAuditLogEntryDto.OrganizationId = orgRegProgUser.OrganizationRegulatoryProgram.OrganizationId;
            cromerrAuditLogEntryDto.RegulatorOrganizationId = orgRegProgUser.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = orgRegProgUser.UserProfileId;
            cromerrAuditLogEntryDto.UserName = user.UserName;
            cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
            cromerrAuditLogEntryDto.UserLastName = user.LastName;
            cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
            cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("organizationName", programUserDto.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName);
            contentReplacements.Add("firstName", user.FirstName);
            contentReplacements.Add("lastName", user.LastName);
            contentReplacements.Add("userName", user.UserName);
            contentReplacements.Add("emailAddress", user.Email);

            _crommerAuditLogService.Log(CromerrEvent.Login_Success, cromerrAuditLogEntryDto, contentReplacements);

        }

        public void SignOff()
        {
            var owinUserId = _sessionCache.GetValue(CacheKey.OwinUserId) as string;
            ClearClaims(owinUserId);
            _sessionCache.Clear();
            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }

        #region private section

        // Check if password is in one of the last # passwords stores in UserPasswordHistory table
        // Return false means the new password is not validate because it has been used in the last #number of times
        // Return true means the new password is validate to use
        private bool IsValidPasswordCheckInHistory(string password, int userProfileId, IEnumerable<SettingDto> organizationSettings)
        {
            var numberOfPasswordsInHistory = GetStrictestPasswordHistoryCounts(organizationSettings, null);

            var lastNumberPasswordInHistory = _dbContext.UserPasswordHistories
                .Where(i => i.UserProfileId == userProfileId)
                .OrderByDescending(i => i.LastModificationDateTimeUtc).Take(numberOfPasswordsInHistory)
                .ToList();

            if (lastNumberPasswordInHistory.Any(i => IsValidPassword(i.PasswordHash, password)))
            {
                return false;
            }

            return true;
        }

        private bool IsValidPassword(string passwordHash, string password)
        {
            return _userManager.PasswordHasher.VerifyHashedPassword(passwordHash, password) == PasswordVerificationResult.Success;
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
            var passwordExpiredDays = GetStrictestLengthPasswordExpiredDays(organizationSettings, null);
            if (DateTimeOffset.UtcNow > lastestUserPassword.LastModificationDateTimeUtc.AddDays(passwordExpiredDays))
            {
                return true;
            }

            return false;
        }

        private void ClearClaims(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return;
            var user = _userManager.FindById(userId);
            if (user == null)
            {
                return;
            }

            var claims = _userManager.GetClaims(userId).ToList();
            foreach (var claim in claims)
            {
                _userManager.RemoveClaim(userId, claim);
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

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userProfile.Id));
            claims.Add(new Claim(CacheKey.OwinUserId, userProfile.Id));
            claims.Add(new Claim(CacheKey.UserProfileId, userProfile.UserProfileId.ToString()));
            claims.Add(new Claim(CacheKey.FirstName, userProfile.FirstName));
            claims.Add(new Claim(CacheKey.LastName, userProfile.LastName));
            claims.Add(new Claim(CacheKey.UserName, userProfile.UserName));
            claims.Add(new Claim(CacheKey.Email, userProfile.Email));
            claims.Add(new Claim(CacheKey.SessionId, _httpContext.Current().Session.SessionID));

            return claims;
        }

        private void SaveClaims(string userId, IList<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

            var authProperties = new AuthenticationProperties();

            var existingClaims = _userManager.GetClaims(userId);

            foreach (var claim in claims)
            {
                if (!IsHavingClaim(existingClaims, claim))
                {
                    _userManager.AddClaim(userId, claim);
                }
            }

            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            _authenticationManager.SignIn(authProperties, identity);
        }

        private bool IsHavingClaim(IList<Claim> claims, Claim claim)
        {
            foreach (var cl in claims)
            {
                if (cl.Type.Equals(claim.Type)) return true;
            }

            return false;
        }

        private void SetPasswordPolicy(IEnumerable<SettingDto> organizationSettings)
        {
            // Password policy is only defined on system global level 
            // Failed trial times is defined on organization level 

            // If one setting has multiple definitions, choose the strictest one
            _userManager.UserLockoutEnabledByDefault = true;
            _userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromHours(_settingService.PasswordLockoutHours());

            _userManager.MaxFailedAccessAttemptsBeforeLockout = MaxFailedPasswordAttempts(organizationSettings, orgTypeName: null);
        }


        #region organization setting;

        private int GetSettingIntValue(SettingType settingType, IEnumerable<SettingDto> organizationSettingsValue, OrganizationTypeName? orgTypeName, bool isMax = true)
        {
            var defaultValueStr = _settingService.GetSettingTemplateValue(settingType, orgTypeName);
            var defaultValue = ValueParser.TryParseInt(defaultValueStr, 0);
            var organizationSettings = organizationSettingsValue.ToList();
            if (organizationSettings.Any())
            {
                if (orgTypeName != null)
                {
                    defaultValue = isMax
                        ? organizationSettings
                            .Where(i => i.TemplateName == settingType && i.OrgTypeName == orgTypeName).Max(i => ValueParser.TryParseInt(i.Value, defaultValue))
                        : organizationSettings.Where(i => i.TemplateName == settingType && i.OrgTypeName == orgTypeName)
                            .Min(i => ValueParser.TryParseInt(i.Value, defaultValue));
                }
                else
                {
                    defaultValue = isMax
                        ? organizationSettings
                            .Where(i => i.TemplateName == settingType).Max(i => ValueParser.TryParseInt(i.Value, defaultValue))
                        : organizationSettings.Where(i => i.TemplateName == settingType)
                            .Min(i => ValueParser.TryParseInt(i.Value, defaultValue));

                }
            }

            return defaultValue;
        }

        private int GetStrictestPasswordHistoryCounts(IEnumerable<SettingDto> organizationSettings, OrganizationTypeName? orgTypeName)
        {
            return GetSettingIntValue(SettingType.PasswordHistoryMaxCount, organizationSettings, orgTypeName, isMax: false);
        }

        private int GetStrictestLengthPasswordExpiredDays(IEnumerable<SettingDto> organizationSettings, OrganizationTypeName? orgTypeName)
        {
            return GetSettingIntValue(SettingType.PasswordChangeRequiredDays, organizationSettings, orgTypeName, isMax: false);
        }

        private int MaxFailedPasswordAttempts(IEnumerable<SettingDto> organizationSettings, OrganizationTypeName? orgTypeName)
        {
            return GetSettingIntValue(SettingType.FailedPasswordAttemptMaxCount, organizationSettings, orgTypeName, isMax: false);
        }

        #endregion 

        private IEnumerable<int> GetUserAuthorityOrganizationIds(int userid)
        {
            var authorityOrgIds = new List<int>();
            var orgRegPrograms = _organizationService.GetUserOrganizations(userid);
            foreach (var orgRegProgram in orgRegPrograms)
            {
                authorityOrgIds.Add(orgRegProgram.RegulatorOrganizationId ?? orgRegProgram.OrganizationId);
            }

            return authorityOrgIds;
        }

        private void SendResetPasswordConfirmationEmail(UserProfile userProfile)
        {
            //var token = _userManager.GeneratePasswordResetTokenAsync(userProfile.Id).Result; 
            var token = Guid.NewGuid().ToString();

            string baseUrl = _httpContext.GetRequestBaseUrl();
            string link = baseUrl + "Account/ResetPassword/?token=" + token;

            string supportPhoneNumber = _globalSettings[SystemSettingType.SupportPhoneNumber];
            string supportEmail = _globalSettings[SystemSettingType.SupportEmailAddress];

            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("link", link);
            contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
            contentReplacements.Add("supportEmail", supportEmail);

            _requestCache.SetValue(CacheKey.Token, token);
            _emailService.SendEmail(new[] { userProfile.Email }, EmailType.ForgotPassword_ForgotPassword, contentReplacements);
        }



        private void SendRequestUsernameEmail(UserProfile userProfile)
        {
            string baseUrl = _httpContext.GetRequestBaseUrl();
            string link = baseUrl + "Account/SignIn";

            string supportPhoneNumber = _globalSettings[SystemSettingType.SupportPhoneNumber];
            string supportEmail = _globalSettings[SystemSettingType.SupportEmailAddress];

            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("userName", userProfile.UserName);
            contentReplacements.Add("link", link);
            contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
            contentReplacements.Add("supportEmail", supportEmail);

            _emailService.SendEmail(new[] { userProfile.Email }, EmailType.ForgotUserName_ForgotUserName, contentReplacements);
        }

        public RegistrationResult ValidateRegistrationData(UserDto userProfile, IEnumerable<AnswerDto> securityQuestions, IEnumerable<AnswerDto> kbqQuestions)
        {
            if (userProfile.AgreeTermsAndConditions == false)
            {
                return RegistrationResult.NotAgreedTermsAndConditions;
            }

            // To verify user's password  
            var passwordRequiredLengthFromWebConfig = ValueParser.TryParseInt(ConfigurationManager.AppSettings["PasswordRequiredLength"], defaultValue: 8);
            var passwordRequiredLength = ValueParser.TryParseInt(_globalSettings[SystemSettingType.PasswordRequiredLength], defaultValue: passwordRequiredLengthFromWebConfig);
            var passwordRequiredMaxLength = ValueParser.TryParseInt(_globalSettings[SystemSettingType.PasswordRequiredMaxLength], defaultValue: 16);

            if (userProfile.Password.Length < passwordRequiredLength || userProfile.Password.Length > passwordRequiredMaxLength)
            {
                return RegistrationResult.BadPassword;
            }

            var validPassword = _userManager.PasswordValidator.ValidateAsync(userProfile.Password).Result;
            if (validPassword.Succeeded == false)
            {
                return RegistrationResult.BadPassword;
            }

            return _userService.ValidateRegistrationUserData(userProfile, securityQuestions, kbqQuestions);
        }

        public RegistrationResult[] KbqValidation(UserDto userProfile, IEnumerable<AnswerDto> kbqQuestions)
        {
            return null;
        }
        public RegistrationResult[] SecurityValidation(UserDto userProfile, IEnumerable<AnswerDto> kbqQuestions)
        { 
            return null;
        } 

        public void UpdateClaim(string key, string value)
        {
            var currentClaims = GetClaims();
            if (currentClaims != null)
            {
                var claim = currentClaims.FirstOrDefault(i => i.Type == key);
                if (claim != null)
                {
                    currentClaims.Remove(claim);
                }

                currentClaims.Add(new Claim(key, value));
            }

            var owinUserId = GetClaimsValue(CacheKey.OwinUserId);

            ClearClaims(owinUserId);
            SaveClaims(owinUserId, currentClaims);
        }

        #endregion
    }
}
