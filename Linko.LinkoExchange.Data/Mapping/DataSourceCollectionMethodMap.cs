using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class DataSourceCollectionMethodMap : EntityTypeConfiguration<DataSourceCollectionMethod>
    {
        #region constructors and destructor

        public DataSourceCollectionMethodMap()
        {
            ToTable(tableName: "tDataSourceCollectionMethod");

            HasKey(x => x.DataSourceCollectionMethodId);

            Property(x => x.DataSourceTerm).IsRequired().HasMaxLength(value: 254);

            Property(x => x.DataSourceId).IsRequired();

            Property(x => x.DataSourceCollectionMethodId).IsRequired();
        }

        #endregion
    }
}