using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Email;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;

namespace Linko.LinkoExchange.Services.Authentication
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class ApplicationUserManager : UserManager<UserProfile>
    {
        #region constructors and destructor

        // ReSharper disable once MemberCanBePrivate.Global
        public ApplicationUserManager(IUserStore<UserProfile> store)
            : base(store:store)
        {
            var provider = new DpapiDataProtectionProvider(appName:"TokenProvider");
            var tokenLife = ValueParser.TryParseInt(value:ConfigurationManager.AppSettings[name:"ResetPasswordTokenValidateInterval"], defaultValue:168);

            UserTokenProvider = new DataProtectorTokenProvider<UserProfile>(protector:provider.Create("ASP.NET Identity"))
                                {
                                    // set the token life span;
                                    TokenLifespan = TimeSpan.FromHours(value:tokenLife)
                                };

            UserValidator = new UserValidator<UserProfile>(manager:this)
                            {
                                AllowOnlyAlphanumericUserNames = false
                            };

            PasswordValidator = new PasswordValidator
                                {
                                    RequiredLength = 8,
                                    RequireNonLetterOrDigit = false,
                                    RequireDigit = true,
                                    RequireLowercase = true,
                                    RequireUppercase = true
                                };

            // Configure user lockout defaults
            UserLockoutEnabledByDefault = true;
            DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(value:1);
            MaxFailedAccessAttemptsBeforeLockout = 3;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the userProfile
            // You can write your own provider and plug it in here.
            //RegisterTwoFactorProvider(twoFactorProvider:"Phone Code", provider:new PhoneNumberTokenProvider<UserProfile>
            //                                                                   {
            //                                                                       MessageFormat = "Your security code is {0}"
            //                                                                   });
            //RegisterTwoFactorProvider(twoFactorProvider:"Email Code", provider:new EmailTokenProvider<UserProfile>
            //                                                                   {
            //                                                                       Subject = "Security Code",
            //                                                                       BodyFormat = "Your security code is {0}"
            //                                                                   });

            //EmailService = new EmailService();
        }

        #endregion

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(store:new UserStore<UserProfile>(context:context.Get<LinkoExchangeContext>()));

            // Configure validation logic for user names
            manager.UserValidator = new UserValidator<UserProfile>(manager:manager)
                                    {
                                        AllowOnlyAlphanumericUserNames = false
                                    };

            // Configure validation logic for passwords
            var passwordRequiredLength = ValueParser.TryParseInt(value:ConfigurationManager.AppSettings[name:"PasswordRequiredLength"], defaultValue:8);
            manager.PasswordValidator = new PasswordValidator
                                        {
                                            RequiredLength = passwordRequiredLength,
                                            RequireNonLetterOrDigit = false,
                                            RequireDigit = true,
                                            RequireLowercase = true,
                                            RequireUppercase = true
                                        };

            //var cookieValidateInterval = ValueParser.TryParseInt(ConfigurationManager.AppSettings["CookieValidateInterval"], 30); 

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(value:1);
            manager.MaxFailedAccessAttemptsBeforeLockout = 3;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the userProfile
            // You can write your own provider and plug it in here.
            //manager.RegisterTwoFactorProvider(twoFactorProvider:"Phone Code", provider:new PhoneNumberTokenProvider<UserProfile>
            //                                                                           {
            //                                                                               MessageFormat = "Your security code is {0}"
            //                                                                           });
            //manager.RegisterTwoFactorProvider(twoFactorProvider:"Email Code", provider:new EmailTokenProvider<UserProfile>
            //                                                                           {
            //                                                                               Subject = "Security Code",
            //                                                                               BodyFormat = "Your security code is {0}"
            //                                                                           });
            //manager.EmailService = new EmailService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                var tokenLife =
                    ValueParser.TryParseInt(value:ConfigurationManager.AppSettings[name:"ResetPasswordTokenValidateInterval"], defaultValue:168);

                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<UserProfile>(protector:dataProtectionProvider.Create("ASP.NET Identity"))
                    {
                        // set the token life span;
                        TokenLifespan = TimeSpan.FromHours(value:tokenLife)
                    };
            }

            return manager;
        }

        public virtual async Task<IdentityResult> UnlockUserAccount(string userId)
        {
            var result = await SetLockoutEndDateAsync(userId:userId, lockoutEnd:DateTimeOffset.MinValue);
            if (result.Succeeded)
            {
                result = await ResetAccessFailedCountAsync(userId:userId);
            }
            return result;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<UserProfile, string>
    {
        #region constructors and destructor

        // ReSharper disable once MemberCanBePrivate.Global
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager:userManager, authenticationManager:authenticationManager) { }

        #endregion

        #region interface implementations

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(UserProfile userProfile)
        {
            return userProfile.GenerateUserIdentityAsync(manager:(ApplicationUserManager) UserManager);
        }

        #endregion

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(userManager:context.GetUserManager<ApplicationUserManager>(), authenticationManager:context.Authentication);
        }
    }
}