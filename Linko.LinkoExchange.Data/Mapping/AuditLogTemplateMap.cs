using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{

    public class AuditLogTemplateMap : EntityTypeConfiguration<AuditLogTemplate>
    {
        public AuditLogTemplateMap()
        {
            ToTable("tAuditLogTemplate");

            HasKey(x => x.AuditLogTemplateId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.TemplateType).IsRequired().HasMaxLength(15);

            Property(x => x.EventCategory).IsRequired().HasMaxLength(30);

            Property(x => x.EventType).IsRequired().HasMaxLength(50);

            Property(x => x.SubjectTemplate).IsOptional().HasMaxLength(500);

            Property(x => x.MessageTemplate).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }

}