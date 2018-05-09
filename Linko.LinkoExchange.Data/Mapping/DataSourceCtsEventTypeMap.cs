using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class DataSourceCtsEventTypeMap : EntityTypeConfiguration<DataSourceCtsEventType>
    {
        #region constructors and destructor

        public DataSourceCtsEventTypeMap()
        {
            ToTable(tableName: "tDataSourceCtsEventType");

            HasKey(x => x.DataSourceCtsEventTypeId);

            Property(x => x.DataSourceTerm).IsRequired().HasMaxLength(value: 254);

            Property(x => x.DataSourceId).IsRequired();

            Property(x => x.DataSourceCtsEventTypeId).IsRequired();
        }

        #endregion
    }
}