using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class CtsEventTypeMap : EntityTypeConfiguration<CtsEventType>
    {
        public CtsEventTypeMap()
        {
            ToTable("tCtsEventType");

            HasKey(x => x.CtsEventTypeId);

            Property(x => x.Name).IsRequired().HasMaxLength(50);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.CtsEventCategoryName).IsRequired().HasMaxLength(50);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.CtsEventTypes)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.IsEnabled).IsRequired();

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}