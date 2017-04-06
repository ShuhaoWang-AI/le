using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class MonitoringPointParameterLimitMap : EntityTypeConfiguration<MonitoringPointParameterLimit>
    {
        public MonitoringPointParameterLimitMap()
        {
            ToTable("tMonitoringPointParameterLimit");

            HasKey(x => x.MonitoringPointParameterLimitId);

            HasRequired(a => a.MonitoringPointParameter)
                .WithMany(b => b.MonitoringPointParameterLimits)
                .HasForeignKey(c => c.MonitoringPointParameterId)
                .WillCascadeOnDelete(false);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.MinimumValue).IsOptional();

            Property(x => x.MaximumValue).IsRequired();

            HasRequired(a => a.BaseUnit)
                .WithMany()
                .HasForeignKey(c => c.BaseUnitId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.CollectionMethodType)
                .WithMany()
                .HasForeignKey(c => c.CollectionMethodTypeId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.LimitType)
                .WithMany()
                .HasForeignKey(c => c.LimitTypeId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.LimitBasis)
                .WithMany()
                .HasForeignKey(c => c.LimitBasisId)
                .WillCascadeOnDelete(false);

            Property(x => x.IsAlertsOnly).IsOptional();

        }
    }
}
