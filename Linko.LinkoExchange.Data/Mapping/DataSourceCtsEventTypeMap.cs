using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class DataSourceCtsEventTypeMap : EntityTypeConfiguration<DataSourceCtsEventType>
    {
        #region constructors and destructor

        public DataSourceCtsEventTypeMap()
        {
            ToTable(tableName:"tDataSourceCtsEventType");

            HasKey(x => x.DataSourceCtsEventTypeId);

            Property(x => x.DataSourceTerm).IsRequired().HasMaxLength(value:254);

            HasRequired(a => a.DataSource)
                .WithMany(b => b.DataSourceCtsEventTypes)
                .HasForeignKey(c => c.DataSourceId)
                .WillCascadeOnDelete(value:false);

            HasRequired(x => x.CtsEventType)
                .WithMany()
                .HasForeignKey(x => x.CtsEventTypeId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}