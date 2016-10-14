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
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Permission;

namespace Linko.LinkoExchange.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private static readonly ApplicationDbContext _linkoExchangeDbContext = new ApplicationDbContext(); 

        private readonly ApplicationSignInManager _signInManager;
        private readonly ApplicationUserManager _userManager;
        
        private readonly ISettingService _settingService;
        private readonly IOrganizationService _organizationService;
        private readonly IProgramService _programService;
        private readonly IInvitationService _invitationService;
        private readonly IAuthenticationManager _authenticationManager;
        private readonly IPermissionService _permissionService;
        private readonly IEmailService _emailService;

        private readonly TokenGenerator _tokenGenerator = new TokenGenerator();
        private readonly IAuditLogService _auditLogService = new CrommerAuditLogService();

        private readonly IDictionary<string, string> _globalSettings;

        public AuthenticationService(ApplicationUserManager userManager,
            ApplicationSignInManager signInManager, 
            IAuthenticationManager authenticationManager,
            ISettingService settingService,
            IOrganizationService organizationService,
            IProgramService programService,
            IInvitationService invitationService,
            IEmailService emailService,
            IPermissionService permissionService)
        {
            if (userManager == null) throw new ArgumentNullException("userManager");
            if (signInManager == null) throw new ArgumentNullException("signInManager");
            if (authenticationManager == null) throw new ArgumentNullException("authenticationManager");
            if (settingService == null) throw new ArgumentNullException("settingService");
            if (organizationService == null) throw new ArgumentNullException("organizationService");
            if (programService == null) throw new ArgumentNullException("programService");
            if (invitationService == null) throw new ArgumentNullException("invitationService");
            if (emailService == null) throw new ArgumentNullException("emailService");
            if (permissionService == null) throw new ArgumentNullException("permissionService");

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
        }

        public IList<Claim> GetClaims()
        {
            var claims = HttpContext.Current.Session["claims"] as IEnumerable<Claim>;
            if (claims == null)
            {
                var userId = HttpContext.Current.Session["userId"] as string;
                claims = string.IsNullOrWhiteSpace(userId) ? null : _userManager.GetClaims(userId);
                HttpContext.Current.Session["claims"] = claims;
            }

            return claims.ToList();
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
                var userId = HttpContext.Current.Session["userId"] as string;
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

                SetPasswordPolicy(organizationSettings, programSettings); 
                // Use PasswordValidator
                var validateResult = _userManager.PasswordValidator.ValidateAsync(newPassword).Result;
                if (validateResult.Succeeded == false)
                {
                    authenticationResult.Success = false;
                    authenticationResult.Errors = validateResult.Errors;
                    return Task.FromResult(authenticationResult);
                }

                // Check if the new password is one of the password used last # numbers
                if (IsValidPasswordCheckInHistory(applicationUser.PasswordHash, applicationUser.UserProfileId, organizationSettings, programSettings))
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
            var applicationUser = AssignUser(userInfo.UserName, userInfo.Email);
            try
            {  
                // TODO invitation and program and organization relationship ?  one to many or one to one?  
                var programIds = new[] {_invitationService.GetInvitationProgram(registrationToken).ProgramId};
                var organizationIds = _invitationService.GetInvitationrOrganizations(registrationToken).Select(i=>i.OrganizationId);
                 
                var organizationSettings = _settingService.GetOrganizationSettingsByIds(organizationIds).SelectMany(i => i.Settings).ToList();
                var programSettings = _settingService.GetProgramSettingsByIds(programIds).SelectMany(i => i.Settings).ToList();

                SetPasswordPolicy(organizationSettings, programSettings);

                var result = _userManager.CreateAsync(applicationUser, userInfo.Password).Result;

                if (result == IdentityResult.Success)
                {
                    // Retrieve user again to get userProfile Id. 
                    applicationUser = _userManager.FindById(applicationUser.Id);  

                    // 1. Create a new row in user password history table 
                    _linkoExchangeDbContext.UserPasswordHistories.Add(new UserPasswordHistory
                    {
                         LastModificationDateTime = DateTime.UtcNow,
                         PasswordHash = applicationUser.PasswordHash,
                         UserProfileId = applicationUser.UserProfileId
                    });
                    
                    // 2 TODO change user status to  Registration Pending
                    // UC-42 6


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
                        _emailService.SendEmail(sendTo, EmailType.Registration_AuthorityUserRegistrationPendingToApprovers, emailContentReplacements, logEntry);
                    }
                    else
                    {  
                        _emailService.SendEmail(sendTo, EmailType.Registration_IndustryUserRegistrationPendingToApprovers, emailContentReplacements, logEntry);
                    } 

                    // 5 TODO logs invite email  
                    // UC-1 

                    // 6 TODO logs invite to Audit 
                    // UC-2 

                    _linkoExchangeDbContext.SaveChangesAsync();

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
        /// Reset password happens when user request a 'reset password', and system generates a reset password token and sends to user's email
        /// And user click the link in the email to reset the password.
        /// </summary>
        /// <param name="email">User email address</param>
        /// <param name="resetPasswordToken">The reset password token</param>
        /// <param name="newPassword">The new password</param>
        /// <returns></returns>
        public async Task<AuthenticationResultDto> ResetPasswordAsync(string email, string resetPasswordToken,
            string newPassword)
        {
            AuthenticationResultDto authenticationResult = new AuthenticationResultDto();
            try
            {
                // Step 1: Determine if the user by email address exists 
                var applicationUser = await _userManager.FindByEmailAsync(email);
                if (applicationUser == null)
                {
                    authenticationResult.Success = false;
                    authenticationResult.Result = AuthenticationResult.UserNotFound;
                }
                else if (applicationUser.EmailConfirmed == false)
                {
                    authenticationResult.Success = false;
                    authenticationResult.Result = AuthenticationResult.EmailIsNotConfirmed;
                }
                else
                {
                    var userId = applicationUser.UserProfileId;
                    var programIds = GetUserProgramIds(userId);
                    var organizationIds = GetUserOrganizationIds(userId);

                    var organizationSettings = _settingService.GetOrganizationSettingsByIds(organizationIds).SelectMany(i => i.Settings).ToList();
                    var programSettings = _settingService.GetProgramSettingsByIds(programIds).SelectMany(i => i.Settings).ToList();

                    SetPasswordPolicy(organizationSettings, programSettings); 

                    // Step 2: reset the password
                    var identityResult =
                        await _userManager.ResetPasswordAsync(applicationUser.Id, resetPasswordToken, newPassword);

                    if (identityResult.Succeeded)
                    {
                        authenticationResult.Success = true;
                    }
                    else
                    {
                        authenticationResult.Success = false;
                        authenticationResult.Errors = identityResult.Errors;
                    }
                }

                return authenticationResult;
            }
            catch (Exception ex)
            {
                var errors = new List<string> {ex.Message};
                authenticationResult.Errors = errors;
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
        public async Task<AuthenticationResultDto> RequestResetPassword(string email)
        {
            AuthenticationResultDto authenticationResult = new AuthenticationResultDto();

            var user = _userManager.FindByEmail(email);
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
            if (_userManager.IsLockedOut(applicationUser.Id))
            {
                signInResultDto.AutehticationResult = AuthenticationResult.PasswordLockedOut;
                return Task.FromResult(signInResultDto);
            }

            // TODO to check if the user has been locked "Account Lockout"  by an authority
            
            // TODO to check if the user is disabled or not  
            // clear claims from db if there are   

            ClearClaims(applicationUser.Id);
            applicationUser.Claims.Clear();
            
            var userId = applicationUser.UserProfileId;
            var programIds = GetUserProgramIds(userId);
            var organizationIds = GetUserOrganizationIds(userId);

            var organizationSettings = _settingService.GetOrganizationSettingsByIds(organizationIds).SelectMany(i => i.Settings).ToList();
            var programSettings = _settingService.GetProgramSettingsByIds(programIds).SelectMany(i => i.Settings).ToList();
            // Check if user's password is expired or not  
            if (IsUserPasswordExpired(userId, organizationSettings, programSettings))
            {
                signInResultDto.AutehticationResult = AuthenticationResult.PasswordExpired;
                return Task.FromResult(signInResultDto); 
            }

            SetPasswordPolicy(organizationSettings, programSettings); 

            _signInManager.UserManager = _userManager; 
            var signInStatus = _signInManager.PasswordSignInAsync(userName, password, isPersistent, true).Result;

            if (signInStatus == SignInStatus.Success)
            {
                signInResultDto.AutehticationResult = AuthenticationResult.Success;
                signInResultDto.Token = _tokenGenerator.GenToken(userName);

                HttpContext.Current.Session["userId"] = applicationUser.Id; 

                //Set user's claims
                GetUserIdentity(applicationUser); 
            }
            else if (signInStatus == SignInStatus.Failure)
            {
                signInResultDto.AutehticationResult = AuthenticationResult.InvalidUserNameOrPassword;
            }

            return Task.FromResult(signInResultDto);
        }

        public void SignOff()
        {
            var userId = HttpContext.Current.Session["userid"] as string;
            ClearClaims(userId);
            HttpContext.Current.Session.Clear();
            _authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie); 
        }

        #region private section

        // Check if password is in one of the last # passwords stores in UserPasswordHistory table
        // Return false means the new password is not validate because it has been used in the last #number of times
        // Return true means the new password is validate to use
        private bool IsValidPasswordCheckInHistory(string passwordHash, int userProfileId, IEnumerable<SettingDto> organizationSettings, IEnumerable<SettingDto> programSettings)
        {
            var numberOfPasswordsInHistory = GetStrictestPasswordHistoryCounts(programSettings, organizationSettings);

            var lastNumberPasswordInHistory = _linkoExchangeDbContext.UserPasswordHistories
                .Where(i => i.UserProfileId == userProfileId)
                .OrderByDescending(i => i.LastModificationDateTime).Take(numberOfPasswordsInHistory);
            if (lastNumberPasswordInHistory.Any(i=>i.PasswordHash == passwordHash))
            {
                return false; 
            }

            return true;
        } 

        private bool IsUserPasswordExpired(int userProfileId, IEnumerable<SettingDto> organizationSettings, IEnumerable<SettingDto> programSettings)
        {
            var lastestUserPassword = _linkoExchangeDbContext.UserPasswordHistories
                .Where(i => i.UserProfileId == userProfileId)
                .OrderByDescending(i => i.LastModificationDateTime).FirstOrDefault();

            if (lastestUserPassword == null || lastestUserPassword.UserProfileId == 0)
            {
                return true;
            }

            // Get password expiration setting
            var passwordExpiredDays = GetStrictestLengthPasswordExpiredDays(programSettings, organizationSettings);
            if (DateTime.UtcNow > lastestUserPassword.LastModificationDateTime.AddDays(passwordExpiredDays))
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

        private ApplicationUser AssignUser(string userName, string email)
        {
            return new ApplicationUser
            {
                UserName = userName,
                Email = email
            };
        } 
         
        private void GetUserIdentity(ApplicationUser applicationUser)
        { 
            //TODO: to replace using real claims  
            // get userDto's role, organizations, programs, current organization, current program.....
             
            var claims = new List<Claim>(); 
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "userId"));
            claims.Add(new Claim(ClaimTypes.Name, "Shuhao Wang"));
            claims.Add(new Claim(ClaimTypes.Role, "IU-Standard"));
            claims.Add(new Claim("OrganizationId","12345"));
            claims.Add(new Claim("ProgramId", "678910"));
            claims.Add(new Claim("CurrentProgramId", "678910"));
            claims.Add(new Claim("OrganizationName", "This is a very looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooog orgnaization name"));
            claims.Add(new Claim("IndustryName", "This is  a very loooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong industry name"));
            claims.Add(new Claim("ProgramName", "This is a very looooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooong program name"));

            SaveClaims(applicationUser.Id, claims);
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
        private void SetPasswordPolicy(IEnumerable<SettingDto> organizationSettings, IEnumerable<SettingDto> programSettings)
        {

            var passwordValidator = new PasswordValidator();

            // userDto might belong to multiple organizations, and multiple programs 
            // 1: get all multiple programs policies 
            // 2: get all multiple organization policies if there is no definition from program policies 
            // 3: get setting from global settings if there is no definitions from programs and organizations 
            // 
            // If one setting has multiple definitions, choose the strictest one
            _userManager.UserLockoutEnabledByDefault = true;
            _userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(GetStrictestDefaultAccountLockoutTimeSpan(programSettings, organizationSettings));

            passwordValidator.RequiredLength = GetStrictestLengthPasswordRequire(programSettings, organizationSettings);
            passwordValidator.RequireDigit = GetStrictestBooleanPasswordRequire(programSettings, organizationSettings,
                "PasswordRequireDigit");

            passwordValidator.RequireLowercase = GetStrictestBooleanPasswordRequire(programSettings, organizationSettings,
                         "PasswordRequireLowercase");

            passwordValidator.RequireUppercase = GetStrictestBooleanPasswordRequire(programSettings, organizationSettings,
                     "PasswordRequireUpppercase");

            _userManager.MaxFailedAccessAttemptsBeforeLockout = GetStrictestFailedAccessAttemptsBeforeLockout(programSettings, organizationSettings); 

            _userManager.PasswordValidator = passwordValidator; 
        }
        
        private int GetSettingIntValue(string settingKey, IEnumerable<SettingDto> programSettings, IEnumerable<SettingDto> organizationSettings, bool isMax = true)
        {
            var defaultValueStr = _globalSettings[settingKey];

            var defaultValue = int.Parse(defaultValueStr);
            if (programSettings.Any(i => i.Name.Equals(settingKey)))
            {
                defaultValue = isMax ? programSettings.Where(i=>i.Name== settingKey)
                    .Max(i => ValueParser.TryParseInt(i.Value, defaultValue)) :
                    programSettings.Where(i => i.Name == settingKey).Min(i => ValueParser.TryParseInt(i.Value, defaultValue));
            }
            else
            {
                defaultValue = isMax ? organizationSettings
                    .Where(i => i.Name == settingKey).Max(i => ValueParser.TryParseInt(i.Value, defaultValue)) : 
                    organizationSettings.Where(i => i.Name == settingKey).Min(i => ValueParser.TryParseInt(i.Value, defaultValue));
            }

            return defaultValue;
        }

        private int GetStrictestFailedAccessAttemptsBeforeLockout(IEnumerable<SettingDto> programSettings, IEnumerable<SettingDto> organizationSettings)
        {
            string settingKey = "MaxFailedAccessAttemptsBeforeLockout";

            if (!_globalSettings.ContainsKey(settingKey))
            {
                return 6;
            }

            return GetSettingIntValue(settingKey, programSettings, organizationSettings, isMax: false);
        }

        private int GetStrictestPasswordHistoryCounts(IEnumerable<SettingDto> programSettings, IEnumerable<SettingDto> organizationSettings)
        {
            string settingKey = "NumberOfPasswordsInHistory";

            if (!_globalSettings.ContainsKey(settingKey))
            {
                return 10;
            }

            return GetSettingIntValue(settingKey, programSettings, organizationSettings, isMax: true);
        }

        private int GetStrictestDefaultAccountLockoutTimeSpan(IEnumerable<SettingDto> programSettings, IEnumerable<SettingDto> organizationSettings)
        {
            string settingKey = "DefaultAccountLockoutTimeSpan"; 
            if (!_globalSettings.ContainsKey(settingKey))
            {
                return 1;
            }

            return GetSettingIntValue(settingKey, programSettings, organizationSettings);

        } 

        private int GetStrictestLengthPasswordExpiredDays(IEnumerable<SettingDto> programSettings, IEnumerable<SettingDto> organizationSettings)
        {
            string settingKey = "PasswordExpiredDays";  
            if (!_globalSettings.ContainsKey(settingKey))
            {
                return 90;
            }

            return GetSettingIntValue(settingKey, programSettings, organizationSettings, isMax:false); 
        } 

        private int GetStrictestLengthPasswordRequire(IEnumerable<SettingDto> programSettings,IEnumerable<SettingDto> organizationSettings)
        {
            var settingKey = "PasswordRequireLength";  
            if (!_globalSettings.ContainsKey(settingKey))
            {
                return 6;
            }


            return GetSettingIntValue(settingKey, programSettings, organizationSettings, isMax:true); 
        }

        private bool GetStrictestBooleanPasswordRequire(IEnumerable<SettingDto> programSettings, IEnumerable<SettingDto> organizationSettings,string settingKey)
        {
            if (!_globalSettings.ContainsKey(settingKey))
            {
                return false;
            }

            var passwordRequireDigital = _globalSettings[settingKey];
            var passwordRequireDigitalValue = bool.Parse(passwordRequireDigital);
            if (programSettings.Any(i => i.Name.Equals(settingKey)))
            {
                return programSettings.Where(i => i.Name == settingKey).Any(i => i.Value.ToLower() == "true");
            }

            if (organizationSettings.Any(i=> i.Name.Equals(settingKey)))
            {
                return organizationSettings.Where(i => i.Name == settingKey).Any(i => i.Value.ToLower() == "true"); 
            }

            return passwordRequireDigitalValue;
        }

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
            var programs = _programService.GetUserPrograms(userId);
            return programs.Select(i => i.ProgramId).ToArray();
        }

        private IEnumerable<int> GetUserOrganizationIds(int userid)
        {
            var orgnizations = _organizationService.GetUserOrganizations(userid);
            return orgnizations.Select(i => i.OrganizationId).ToArray();
        }

        void SendResetPasswordConfirmationEmail(ApplicationUser user)
        {
            var code = _userManager.GeneratePasswordResetTokenAsync(user.Id).Result; 

            string baseUrl = GetBaseUrl();
            string link = baseUrl + "?code=" + code;

            string supportPhoneNumber = _globalSettings["supportPhoneNumber"]; 
            string supportEmail = _globalSettings["supportEmail"];

            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("link", link);
            contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
            contentReplacements.Add("supportEmail", supportEmail);

            var logEntry = new EmailAuditLogEntryDto();
            logEntry.RecipientFirstName = "First Name";
            logEntry.RecipientLastName = "last name";
            logEntry.RecipientUserName = "Fist name, lastName";
            logEntry.SenderFirstName = "Linko support";
            logEntry.SenderLastName = "Linko support";


            _emailService.SendEmail(new[] { user.Email }, EmailType.ForgotPassword_ForgotPassword, contentReplacements, logEntry);
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
