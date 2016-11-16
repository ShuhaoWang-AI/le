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
            if(transaction!=null)
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