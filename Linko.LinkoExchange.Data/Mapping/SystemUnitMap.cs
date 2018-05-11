using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SystemUnitMap : EntityTypeConfiguration<SystemUnit>
    {
        #region constructors and destructor

        public SystemUnitMap()
        {
            ToTable(tableName:"tSystemUnit");

            HasKey(x => x.SystemUnitId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:50);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);
            

            Property(x => x.UnitDimensionId).IsRequired();

            HasRequired(a => a.UnitDimension)
                .WithMany(b => b.SystemUnits)
                .HasForeignKey(c => c.UnitDimensionId)
                .WillCascadeOnDelete(value:false);

            
            Property(x => x.ConversionFactor).IsRequired();
            
            Property(x => x.AdditiveFactor).IsRequired();


            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}