using System;
using System.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Linko.LinkoExchange.Services.Email;

namespace Linko.LinkoExchange.Services.Authentication
{
    public class ApplicationUserManager : UserManager<UserProfile>
    {
        public ApplicationUserManager(IUserStore<UserProfile> store)
            : base(store)
        {
            var provider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("TokenProvider");
            var tokenLife =
                ValueParser.TryParseInt(ConfigurationManager.AppSettings["ResetPasswordTokenValidateInterval"], 168);

            UserTokenProvider =
                new DataProtectorTokenProvider<UserProfile>(provider.Create("ASP.NET Identity"))
                {
                    // set the token life span;
                    TokenLifespan = TimeSpan.FromHours(tokenLife)
                };


            EmailService = new EmailService();
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager (new UserStore<UserProfile> (context.Get<LinkoExchangeContext> ()));
            
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<UserProfile> (manager)
            {
                AllowOnlyAlphanumericUserNames = false 
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 8,                
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            //
            //var cookieValidateInterval = ValueParser.TryParseInt(ConfigurationManager.AppSettings["CookieValidateInterval"], 30); 

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(1);   
            manager.MaxFailedAccessAttemptsBeforeLockout = 3;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the userProfile
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider ("Phone Code", new PhoneNumberTokenProvider<UserProfile>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider ("Email Code", new EmailTokenProvider<UserProfile>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });
            manager.EmailService = new EmailService (); 
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                var tokenLife =
                    ValueParser.TryParseInt(ConfigurationManager.AppSettings["ResetPasswordTokenValidateInterval"], 168);

                manager.UserTokenProvider =
                    new DataProtectorTokenProvider<UserProfile>(dataProtectionProvider.Create("ASP.NET Identity"))
                    {
                        // set the token life span;
                        TokenLifespan = TimeSpan.FromHours(tokenLife)
                    };

            }

            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<UserProfile, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base (userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(UserProfile userProfile)
        {
            return userProfile.GenerateUserIdentityAsync ((ApplicationUserManager) UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager (context.GetUserManager<ApplicationUserManager> (), context.Authentication);
        }
    }
}
