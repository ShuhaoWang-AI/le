using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class MonitoringPointParameterLimitMap : EntityTypeConfiguration<MonitoringPointParameterLimit>
    {
        #region constructors and destructor

        public MonitoringPointParameterLimitMap()
        {
            ToTable(tableName:"tMonitoringPointParameterLimit");

            HasKey(x => x.MonitoringPointParameterLimitId);

            HasRequired(a => a.MonitoringPointParameter)
                .WithMany(b => b.MonitoringPointParameterLimits)
                .HasForeignKey(c => c.MonitoringPointParameterId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.Name).IsRequired().HasMaxLength(value:200);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            Property(x => x.MinimumValue).IsOptional();

            Property(x => x.MaximumValue).IsRequired();

            HasRequired(a => a.BaseUnit)
                .WithMany()
                .HasForeignKey(c => c.BaseUnitId)
                .WillCascadeOnDelete(value:false);

            HasOptional(a => a.CollectionMethodType)
                .WithMany()
                .HasForeignKey(c => c.CollectionMethodTypeId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.LimitType)
                .WithMany()
                .HasForeignKey(c => c.LimitTypeId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.LimitBasis)
                .WithMany()
                .HasForeignKey(c => c.LimitBasisId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IsAlertOnly).IsOptional();
        }

        #endregion
    }
}