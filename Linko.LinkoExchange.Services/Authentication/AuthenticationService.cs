using System;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.AuditLog;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity.Infrastructure;
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

namespace Linko.LinkoExchange.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ApplicationSignInManager _signInManager;
        private readonly ApplicationUserManager _userManager;
        
        private readonly ISettingService _settingService;
        private readonly IOrganizationService _organizationService;
        private readonly IProgramService _programService;
        private readonly IInvitationService _invitationService;
        private readonly IAuthenticationManager _authenticationManager;

        private readonly TokenGenerator _tokenGenerator = new TokenGenerator();
        private readonly IAuditLogService _auditLogService = new CrommerAuditLogService();

        private readonly IDictionary<string, string> _globalSettings;

        public AuthenticationService(ApplicationUserManager userManager,
            ApplicationSignInManager signInManager, 
            IAuthenticationManager authenticationManager,
            ISettingService settingService,
            IOrganizationService organizationService,
            IProgramService programService,
            IInvitationService invitationService)
        {
            if (userManager == null) throw new ArgumentNullException("userManager");
            if (signInManager == null) throw new ArgumentNullException("signInManager");
            if (authenticationManager == null) throw new ArgumentNullException("authenticationManager");
            if (settingService == null) throw new ArgumentNullException("settingService");
            if (organizationService == null) throw new ArgumentNullException("organizationService");
            if (programService == null) throw new ArgumentNullException("programService");
            if (invitationService == null) throw new ArgumentNullException("invitationService");

            _userManager = userManager;
            _signInManager = signInManager;
            _authenticationManager = authenticationManager; 
            _settingService = settingService;
            _organizationService = organizationService;
            _programService = programService;
            _invitationService = invitationService; 
            _globalSettings = _settingService.GetGlobalSettings();
        }

        public IEnumerable<Claim> GetClaims()
        {
            var claims = HttpContext.Current.Session["claims"] as IEnumerable<Claim>;
            if (claims == null)
            {
                var userId = HttpContext.Current.Session["userId"] as string;
                claims = string.IsNullOrWhiteSpace(userId) ? null : _userManager.GetClaims(userId);
                HttpContext.Current.Session["claims"] = claims;
            }

            return claims;
        }

        // Change or reset password
        /// <summary>
        /// Change password happends after a user login, and change his password
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="newPassword">The new password</param>
        /// <returns></returns>
        public Task<AuthenticationResultDto> ChangePasswordAsync(string userId, string newPassword)
        {
            var authenticationResult = new AuthenticationResultDto();
            try
            {
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
                var programIds = new[] {_invitationService.GetInvitationProgram(registrationToken).ProgramId};
                var organizationIds = _invitationService.GetInvitationrOrganizations(registrationToken).Select(i=>i.OrganizationId); 

                SetPasswordPolicy(programIds,organizationIds);

                var result = _userManager.CreateAsync(applicationUser, userInfo.Password).Result;

                if (result == IdentityResult.Success)
                {
                    authenticationResult.Success = true; 
                    SendRegistrationConfirmationEmail(applicationUser.Id, userInfo);  
                    return Task.FromResult(authenticationResult);
                }

                authenticationResult.Errors = result.Errors;
                return Task.FromResult(authenticationResult);
            }
            catch (Exception ex)
            {
                var errors = new List<string> {ex.Message};
                authenticationResult.Errors = errors;
            }

            return Task.FromResult(authenticationResult);
        }

        /// <summary>
        /// Reset password happends when user request a 'reset password', and system generates a reset password token and sends to user's email
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
                    //TODO
                    var userId = 100;  //applicationUser.profileId;
                    var programIds = GetUserProgramIds(userId);
                    var organizationIds = GetUserOrganizationIds(userId);
                    SetPasswordPolicy(programIds, organizationIds);

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
        /// <param name="userName">The username used when sign in</param>
        /// <param name="password">The password used when sign in</param>
        /// <param name="isPersistent">The flag indicates persistent the sign or not</param>
        /// <returns></returns>
        public Task<SignInResultDto> SignInByUserName(string userName, string password, bool isPersistent)
        {
            SignInResultDto signInResultDto = new SignInResultDto();
            
            //Todo find out userDto's organizations, and set policy 
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

            // clear claims from db if there are   
            ClearClaims(applicationUser.Id);
            applicationUser.Claims.Clear();

            // Todo: change to the real userid;
            var userId = 100;  //applicationUser.profileId;
            var programIds = GetUserProgramIds(userId);
            var organizationIds = GetUserOrganizationIds(userId);
            SetPasswordPolicy(programIds, organizationIds);

            _signInManager.UserManager = _userManager; 
            var signInStatus = _signInManager.PasswordSignInAsync(userName, password, isPersistent, true).Result;

            if (signInStatus == SignInStatus.Success)
            {
                //TODO try sign on GetUserIdentity
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

            var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            };

            foreach (var claim in claims)
            {
                _userManager.AddClaim(applicationUser.Id, claim);
            } 

            _authenticationManager.SignIn(authProperties, identity); 
        }

        private void SetPasswordPolicy(IEnumerable<int> programIds, IEnumerable<int> orgnizationIds)
        {
            // todo: from registrationToken get program Id, then get organizationIds
            var organizationSettings = _settingService.GetOrganizationSettingsByIds(orgnizationIds).SelectMany(i => i.Settings);
            var programSettings = _settingService.GetProgramSettingsByIds(programIds).SelectMany(i => i.Settings);

            var passwordValidator = new PasswordValidator();

            // userDto might belong to multiple organizations, and multiple programs 
            // 1: get all multiple programs policies 
            // 2: get all multiple organization policies if there is no definition from program policies 
            // 3: get setting from global settings if there is no definitions from programs and organizations 
            // 
            // If one setting has muliple definitions, choose the strictest one
            _userManager.UserLockoutEnabledByDefault = true;
            _userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(GetStrictestLengthPasswordRequire(programSettings, organizationSettings, "DefaultAccountLockoutTimeSpan"));

            passwordValidator.RequiredLength = GetStrictestLengthPasswordRequire(programSettings, organizationSettings, "PasswordRequireLength");
            passwordValidator.RequireDigit = GetStrictestBooleanPasswordRequire(programSettings, organizationSettings,
                "PasswordRequireDigit");

            passwordValidator.RequireLowercase = GetStrictestBooleanPasswordRequire(programSettings, organizationSettings,
                         "PasswordRequireLowercase");

            passwordValidator.RequireUppercase = GetStrictestBooleanPasswordRequire(programSettings, organizationSettings,
                     "PasswordRequireUpppercase");

            _userManager.MaxFailedAccessAttemptsBeforeLockout = GetStrictestLengthPasswordRequire(programSettings,
                organizationSettings, "MaxFailedAccessAttemptsBeforeLockout"); 

            _userManager.PasswordValidator = passwordValidator; 
        } 

        private int GetStrictestLengthPasswordRequire(IEnumerable<SettingDto> programSettings,IEnumerable<SettingDto> organizationSettings, string settingKey)
        {
            var maxPasswordRequireLenthStr = _globalSettings["PasswordRequireLength"];
            var maxPasswordRequireLengthValue = int.Parse(maxPasswordRequireLenthStr);
            if (programSettings.Any(i => i.Name.Equals("PasswordRequireLength")))
            {
                maxPasswordRequireLengthValue =
                    programSettings.Max(i => ValueParser.TryParseInt(i.Value, maxPasswordRequireLengthValue));
            }
            else
            {
                maxPasswordRequireLengthValue =
                    organizationSettings.Max(i => ValueParser.TryParseInt(i.Value, maxPasswordRequireLengthValue));
            }

            return maxPasswordRequireLengthValue;
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
                return programSettings.Any(i => i.Value.ToLower() == "true");
            }

            if (organizationSettings.Any(i=> i.Name.Equals(settingKey)))
            {
                return organizationSettings.Any(i => i.Value.ToLower() == "true"); 
            }

            return passwordRequireDigitalValue;
        }

        private void SendRegistrationConfirmationEmail(string userId, UserDto userDto)
        {
            var token = _userManager.GenerateEmailConfirmationTokenAsync(userId).Result;
            var code = HttpUtility.UrlEncode(token);

            var subject = "Confirm your account";
            var html = HttpUtility.HtmlEncode(code);

            var replacements = new ListDictionary
            {
                {"{name}", userDto.FirstName + " " + userDto.LastName},
                {"{code}", code},
                {"{copyCode}", html}
            };

             LinkoExchangeEmailService.SendEmail(userDto.Email, subject, EmailType.RegistrationConfirmation, replacements);
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

            code = HttpUtility.UrlEncode(code);

            var subject = "Reset Password";
            var html = HttpUtility.HtmlEncode(code); ;

            //TODO to replace the values 
            var replacements = new ListDictionary();
            replacements.Add("{code}", code);
            replacements.Add("{copyCode", html);
            replacements.Add("{userId}", user.Id);

            LinkoExchangeEmailService.SendEmail(user.Email, subject, EmailType.ResetPasswordConfirmation, replacements);
        }

        #endregion
    }
}
