using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class MonitoringPointParameterMap : EntityTypeConfiguration<MonitoringPointParameter>
    {
        #region constructors and destructor

        public MonitoringPointParameterMap()
        {
            ToTable(tableName:"tMonitoringPointParameter");

            HasKey(x => x.MonitoringPointParameterId);

            HasRequired(a => a.MonitoringPoint)
                .WithMany(b => b.MonitoringPointParameters)
                .HasForeignKey(c => c.MonitoringPointId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.Parameter)
                .WithMany(b => b.MonitoringPointParameters)
                .HasForeignKey(c => c.ParameterId)
                .WillCascadeOnDelete(value:false);

            HasOptional(a => a.DefaultUnit)
                .WithMany()
                .HasForeignKey(c => c.DefaultUnitId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.EffectiveDateTime).IsOptional();

            Property(x => x.RetirementDateTime).IsOptional();
        }

        #endregion
    }
}