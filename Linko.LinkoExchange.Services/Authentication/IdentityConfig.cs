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
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
            var provider = new Microsoft.Owin.Security.DataProtection.DpapiDataProtectionProvider("TokenProvider");
            var tokenLife =
                ValueParser.TryParseInt(ConfigurationManager.AppSettings["ResetPasswordTokenValidateInterval"], 168);

            UserTokenProvider =
                new DataProtectorTokenProvider<ApplicationUser>(provider.Create("ASP.NET Identity"))
                {
                    // set the token life span;
                    TokenLifespan = TimeSpan.FromHours(tokenLife)
                };


            EmailService = new EmailService();
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager (new UserStore<ApplicationUser> (context.Get<ApplicationDbContext> ()));
            
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser> (manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            //
            //var cookieValidateInterval = ValueParser.TryParseInt(ConfigurationManager.AppSettings["CookieValidateInterval"], 30); 

            // Configure user lockout defaults
            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromDays(1);   
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider ("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider ("Email Code", new EmailTokenProvider<ApplicationUser>
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
                    new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"))
                    {
                        // set the token life span;
                        TokenLifespan = TimeSpan.FromHours(tokenLife)
                    };

            }

            return manager;
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base (userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync ((ApplicationUserManager) UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager (context.GetUserManager<ApplicationUserManager> (), context.Authentication);
        }
    }
}
