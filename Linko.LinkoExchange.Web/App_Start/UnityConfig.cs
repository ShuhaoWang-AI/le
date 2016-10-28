using System;
using System.Web;
using Microsoft.Practices.Unity;
using Linko.LinkoExchange.Services.Authentication;
using Microsoft.Owin.Security;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Core.Logging;
using NLog;
using AutoMapper;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.Email;
using System.Configuration;
using Linko.LinkoExchange.Web.Mvc;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Invitation;

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

            // Data layer
            string connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ToString();
            container.RegisterType<LinkoExchangeContext>(new PerRequestLifetimeManager(), new InjectionConstructor(connectionString));

            // Logger
            container.AddNewExtension<NLogExtension>();
            
            // Custom filter
            container.RegisterType<CustomHandleErrorAttribute>(new InjectionConstructor(typeof(ILogger)));
            //  container.RegisterType<CommonInfoAttribute>(new InjectionConstructor(typeof(IAuthenticationService)));

            // Services
            container.RegisterType<IAuthenticationManager>(new InjectionFactory(c => HttpContext.Current.GetOwinContext().Authentication));
            //container.RegisterType<ApplicationSignInManager>(new InjectionFactory(c => HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>()));
            container.RegisterType<IAuthenticationService, AuthenticationService>();
            container.RegisterType<ISettingService, SettingService>();
            container.RegisterType<IOrganizationService, OrganizationService>();
            container.RegisterType<IInvitationService, InvitationService>();
            container.RegisterType<IProgramService, ProgramService>();
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<IHttpContextService, HttpContextService>();
            container.RegisterType<IRequestCache, RequestCache>();
            container.RegisterType<ISessionCache, SessionCache>();

            // Custom identity services           
            container.RegisterType<ApplicationSignInManager>();
            container.RegisterType<ApplicationUserManager>(); 
            container.RegisterType<IUserStore<UserProfile>, UserStore<UserProfile>>(new InjectionConstructor(typeof(LinkoExchangeContext)));
            container.RegisterType<IPermissionService, PermissionService>();
            container.RegisterType<IAuditLogEntry, EmailAuditLogEntryDto>();
            container.RegisterType<IPasswordHasher, PasswordHasher>();
            container.RegisterType<IRequestCache, RequestCache>();
            container.RegisterType<IAuditLogService, EmailAuditLogService>();
            container.RegisterType<IEmailService, LinkoExchangeEmailService>(new InjectionConstructor(typeof(LinkoExchangeContext),
                typeof(EmailAuditLogService), 
                typeof(IProgramService),
                typeof(ISettingService),
                typeof(IRequestCache)));


            container.RegisterType<CommonInfoAttribute>();

            //var config = new MapperConfiguration(cfg =>
            //{
            //    cfg.AddProfile(new UserMapProfile());
            //});
            //container.RegisterInstance<IMapper>(config.CreateMapper());
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile(new UserMapProfile());
                cfg.AddProfile(new EmailAuditLogEntryMapProfile()); 
                cfg.AddProfile(new InvitationMapProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramUserDtoMapProfile());
                cfg.AddProfile(new OrganizationMapProfile());
                cfg.AddProfile(new OrganizationRegulatoryProgramMapProfile());
                cfg.AddProfile(new RegulatoryProgramMapperProfile());
                cfg.AddProfile(new PermissionGroupMapProfile());
                cfg.AddProfile(new SettingMapProfile());
            });

            //Make sure there no methods were missing in the mappings loaded above via profiles
            Mapper.AssertConfigurationIsValid();

            container.RegisterInstance<IMapper>(Mapper.Instance);

        }
    }
}
