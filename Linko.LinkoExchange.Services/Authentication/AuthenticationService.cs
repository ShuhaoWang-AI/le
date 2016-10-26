using System;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.AuditLog;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services.Cache;
using System.Configuration;

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
        private readonly TokenGenerator _tokenGenerator = new TokenGenerator();
        private readonly IAuditLogService _auditLogService = new CrommerAuditLogService();
        private readonly IPasswordHasher _passwordHasher;

        private readonly IDictionary<SettingType, string> _globalSettings; 

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
            IUserService userService,
            ISessionCache sessionCache,
            IPasswordHasher passwordHasher
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
            if( userService == null) throw new ArgumentNullException("userService");
            if (sessionCache == null) throw new ArgumentNullException("sessionCache");

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
            _passwordHasher = passwordHasher;
        }

        public IList<Claim> GetClaims()
        {
            var claims = _sessionCache.GetValue(CacheKey.OwinClaims)  as IEnumerable<Claim>;
            if (claims == null)
            {
                var userId = _sessionCache.GetValue(CacheKey.UserProfileId) as string;
                claims = string.IsNullOrWhiteSpace(userId) ? null : _userManager.GetClaims(userId);
                _sessionCache.SetValue(CacheKey.OwinClaims, claims);
            }
            return claims?.ToList();
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
                var userId = _sessionCache.GetValue(CacheKey.OwinUserId) as string;
                var itor = claims.GetEnumerator();

                while (itor.MoveNext())
                {
                    currentClaims.Add(new Claim(itor.Current.Key, itor.Current.Value)); 
                }

                ClearClaims(userId);
                SaveClaims(userId, currentClaims);
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

                var programIds = GetUserProgramIds(applicationUser.UserProfileId);
                var organizationIds = GetUserOrganizationIds(applicationUser.UserProfileId);

                var organizationSettings = _settingService.GetOrganizationSettingsByIds(organizationIds).SelectMany(i => i.Settings).ToList();
                var programSettings = _settingService.GetProgramSettingsByIds(programIds).SelectMany(i => i.Settings).ToList();

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
                if (IsValidPasswordCheckInHistory(applicationUser.PasswordHash, applicationUser.UserProfileId, organizationSettings))
                {
                    authenticationResult.Success = false;
                    authenticationResult.Result = AuthenticationResult.CanNotUseOldPassword;
                    authenticationResult.Errors = new string[] { "Can not use old password." };
                    return Task.FromResult(authenticationResult);
                }
                 
                _userManager.RemovePassword(userId);
                _userManager.AddPassword(userId, newPassword);
            }
            catch (Exception ex)
            {
                authenticationResult.Success = false;
                var errors = new List<string> { ex.Message };
                authenticationResult.Errors = errors;
            }
            return Task.FromResult(authenticationResult);
        }

        /// <summary>
        /// Create a new user for registration
        /// </summary>
        /// <param name="userInfo"></param>
        /// <param name="registrationToken"></param>
        /// <returns></returns>
        public Task<AuthenticationResultDto> CreateUserAsync(UserDto userInfo, string registrationToken)
        {
            var authenticationResult = new AuthenticationResultDto();
            
            var invitationDto = _invitationService.GetInvitation(registrationToken);
    
            if (invitationDto == null)
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.InvalidateRegistrationToken;
                return Task.FromResult(authenticationResult);
            }
            
