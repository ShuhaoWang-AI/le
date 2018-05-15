using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class DataSourceParameterMap : EntityTypeConfiguration<DataSourceParameter>
    {
        #region constructors and destructor

        public DataSourceParameterMap()
        {
            ToTable(tableName:"tDataSourceParameter");

            HasKey(x => x.DataSourceParameterId);

            Property(x => x.DataSourceTerm).IsRequired().HasMaxLength(value:254);

            HasRequired(a => a.DataSource)
                .WithMany(b => b.DataSourceParameters)
                .HasForeignKey(c => c.DataSourceId)
                .WillCascadeOnDelete(value:false);

            HasRequired(x => x.Parameter)
                .WithMany()
                .HasForeignKey(x => x.ParameterId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}