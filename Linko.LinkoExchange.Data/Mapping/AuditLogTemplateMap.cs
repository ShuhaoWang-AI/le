using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class AuditLogTemplateMap : EntityTypeConfiguration<AuditLogTemplate>
    {
        #region constructors and destructor

        public AuditLogTemplateMap()
        {
            ToTable(tableName:"tAuditLogTemplate");

            HasKey(x => x.AuditLogTemplateId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.TemplateType).IsRequired().HasMaxLength(value:15);

            Property(x => x.EventCategory).IsRequired().HasMaxLength(value:30);

            Property(x => x.EventType).IsRequired().HasMaxLength(value:50);

            Property(x => x.SubjectTemplate).IsOptional().HasMaxLength(value:500);

            Property(x => x.MessageTemplate).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}