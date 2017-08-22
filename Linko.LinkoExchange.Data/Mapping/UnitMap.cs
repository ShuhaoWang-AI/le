using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class UnitMap : EntityTypeConfiguration<Unit>
    {
        #region constructors and destructor

        public UnitMap()
        {
            ToTable(tableName:"tUnit");

            HasKey(x => x.UnitId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:50);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            Property(x => x.IsFlowUnit).IsRequired();

            HasRequired(a => a.Organization)
                .WithMany(b => b.Units)
                .HasForeignKey(c => c.OrganizationId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}