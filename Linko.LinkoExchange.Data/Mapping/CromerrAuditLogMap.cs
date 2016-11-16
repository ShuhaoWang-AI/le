using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class CromerrAuditLogMap : EntityTypeConfiguration<CromerrAuditLog>
    {
        public CromerrAuditLogMap()
        {
            ToTable("tCromerrAuditLog");

            HasKey(x => x.CromerrAuditLogId);

            HasRequired(a => a.AuditLogTemplate)
                .WithMany()
                .HasForeignKey(c => c.AuditLogTemplateId)
                .WillCascadeOnDelete(false);

            Property(x => x.RegulatoryProgramId).IsOptional();

            Property(x => x.OrganizationId).IsOptional();

            Property(x => x.RegulatorOrganizationId).IsOptional();

            Property(x => x.UserName).IsRequired().HasMaxLength(256);

            Property(x => x.UserFirstName).IsOptional().HasMaxLength(50);

            Property(x => x.UserLastName).IsOptional().HasMaxLength(50);

            Property(x => x.UserEmailAddress).IsOptional().HasMaxLength(256);

            Property(x => x.IPAddress).IsOptional().HasMaxLength(50);

            Property(x => x.HostName).IsOptional().HasMaxLength(256);

            Property(x => x.Comment).IsRequired();

            Property(x => x.LogDateTimeUtc).IsRequired();
        }
    }
}