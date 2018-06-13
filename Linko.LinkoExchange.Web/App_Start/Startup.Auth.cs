using System;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Mvc;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Authentication;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace Linko.LinkoExchange.Web
{
    public partial class Startup
    {
        // add this static variable
        //internal static IDataProtectionProvider DataProtectionProvider { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        // ReSharper disable once MemberCanBePrivate.Global
        public void ConfigureAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext(() => DependencyResolver.Current.GetService<ApplicationUserManager>());

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party SignIn provider
            // Configure the sign in cookie
            
            var cookieValidateInterval = ((SessionStateSection) ConfigurationManager.GetSection(sectionName:"system.web/sessionState")).Timeout.Minutes;
            app.UseCookieAuthentication(options:new CookieAuthenticationOptions
                                                {
                                                    AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                                                    LoginPath = new PathString(value:"/Account/SignIn"),
                                                    ExpireTimeSpan = TimeSpan.FromMinutes(value:cookieValidateInterval),
                                                    SlidingExpiration = true,
                                                    Provider = new CookieAuthenticationProvider
                                                               {
                                                                   OnResponseSignIn = context => { context.Properties.IsPersistent = false; },

                                                                   // Enables the application to validate the security stamp when the user logs in.
                                                                   // This is a security feature which is used when you change a password or add an external SignIn to your account.  
                                                                   OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, UserProfile>
                                                                       (validateInterval:TimeSpan.FromMinutes(value:cookieValidateInterval),
                                                                        regenerateIdentity:(manager, user) => user.GenerateUserIdentityAsync(manager:manager))
                                                               }
                                                });

            //app.UseExternalSignInCookie(externalAuthenticationType:DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            //app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second SignIn verification factor such as phone or email.
            // Once you check this option, your second step of verification during the SignIn process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            //app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie); 
        }
    }
}