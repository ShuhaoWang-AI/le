using System;
using System.Configuration;
using System.Web;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Logging;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Config;
using Linko.LinkoExchange.Services.CopyOfRecord;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.FileStore;
using Linko.LinkoExchange.Services.Invitation;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.MonitoringPoint;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Parameter;
using Linko.LinkoExchange.Services.Permission;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.Sync;
using Linko.LinkoExchange.Services.TermCondition;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.Unit;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Practices.Unity;
using NLog;

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
            container.RegisterType<LogAttribute>();
            container.RegisterType<CustomHandleErrorAttribute>(new InjectionConstructor(typeof(ILogger)));

            // Services
            //container.RegisterType<ApplicationSignInManager>(new InjectionFactory(c => HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>()));
            container.RegisterType<IAuditLogEntry, EmailAuditLogEntryDto>();
            container.RegisterType<IAuditLogService, EmailAuditLogService>();
            container.RegisterType<IAuthenticationManager>(new InjectionFactory(c => HttpContext.Current.GetOwinContext().Authentication));
            container.RegisterType<IAuthenticationService, AuthenticationService>();
            container.RegisterType<IConfigSettingService, ConfigSettingService>();
            container.RegisterType<ICopyOfRecordService, CopyOfRecordService>();
            container.RegisterType<ICromerrAuditLogService, CromerrAuditLogService>();
            container.RegisterType<IDigitalSignatureManager, CertificateDigitalSignatureManager>();
            container.RegisterType<IEmailService, LinkoExchangeEmailService>(new InjectionConstructor(typeof(LinkoExchangeContext), typeof(EmailAuditLogService), typeof(IProgramService), typeof(ISettingService), typeof(IRequestCache)));
            container.RegisterType<IEncryptionService, EncryptionService>();
            container.RegisterType<IFileStoreService, FileStoreService>();
            container.RegisterType<IHttpContextService, HttpContextService>();
            container.RegisterType<IInvitationService, InvitationService>();
            container.RegisterType<IJurisdictionService, JurisdictionService>();
            container.RegisterType<IMonitoringPointService, MonitoringPointService>();
            container.RegisterType<IOrganizationService, OrganizationService>();
            container.RegisterType<IParameterService, ParameterService>();
            container.RegisterType<IPasswordHasher, PasswordHasher>();
            container.RegisterType<IPermissionService, PermissionService>();
            container.RegisterType<IProgramService, ProgramService>();
            container.RegisterType<IQuestionAnswerService, QuestionAnswerService>();
            container.RegisterType<IReportElementService, ReportElementService>();
            container.RegisterType<IReportPackageService, ReportPackageService>();
            container.RegisterType<IReportTemplateService, ReportTemplateService>();
            container.RegisterType<IRequestCache, RequestCache>();
            container.RegisterType<ISampleService, SampleService>();
            container.RegisterType<ISessionCache, SessionCache>();
            container.RegisterType<ISettingService, SettingService>();
            container.RegisterType<IGlobalSettings, GlobalSettings>(new ContainerControlledLifetimeManager());
            container.RegisterType<ITimeZoneService, TimeZoneService>();
            container.RegisterType<IUnitService, UnitService>();
            container.RegisterType<IUserService, UserService>();
            container.RegisterType<ITermConditionService, TermConditionService>();
            container.RegisterType<ISyncService, SyncService>();
            container.RegisterType<IApplicationCache, ApplicationCache>();

            // Custom identity services           
            container.RegisterType<ApplicationSignInManager>();
            container.RegisterType<ApplicationUserManager>();
            container.RegisterType<IUserStore<UserProfile>, UserStore<UserProfile>>(new InjectionConstructor(typeof(LinkoExchangeContext)));

            //Map POCO <-> DTO
            container.RegisterType<Services.Mapping.IMapHelper, Services.Mapping.MapHelper>();

            //Map DTO <-> ViewModel
            container.RegisterType<Web.Mapping.IMapHelper, Web.Mapping.MapHelper>();
        }
    }
}
