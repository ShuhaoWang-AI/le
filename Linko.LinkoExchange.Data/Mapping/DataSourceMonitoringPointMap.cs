﻿using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class DataSourceMonitoringPointMap : EntityTypeConfiguration<DataSourceMonitoringPoint>
    {
        #region constructors and destructor

        public DataSourceMonitoringPointMap()
        {
            ToTable(tableName: "tDataSourceMonitoringPoint");

            HasKey(x => x.DataSourceMonitoringPointId);

            Property(x => x.DataSourceTerm).IsRequired().HasMaxLength(value: 254);

            Property(x => x.DataSourceId).IsRequired();

            HasRequired(x => x.MonitoringPoint)
                .WithMany()
                .HasForeignKey(x => x.MonitoringPoint)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}