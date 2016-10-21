using System.Data.Entity;
using Linko.LinkoExchange.Core.Domain;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Reflection;
using System.Linq;
using System;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data
{
    public class LinkoExchangeContext : IdentityDbContext<ApplicationUser>
    {
        #region constructor

        public LinkoExchangeContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        //public static LinkoExchangeContext Create()
        //{
        //    return new LinkoExchangeContext ();
        //}


        #endregion


        #region DBSets
        public DbSet<AuditLogTemplate> AuditLogTemplates { set; get; }
        public DbSet<EmailAuditLog> EmailAuditLog { set; get; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserPasswordHistory> UserPasswordHistories { get; set; }
        public DbSet<OrganizationRegulatoryProgramUser> OrganizationRegulatoryProgramUsers { get; set; }
        public DbSet<UserQuestionAnswer> UserQuestionAnswers { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionType> QuestionTypes { get; set; }
        public DbSet<PermissionGroup> PermissionGroups { get; set; }
        public DbSet<OrganizationRegulatoryProgram> OrganizationRegulatoryPrograms { get; set; }
        public DbSet<OrganizationRegulatoryProgramSetting> OrganizationRegulatoryProgramSettings { get; set; }
        public DbSet<OrganizationSetting> OrganizationSettings { get; set; }

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
        }
        #endregion
    }
}