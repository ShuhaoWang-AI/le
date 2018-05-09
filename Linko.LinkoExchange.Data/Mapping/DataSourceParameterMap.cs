using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class DataSourceParameterMap : EntityTypeConfiguration<DataSourceParameter>
    {
        #region constructors and destructor

        public DataSourceParameterMap()
        {
            ToTable(tableName: "tDataSourceParameter");

            HasKey(x => x.DataSourceParameterId);

            Property(x => x.DataSourceTerm).IsRequired().HasMaxLength(value: 254);

            Property(x => x.DataSourceId).IsRequired();

            Property(x => x.DataSourceParameterId).IsRequired();
        }

        #endregion
    }
}