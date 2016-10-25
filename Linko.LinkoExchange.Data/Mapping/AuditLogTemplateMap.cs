using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
 
    class AuditLogTemplateMap : EntityTypeConfiguration<AuditLogTemplate>
    {
        public AuditLogTemplateMap()
        {
            ToTable("tAuditLogTemplate");

            HasKey(i => i.AuditLogTemplateId);

            Property(i => i.Name).HasMaxLength(100);

            Property(i => i.TemplateType).HasMaxLength(30); 

            Property(i => i.EventCategory).HasMaxLength(20);

            Property(i => i.EventType).HasMaxLength(50);

            Property(i => i.SubjectTemplate).HasMaxLength(500); 

            Property(i => i.MessageTemplate).IsRequired();

            Property(i => i.CreationDateTimeUtc).IsRequired();

            Property(i => i.LastModificationDateTimeUtc).IsOptional();

            Property(i => i.LastModifierUserId).IsOptional();
         }
    }
}
