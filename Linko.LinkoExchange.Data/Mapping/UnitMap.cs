using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class UnitMap : EntityTypeConfiguration<Unit>
    {
        public UnitMap()
        {
            ToTable("tUnit");

            HasKey(x => x.UnitId);

            Property(x => x.Name).IsRequired().HasMaxLength(50);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.IsFlowUnit).IsRequired();

            HasRequired(a => a.Organization)
                .WithMany(b => b.Units)
                .HasForeignKey(c => c.OrganizationId)
                .WillCascadeOnDelete(false);

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}