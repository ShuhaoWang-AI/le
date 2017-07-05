using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class MonitoringPointParameterMap : EntityTypeConfiguration<MonitoringPointParameter>
    {
        public MonitoringPointParameterMap()
        {
            ToTable("tMonitoringPointParameter");

            HasKey(x => x.MonitoringPointParameterId);

            HasRequired(a => a.MonitoringPoint)
                .WithMany(b => b.MonitoringPointParameters)
                .HasForeignKey(c => c.MonitoringPointId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.Parameter)
                .WithMany(b => b.MonitoringPointParameters)
                .HasForeignKey(c => c.ParameterId)
                .WillCascadeOnDelete(false);

            HasOptional(a => a.DefaultUnit)
              .WithMany()
              .HasForeignKey(c => c.DefaultUnitId)
              .WillCascadeOnDelete(false);

            Property(x => x.EffectiveDateTime).IsOptional();

            Property(x => x.RetirementDateTime).IsOptional();
        }
    }
}
