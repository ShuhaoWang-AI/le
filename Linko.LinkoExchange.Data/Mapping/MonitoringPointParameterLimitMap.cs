using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            Property(x => x.Name).IsRequired().HasMaxLength(254);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.MinimumValue).IsOptional();

            Property(x => x.MaximumValue).IsOptional();

            HasRequired(a => a.BaseUnit)
                .WithMany(b => b.MonitoringPointParameterLimits)
                .HasForeignKey(c => c.BaseUnitId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.CollectionMethod)
                .WithMany(b => b.MonitoringPointParameterLimits)
                .HasForeignKey(c => c.CollectionMethodId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.LimitType)
                .WithMany(b => b.MonitoringPointParameterLimits_LimitType)
                .HasForeignKey(c => c.LimitTypeId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.LimitBasis)
                .WithMany(b => b.MonitoringPointParameterLimits_LimitBasis)
                .HasForeignKey(c => c.LimitBasisId)
                .WillCascadeOnDelete(false);

            Property(x => x.IsAlertsOnly).IsOptional();

        }
    }
}
