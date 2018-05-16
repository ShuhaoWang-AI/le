using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class DataSourceMonitoringPointMap : EntityTypeConfiguration<DataSourceMonitoringPoint>
    {
        #region constructors and destructor

        public DataSourceMonitoringPointMap()
        {
            ToTable(tableName:"tDataSourceMonitoringPoint");

            HasKey(x => x.DataSourceMonitoringPointId);

            Property(x => x.DataSourceTerm).IsRequired().HasMaxLength(value:254);

            HasRequired(a => a.DataSource)
                .WithMany(b => b.DataSourceMonitoringPoints)
                .HasForeignKey(c => c.DataSourceId)
                .WillCascadeOnDelete(value:false);

            HasRequired(x => x.MonitoringPoint)
                .WithMany()
                .HasForeignKey(x => x.MonitoringPointId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}