using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class CromerrAuditLogMap : EntityTypeConfiguration<CromerrAuditLog>
    {
        #region constructors and destructor

        public CromerrAuditLogMap()
        {
            ToTable(tableName:"tCromerrAuditLog");

            HasKey(x => x.CromerrAuditLogId);

            HasRequired(a => a.AuditLogTemplate)
                .WithMany()
                .HasForeignKey(c => c.AuditLogTemplateId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.RegulatoryProgramId).IsOptional();

            Property(x => x.OrganizationId).IsOptional();

            Property(x => x.RegulatorOrganizationId).IsOptional();

            Property(x => x.UserName).IsRequired().HasMaxLength(value:256);

            Property(x => x.UserFirstName).IsOptional().HasMaxLength(value:50);

            Property(x => x.UserLastName).IsOptional().HasMaxLength(value:50);

            Property(x => x.UserEmailAddress).IsOptional().HasMaxLength(value:256);

            Property(x => x.IPAddress).IsOptional().HasMaxLength(value:50);

            Property(x => x.HostName).IsOptional().HasMaxLength(value:256);

            Property(x => x.Comment).IsRequired();

            Property(x => x.LogDateTimeUtc).IsRequired();
        }

        #endregion
    }
}