//            // TODO  check token is expired or not? from organization settings
            var invitationRecipientProgram =
                _programService.GetOrganizationRegulatoryProgram(invitationDto.RecipientOrganizationRegulatoryProgramId);
            var inivitationRecipintOrganizationSettings =
                _settingService.GetOrganizationSettingsById(invitationRecipientProgram.OrganizationId);

            var invitationExpirationHours = ValueParser.TryParseInt(inivitationRecipintOrganizationSettings
                .Settings.Single(i => i.Type == SettingType.InvitationExpiryHours).Value, 0);

            if (DateTime.UtcNow > invitationDto.InvitationDateTimeUtc.AddHours(invitationExpirationHours))
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.ExpiredRegistrationToken;
                return Task.FromResult(authenticationResult);
            } 
 
            
            var applicationUser = AssignUser(userInfo.UserName, userInfo.Email);
            try
            {  
                // Only check the organization settings of invitation regulatory program
                //SetPasswordPolicy(inivitationRecipintOrganizationSettings.Settings);
                applicationUser.FirstName = userInfo.FirstName;
                applicationUser.LastName = userInfo.LastName;
                applicationUser.IsAccountLocked = false;
                applicationUser.IsAccountResetRequired = false;
                applicationUser.LockoutEnabled = true;
                applicationUser.EmailConfirmed = true;
                applicationUser.IsInternalAccount = false;
                applicationUser.AddressLine1 = userInfo.AddressLine1;
                applicationUser.AddressLine2 = userInfo.AddressLine2;
                applicationUser.CityName = userInfo.CityName;
                applicationUser.ZipCode = userInfo.ZipCode;
                applicationUser.IsIdentityProofed = false; 



                var result = _userManager.CreateAsync(applicationUser, userInfo.Password).Result;  
                
                if (result == IdentityResult.Success)
                {
                    // Retrieve user again to get userProfile Id. 
                    applicationUser = _userManager.FindById(applicationUser.Id);

                    // 1. Create a new row in userProfile password history table 
                    _dbContext.UserPasswordHistories.Add(new UserPasswordHistory
                    {
                         LastModificationDateTimeUtc = DateTime.UtcNow,
                         PasswordHash = applicationUser.PasswordHash,
                         UserProfileId = applicationUser.UserProfileId
                    });

                    // Save into user profile? 
                    var userProfile = _userService.GetUserProfileByEmail(applicationUser.Email);
                    // TODO more..... 
                    

                    // 2 // Create organziation regulatory program userProfile, and set the approved statue to false  
                    // TODO change user status to  Registration Pending
                    // UC-42 6
                    //_userService.UpdateOrganizationRegulatoryProgramUserApprovedStatus(applicationUser.UserProfileId, invitationDto.RecipientOrganizationRegulatoryProgramId, false);


                    // 3 TODO send email to all users who have rights to approve a registrant 
                    // UC-42 7
                    // find out who have the approve permission


                    // 4 TODO remove the invitation from table
                    // UC-42 8

                    var organizationId = 100;
                    var approvalPeople = _permissionService.GetApprovalPeople(applicationUser.UserProfileId, organizationId);
                    var sendTo = approvalPeople.Select(i => i.Email);

                    // TODO:  to determine if user is authority user or is industry user;
                    var isUserAuthorityUser = true;
                    var emailContentReplacements = new Dictionary<string, string>(); 

                    // TODO  to prepare email audit log entry information 
                     
                    var logEntry = new EmailAuditLogEntryDto();
                    logEntry.RecipientFirstName = "First Name";
                    logEntry.RecipientLastName = "last name";
                    logEntry.RecipientUserName = "Fist name, lastName";
                    logEntry.SenderFirstName = "Linko support";
                    logEntry.SenderLastName = "Linko support"; 

                    if (isUserAuthorityUser)
                    {
                        _emailService.SendEmail(sendTo, EmailType.Registration_AuthorityUserRegistrationPendingToApprovers, emailContentReplacements);
                    }
                    else
                    {  
                        _emailService.SendEmail(sendTo, EmailType.Registration_IndustryUserRegistrationPendingToApprovers, emailContentReplacements);
                    }

                    // 5 TODO logs invite email  
                    // UC-1 

                    // 6 TODO logs invite to Audit 
                    // UC-2 

                    _dbContext.SaveChangesAsync();

                    authenticationResult.Success = true;


                    SendRegistrationConfirmationEmail(applicationUser.Id, userInfo);  
                    return Task.FromResult(authenticationResult);
                }

                authenticationResult.Success = false;
                authenticationResult.Errors = result.Errors;
                return Task.FromResult(authenticationResult);
            }
            catch (Exception ex)
            {
                var errors = new List<string> {ex.Message};
                authenticationResult.Success = false;
                authenticationResult.Errors = errors;
            }

            return Task.FromResult(authenticationResult);
        }

        /// <summary>
        /// Reset password happens when user request a 'reset password', and system generates a reset password token and sends to userProfile's email
        /// And user click the link in the email to reset the password.
        /// </summary>
        /// <param name="email">User email address</param>
        /// <param name="resetPasswordToken">The reset password token</param>
        /// <param name="newPassword">The new password</param>
        /// <returns></returns>
        public async Task<AuthenticationResultDto> ResetPasswordAsync(string resetPasswordToken, int userQuestionAnswerId, 
            string answer, int attemptCount, string newPassword)
        {
            int userProfileId = Convert.ToInt32(_sessionCache.GetValue(CacheKey.UserProfileId));
            int orgRegProgUserId = Convert.ToInt32(_sessionCache.GetValue(CacheKey.OrganizationRegulatoryProgramUserId));
            int orgRegProgramId = Convert.ToInt32(_sessionCache.GetValue(CacheKey.OrganizationRegulatoryProgramId));
            string passwordHash = _passwordHasher.HashPassword(newPassword);
            string answerHash = _passwordHasher.HashPassword(answer);
            var organizationIds = GetUserOrganizationIds(userProfileId);
            var organizationSettings = _settingService.GetOrganizationSettingsByIds(organizationIds).SelectMany(i => i.Settings).ToList();
            int resetPasswordTokenValidateInterval = Convert.ToInt32(ConfigurationManager.AppSettings["ResetPasswordTokenValidateInterval"]);

            var authenticationResult = new AuthenticationResultDto();
           
            
            var emailAuditLog = _dbContext.EmailAuditLog.SingleOrDefault(e => e.Token == resetPasswordToken);
            if (emailAuditLog == null)
                throw new Exception(string.Format("ERROR: Cannot find email audit log associated with token={0}", resetPasswordToken));
            DateTimeOffset tokenCreated = emailAuditLog.SentDateTimeUtc;
            if (DateTimeOffset.Now.AddHours(-resetPasswordTokenValidateInterval) > tokenCreated)
            {
                //Check token expiry (5.1.a)

                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.ExpiredRegistrationToken;
            }
            else if (!(_dbContext.UserQuestionAnswers.Single(a => a.UserQuestionAnswerId == userQuestionAnswerId).Content == answerHash))
            {
                //Check hashed answer (5.3.a)

                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.IncorrectAnswerToQuestion;

                //3rd incorrect attempt (5.3.b) => lockout
                if (attemptCount == 3) //Which setting? Web.config?
                {
                    _userService.LockUnlockUserAccount(userProfileId, true);
                    //Get all associated authorities
                    List<string> authorityList = new List<string>();
                    var userOrgs = _organizationService.GetUserOrganizationsByOrgRegProgUserId(orgRegProgUserId);
                    foreach (var org in userOrgs)
                    {
                        if (org.OrganizationType.Name == "Authority")// Make sure get regulator industries
                        {
                            var email = _settingService.GetOrganizationSettingsById(org.OrganizationId) 
                                .ProgramSettings.Single(s => s.OrgRegProgId == orgRegProgramId)
                                .Settings.Single(s => s.Type == SettingType.SupportEmail).Value;
                            authorityList.Add(string.Format("{0} {1} {2}", org.OrganizationName, email, org.PhoneNumber));
                        }
                    }
                    authenticationResult.Errors = authorityList;

                }
            }
            else if (IsValidPasswordCheckInHistory(passwordHash, userProfileId, organizationSettings))
            {
                //Password used before (6.a)

                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.CanNotUseOldPassword;
                authenticationResult.Errors = new string[] { "Can not use old password." };
            }
            else
            {

                //Password not meet pass requirements? -- done at View level via PasswordValidator/data annotations

                _userService.SetHashedPassword(userProfileId, passwordHash);
                authenticationResult.Success = true;
            }

            return authenticationResult;

        }

        /// <summary>
        /// To request a password reset. This will do follow:
        /// 1. generate a reset password token
        /// 2. send a reset password email
        /// 3. log to system 
        /// </summary>
        /// <param name="email">The user address </param>
        /// <returns></returns>
        public async Task<AuthenticationResultDto> RequestResetPassword(string username)
        {
            AuthenticationResultDto authenticationResult = new AuthenticationResultDto();

            //var user = _userManager.FindByEmail(email);
            var user = _dbContext.Users.SingleOrDefault(u => u.UserName == username);
            if(user == null)
            {
                authenticationResult.Success = false;  
                authenticationResult.Result = AuthenticationResult.UserNotFound;
                authenticationResult.Errors = new[] {"UserNotFound"};
            } else if (!await _userManager.IsEmailConfirmedAsync(user.Id))
            {
                authenticationResult.Success = false;
                authenticationResult.Result = AuthenticationResult.EmailIsNotConfirmed;
                authenticationResult.Errors = new[] { "EmailIsNotConfirmed" };
            }
            else
            {
                SendResetPasswordConfirmationEmail(user);
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
            SignInResultDto signInResultDto = new SignInResultDto();
             
            var applicationUser = _userManager.FindByName(userName);
            if (applicationUser == null)
            {
                signInResultDto.AutehticationResult = AuthenticationResult.UserNotFound;
                return Task.FromResult(signInResultDto);
            }

            var regulatoryList = _organizationService.GetUserRegulatories(applicationUser.UserProfileId);
            signInResultDto.RegulatoryList = regulatoryList;

            // UC-29, 2.c
            // Check if the user is in 'passowrd lock' status
            if (_userManager.IsLockedOut(applicationUser.Id))
            {
                //TODO: log failed login because of Locked to Audit (UC-2) 
                signInResultDto.AutehticationResult = AuthenticationResult.PasswordLockedOut; 
                return Task.FromResult(signInResultDto);
            }

            // UC-29, 3.a
            // Check if the user has been locked "Account Lockout"  by an authority
            if (applicationUser.IsAccountLocked)
            {
                //TODO: log failed login because of Locked to Audit (UC-2) 
                signInResultDto.AutehticationResult = AuthenticationResult.UserIsLocked;
                return Task.FromResult(signInResultDto);
            }
             
            // UC-29, 4.a, 5.a, 6.a
            if (!ValidateUserAccess(applicationUser, signInResultDto))
            {
                return Task.FromResult(signInResultDto);
            }

            // clear claims from db if there are   
            ClearClaims(applicationUser.Id);
            applicationUser.Claims.Clear(); 
            var userId = applicationUser.UserProfileId; 
            var organizationIds = GetUserOrganizationIds(userId);

            var organizationSettings = _settingService.GetOrganizationSettingsByIds(organizationIds).SelectMany(i => i.Settings).ToList(); 

            // UC-29 7.a
            // Check if user's password is expired or not   
            if (IsUserPasswordExpired(userId, organizationSettings))
            {
                return Task.FromResult(signInResultDto); 
            }

            SetPasswordPolicy(organizationSettings); 

            _signInManager.UserManager = _userManager; 
            var signInStatus = _signInManager.PasswordSignInAsync(userName, password, isPersistent, true).Result;

            if (signInStatus == SignInStatus.Success)
            {
                signInResultDto.AutehticationResult = AuthenticationResult.Success;
                signInResultDto.Token = _tokenGenerator.GenToken(userName);

                _sessionCache.SetValue(CacheKey.UserProfileId, applicationUser.Id);

                //Set user's claims
                GetUserIdentity(applicationUser);  
            }
            else if (signInStatus == SignInStatus.Failure)
            {
                signInResultDto.AutehticationResult = AuthenticationResult.InvalidUserNameOrPassword;
            }

            return Task.FromResult(signInResultDto);
        }

        // Validate user access for UC-29(4.a, 5.a, 6.a)
        private bool ValidateUserAccess(UserProfile userProfile, SignInResultDto signInResultDto)
        {
            var orpus = _programService.GetUserRegulatoryPrograms(userProfile.Email);
            if (orpus != null && orpus.Any())
            {
                // UC-29 4.a
                // System confirms user has status “Registration Pending” (and no access to any other portal where registration is not pending or portal is not disabled)
                if (orpus.All(i => i.IsRegistrationApproved == false) ||
                    ! orpus.Any(i => i.IsRegistrationApproved && i.OrganizationRegulatoryProgramDto.IsEnabled))
                {
                    // TODO: Log failed login because of registration pending to Audit (UC-2)
                    signInResultDto.AutehticationResult = AuthenticationResult.RegistrationApprovalPending;
                    return false;
                }

                // UC-29 5.a, User account is disabled for all industry, or authorty or application admin
                // If the user is disabled for all programs
                if (orpus.All(u => u.IsEnabled == false) &&  //--- user is disabled for all industry and authority 
                    userProfile.IsInternalAccount == false   //--- user is diabled for Application admin.
                )
                {
                    // TODO: Log failed login because userProfile disabled to Audit (UC-2)
                    signInResultDto.AutehticationResult = AuthenticationResult.UserIsDisabled;
                    return false;
                }

                // 6.a determine user doesn't have access to any enabled industry or authority 
                if (orpus.Any(i => i.IsRegistrationApproved &&
                                   i.IsEnabled && i.OrganizationRegulatoryProgramDto.IsEnabled) == false)
                {
                    // Log failed login because no associations to Audit (UC-2) 
                    signInResultDto.AutehticationResult = AuthenticationResult.AccountIsNotAssociated;
                    return false;
                }

                // 6.b returned in the intermediate page call.... 

            }
            return true;
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

            var orgRegProgUser = _dbContext.OrganizationRegulatoryProgramUsers.Include("OrganizationRegulatoryProgram.RegulatoryProgram")
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
        }

        public void SignOff()
        {
            var userId = _sessionCache.GetValue(CacheKey.UserProfileId) as string;
            ClearClaims(userId);
            _sessionCache.Clear();
            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie); 
        }

        #region private section

        // Check if password is in one of the last # passwords stores in UserPasswordHistory table
        // Return false means the new password is not validate because it has been used in the last #number of times
        // Return true means the new password is validate to use
        private bool IsValidPasswordCheckInHistory(string passwordHash, int userProfileId, IEnumerable<SettingDto> organizationSettings)
        {
            var numberOfPasswordsInHistory = GetStrictestPasswordHistoryCounts(organizationSettings);

            var lastNumberPasswordInHistory = _dbContext.UserPasswordHistories
                .Where(i => i.UserProfileId == userProfileId)
                .OrderByDescending(i => i.LastModificationDateTimeUtc).Take(numberOfPasswordsInHistory);
            if (lastNumberPasswordInHistory.Any(i=>i.PasswordHash == passwordHash))
            {
                return false; 
            }

            return true;
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
            var passwordExpiredDays = GetStrictestLengthPasswordExpiredDays(organizationSettings);
            if (DateTime.UtcNow > lastestUserPassword.LastModificationDateTimeUtc.AddDays(passwordExpiredDays))
            {
                return true;
            }

            return false; 
        }

        private void ClearClaims(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return;

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
         
        private void GetUserIdentity(UserProfile userProfile)
        { 
            //TODO: to replace using real claims  
            // get userDto's role, organizations, programs, current organization, current program.....
             
            var claims = new List<Claim>(); 
             
            claims.Add(new Claim(CacheKey.OwinUserId, userProfile.Id)); 
            claims.Add(new Claim(CacheKey.UserProfileId, userProfile.UserProfileId.ToString()));
            claims.Add(new Claim(CacheKey.FirstName, userProfile.FirstName));
            claims.Add(new Claim(CacheKey.LastName, userProfile.LastName));
            claims.Add(new Claim(CacheKey.UserName, userProfile.UserName));
            claims.Add(new Claim(CacheKey.Email, userProfile.Email));

            SaveClaims(userProfile.Id, claims);
        }

        private void SaveClaims(string userId, IList<Claim> claims)
        {
            var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };

            foreach (var claim in claims)
            {
                _userManager.AddClaim(userId, claim);
            }

            _authenticationManager.SignIn(authProperties, identity);  
        }
        private void SetPasswordPolicy(IEnumerable<SettingDto> organizationSettings)
        {
            // Password policy is only defined on system global level 
            // Failed trial times is defined on organization level

            var passwordValidator = new PasswordValidator();

            // userDto might belong to multiple organizations, and multiple programs 
            // 1: get all multiple programs policies 
            // 2: get all multiple organization policies if there is no definition from program policies 
            // 3: get setting from global settings if there is no definitions from programs and organizations 
            // 
            // If one setting has multiple definitions, choose the strictest one
            _userManager.UserLockoutEnabledByDefault = true;
            _userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromHours(_settingService.PasswordLockoutHours()); 

            passwordValidator.RequiredLength =     _settingService.PasswordRequireLength();
            passwordValidator.RequireDigit = _settingService.PasswordRequireDigital();
            passwordValidator.RequireLowercase = _settingService.PassowrdRequireLowerCase();
            passwordValidator.RequireUppercase = _settingService.PasswordRequireUpperCase();
            
            _userManager.MaxFailedAccessAttemptsBeforeLockout = MaxFailedPasswordAttempts(organizationSettings);

            _userManager.PasswordValidator = passwordValidator; 
        }


        #region organization setting;

        private int GetSettingIntValue(SettingType settingType, IEnumerable<SettingDto> organizationSettings, bool isMax = true)
        {
            var defaultValueStr = _globalSettings[settingType];
            var defaultValue = int.Parse(defaultValueStr);
            if (organizationSettings != null)
            {
                defaultValue = isMax
                    ? organizationSettings
                        .Where(i => i.Type == settingType).Max(i => ValueParser.TryParseInt(i.Value, defaultValue))
                    : organizationSettings.Where(i => i.Type == settingType)
                        .Min(i => ValueParser.TryParseInt(i.Value, defaultValue));
            }

            return defaultValue;
        }

        private int GetStrictestPasswordHistoryCounts(IEnumerable<SettingDto> organizationSettings)
        {
            return GetSettingIntValue(SettingType.PasswordHistoryCount, organizationSettings, isMax: false);
        } 

        private int GetStrictestLengthPasswordExpiredDays(IEnumerable<SettingDto> organizationSettings)
        {
            return GetSettingIntValue(SettingType.PasswordExpiredDays, organizationSettings, isMax:false); 
        }

        private int MaxFailedPasswordAttempts(IEnumerable<SettingDto> organizationSettings)
        {
            return GetSettingIntValue(SettingType.MaxFailedPasswordAttempts, organizationSettings, isMax: false);
        } 

        #endregion

        private void SendRegistrationConfirmationEmail(string userId, UserDto userDto)
        {
            //var token = _userManager.GenerateEmailConfirmationTokenAsync(userId).Result;
            //var code = HttpUtility.UrlEncode(token);

            //var subject = "Confirm your account";
            //var html = HttpUtility.HtmlEncode(code); 

            // TODO to use real email templates
            //var replacements = new ListDictionary
            //{
            //    {"{name}", userDto.FirstName + " " + userDto.LastName},
            //    {"{code}", code},
            //    {"{copyCode}", html}
            //};

            //_emailService.SendEmail(EmailType.regi)
            // LinkoExchangeEmailService.SendEmail(userDto.Email, subject, EmailType.RegistrationConfirmation, replacements);
        }

        private IEnumerable<int> GetUserProgramIds(int userId)
        {
            var programs = _programService.GetUserRegulatoryPrograms(userId);
            return programs.Select(i => i.RegulatoryProgramId).ToArray();
        }

        private IEnumerable<int> GetUserOrganizationIds(int userid)
        {
            var orgnizations = _organizationService.GetUserOrganizations(userid);
            return orgnizations.Select(i => i.OrganizationId).ToArray();
        }

        void SendResetPasswordConfirmationEmail(UserProfile userProfile)
        {
            var code = _userManager.GeneratePasswordResetTokenAsync(userProfile.Id).Result; 

            string baseUrl = GetBaseUrl();
            string link = baseUrl + "?code=" + code;

            string supportPhoneNumber = _globalSettings[SettingType.SupportPhoneNumber]; 
            string supportEmail = _globalSettings[SettingType.SupportEmail];

            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("link", link);
            contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
            contentReplacements.Add("supportEmail", supportEmail);

            //_sessionCache.SetValue("ddd") ='''sss'

            _emailService.SendEmail(new[] { userProfile.Email }, EmailType.ForgotPassword_ForgotPassword, contentReplacements);
        }

        string GetBaseUrl()
        {
            return HttpContext.Current.Request.Url.Scheme + "://" 
                + HttpContext.Current.Request.Url.Authority 
                + HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/";
        }
        #endregion
    }
}
