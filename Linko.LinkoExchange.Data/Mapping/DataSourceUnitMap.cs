using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class DataSourceUnitMap : EntityTypeConfiguration<DataSourceUnit>
    {
        #region constructors and destructor

        public DataSourceUnitMap()
        {
            ToTable(tableName:"tDataSourceUnit");

            HasKey(x => x.DataSourceUnitId);

            Property(x => x.DataSourceTerm).IsRequired().HasMaxLength(value:254);

            HasRequired(a => a.DataSource)
                .WithMany(b => b.DataSourceUnits)
                .HasForeignKey(c => c.DataSourceId)
                .WillCascadeOnDelete(value:false);

            HasRequired(x => x.Unit)
                .WithMany()
                .HasForeignKey(x => x.UnitId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}