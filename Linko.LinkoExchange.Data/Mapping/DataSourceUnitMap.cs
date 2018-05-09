using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class DataSourceUnitMap : EntityTypeConfiguration<DataSourceUnit>
    {
        #region constructors and destructor

        public DataSourceUnitMap()
        {
            ToTable(tableName: "tDataSourceUnit");

            HasKey(x => x.DataSourceUnitId);

            Property(x => x.DataSourceTerm).IsRequired().HasMaxLength(value: 254);

            Property(x => x.DataSourceId).IsRequired();

            Property(x => x.DataSourceUnitId).IsRequired();
        }

        #endregion
    }
}