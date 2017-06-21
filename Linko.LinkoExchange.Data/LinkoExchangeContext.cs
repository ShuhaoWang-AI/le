using System;
using System.Data.Entity;
using Linko.LinkoExchange.Core.Domain;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Reflection;
using System.Linq;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data
{
    public class LinkoExchangeContext : IdentityDbContext<UserProfile>
    {
        #region constructor

        public LinkoExchangeContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            // Disable database initialization when the DB is not found
            Database.SetInitializer<LinkoExchangeContext>(null);
        }

        #endregion


        #region DbSets

        public DbSet<AuditLogTemplate> AuditLogTemplates { get; set; }
        public DbSet<CromerrAuditLog> CromerrAuditLogs { get; set; }
        public DbSet<EmailAuditLog> EmailAuditLogs { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Jurisdiction> Jurisdictions { get; set; }
        public DbSet<Core.Domain.Module> Modules { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationRegulatoryProgram> OrganizationRegulatoryPrograms { get; set; }
        public DbSet<OrganizationRegulatoryProgramModule> OrganizationRegulatoryProgramModules { get; set; }
        public DbSet<OrganizationRegulatoryProgramSetting> OrganizationRegulatoryProgramSettings { get; set; }
        public DbSet<OrganizationRegulatoryProgramUser> OrganizationRegulatoryProgramUsers { get; set; }
        public DbSet<OrganizationSetting> OrganizationSettings { get; set; }
        public DbSet<OrganizationType> OrganizationTypes { get; set; }
        public DbSet<OrganizationTypeRegulatoryProgram> OrganizationTypeRegulatoryPrograms { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<PermissionGroup> PermissionGroups { get; set; }
        public DbSet<PermissionGroupPermission> PermissionGroupPermissions { get; set; }
        public DbSet<PermissionGroupTemplate> PermissionGroupTemplates { get; set; }
        public DbSet<PermissionGroupTemplatePermission> PermissionGroupTemplatePermissions { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionType> QuestionTypes { get; set; }
        public DbSet<RegulatoryProgram> RegulatoryPrograms { get; set; }
        public DbSet<SettingTemplate> SettingTemplates { get; set; }
        public DbSet<SignatoryRequest> SignatoryRequests { get; set; }
        public DbSet<SignatoryRequestStatus> SignatoryRequestStatuses { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<Core.Domain.TimeZone> TimeZones { get; set; }
        public DbSet<UserPasswordHistory> UserPasswordHistories { get; set; }
        public DbSet<UserQuestionAnswer> UserQuestionAnswers { get; set; }

        public DbSet<CtsEventType> CtsEventTypes { get; set; }
        public DbSet<Parameter> Parameters { get; set; }
        public DbSet<MonitoringPoint> MonitoringPoints { get; set; }
        public DbSet<MonitoringPointParameterLimit> MonitoringPointParameterLimits { get; set; }
        public DbSet<SampleFrequency> SampleFrequencies { get; set; }
        public DbSet<SampleRequirement> SampleRequirements { get; set; }
        public DbSet<MonitoringPointParameter> MonitoringPointParameters { get; set; }
        public DbSet<CollectionMethod> CollectionMethods { get; set; }
        public DbSet<Unit> Units { get; set; }

        public DbSet<CollectionMethodType> CollectionMethodTypes { get; set; }
        public DbSet<LimitBasis> LimitBases { get; set; }
        public DbSet<LimitType> LimitTypes { get; set; }

        public DbSet<ParameterGroup> ParameterGroups { get; set; }
        public DbSet<ParameterGroupParameter> ParameterGroupParameters { get; set; }

        public DbSet<FileType> FileTypes { get; set; }
        public DbSet<FileStore> FileStores { get; set; }
        public DbSet<FileStoreData> FileStoreDatas { get; set; }

        public DbSet<CopyOfRecordCertificate> CopyOfRecordCertificates { get; set; }
        public DbSet<CopyOfRecord> CopyOfRecords { get; set; }

        public DbSet<Sample> Samples { get; set; }
        public DbSet<SampleResult> SampleResults { get; set; }

        public DbSet<ReportElementType> ReportElementTypes { get; set; }
        public DbSet<ReportElementCategory> ReportElementCategories { get; set; }

        public DbSet<ReportPackageTemplateAssignment> ReportPackageTemplateAssignments { get; set; }
        public DbSet<ReportPackageTemplate> ReportPackageTempates { get; set; }
        public DbSet<ReportPackageTemplateElementType> ReportPackageTemplateElementTypes { get; set; }
        public DbSet<ReportPackageTemplateElementCategory> ReportPackageTemplateElementCategories { get; set; }

        public DbSet<RepudiationReason> RepudiationReasons { get; set; }
        public DbSet<ReportStatus> ReportStatuses { get; set; }
        public DbSet<ReportPackage> ReportPackages { get; set; }
        public DbSet<ReportPackageElementCategory> ReportPackageElementCategories { get; set; }
        public DbSet<ReportPackageElementType> ReportPackageElementTypes { get; set; }
        public DbSet<ReportFile> ReportFiles { get; set; }
        public DbSet<ReportSample> ReportSamples { get; set; }
        public DbSet<TermCondition> TermConditions { get; set; }
        #endregion


        #region utilities

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var typesToRegister = Assembly.GetExecutingAssembly().GetTypes()
                                                                .Where(type => !String.IsNullOrEmpty(type.Namespace))
                                                                .Where(type => type.BaseType != null && type.BaseType.IsGenericType &&
                                                                               type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));
            foreach (var type in typesToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.Configurations.Add(configurationInstance);
            }

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserProfile>().ToTable("tUserProfile");
        }

        public virtual DbContextTransaction BeginTransaction()
        {
            return Database.BeginTransaction();
        }

        public void Commit(DbContextTransaction transaction)
        {
            if (transaction != null)
                transaction.Commit();
        }

        public void Rollback(DbContextTransaction transaction)
        {
            if (transaction != null)
                transaction.Commit();
        }

        #endregion
    }
}