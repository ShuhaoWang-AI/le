using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class MonitoringPointMap : EntityTypeConfiguration<MonitoringPoint>
    {
        #region constructors and destructor

        public MonitoringPointMap()
        {
            ToTable(tableName:"tMonitoringPoint");

            HasKey(x => x.MonitoringPointId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:50);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.MonitoringPoints)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IsEnabled).IsRequired();

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}