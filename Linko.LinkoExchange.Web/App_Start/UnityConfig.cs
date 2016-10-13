using System;
using System.Web;
using Microsoft.Practices.Unity;
using Linko.LinkoExchange.Services.Authentication;
using Microsoft.Owin.Security;
using System.Data.Entity;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Core.Logging;
using NLog;
using Linko.LinkoExchange.Web.Controllers;
using AutoMapper;
using Linko.LinkoExchange.Services;

namespace Linko.LinkoExchange.Web
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below. Make sure to add a Microsoft.Practices.Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your types here
            // container.RegisterType<IProductRepository, ProductRepository>();

            // Logger
            container.AddNewExtension<NLogExtension>();

            // Custom filter
            container.RegisterType<CommonController>(new InjectionConstructor(typeof(ILogger)));

            container.RegisterType<DbContext, ApplicationDbContext> (new PerRequestLifetimeManager ());
            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>> ();
            container.RegisterType<ApplicationSignInManager> ();
            container.RegisterType<ApplicationUserManager> ();
            container.RegisterType<IAuthenticationManager> (new InjectionFactory (c => HttpContext.Current.GetOwinContext ().Authentication));
            container.RegisterType<IAuthenticationManager>(new InjectionFactory(c => HttpContext.Current.GetOwinContext().Authentication));
            container.RegisterType<ApplicationSignInManager>(new InjectionFactory(c => HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>()));
            container.RegisterType<IAuthenticationService, AuthenticationService> ();
            container.RegisterType<ISettingService, SettingService>();
            container.RegisterType<IOrganizationService, OrganizationService>();
            container.RegisterType<IInvitationService, InvitationService>();
            container.RegisterType<IProgramService, ProgramService>();
            container.RegisterInstance<IMapper>(Mapper.Instance);
        }
    }
}
