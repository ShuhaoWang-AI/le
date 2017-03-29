using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class MonitoringPointMap : EntityTypeConfiguration<MonitoringPoint>
    {
        public MonitoringPointMap()
        {
            ToTable("tMonitoringPoint");

            HasKey(x => x.MonitoringPointId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.MonitoringPoints)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.IsEnabled).IsRequired();

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
    //public class MonitoringPointParameterMap : EntityTypeConfiguration<MonitoringPointParameter>
    //{
    //    public MonitoringPointParameterMap()
    //    {
    //        ToTable("tMonitoringPointParameter");

    //        HasKey(x => x.MonitoringPointParameterId);

    //        HasRequired(a => a.OrganizationRegulatoryProgram)
    //            .WithMany(b => b.MonitoringPointParameters)
    //            .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
    //            .WillCascadeOnDelete(false);

    //        HasRequired(a => a.MonitoringPoint)
    //            .WithMany(b => b.MonitoringPointParameters)
    //            .HasForeignKey(c => c.MonitoringPointId)
    //            .WillCascadeOnDelete(false);

    //        HasRequired(a => a.Parameter)
    //            .WithMany(b => b.MonitoringPointParameters)
    //            .HasForeignKey(c => c.ParameterId)
    //            .WillCascadeOnDelete(false);

    //        HasOptional(a => a.DefaultUnit)
    //          .WithMany()
    //          .HasForeignKey(c => c.DefaultUnitId)
    //          .WillCascadeOnDelete(false);

    //        Property(x => x.EffectiveDateTimeUtc).IsOptional();

    //        Property(x => x.RetireDateTimeUtc).IsOptional();

    //        Property(x => x.CreationDateTimeUtc).IsRequired();

    //        Property(x => x.LastModificationDateTimeUtc).IsOptional();

    //        Property(x => x.LastModifierUserId).IsOptional();
    //    }
    //}

    //public class MonitoringPointParameterLimitMap : EntityTypeConfiguration<MonitoringPointParameterLimit>
    //{
    //    public MonitoringPointParameterLimitMap()
    //    {
    //        ToTable("tMonitoringPointParameterLimit");

    //        HasKey(x => x.MonitoringPointParameterLimitId);

    //        HasRequired(a => a.MonitoringPointParameter)
    //            .WithMany(b => b.MonitoringPointParameterLimits)
    //            .HasForeignKey(c => c.MonitoringPointParameterId)
    //            .WillCascadeOnDelete(false);

    //        Property(x => x.Name).IsRequired();

    //        Property(x => x.Description).IsRequired();

    //        Property(x => x.MinimumValue).IsOptional();

    //        Property(x => x.MaximumValue).IsOptional();

    //        HasRequired(a => a.CollectionMethod)
    //            .WithMany(b => b.MonitoringPointParameterLimits)
    //            .HasForeignKey(c => c.CollectionMethodId)
    //            .WillCascadeOnDelete(false);

    //        HasRequired(a => a.LimitType)
    //            .WithMany(b => b.MonitoringPointParameterLimits_LimitType)
    //            .HasForeignKey(c => c.LimitTypeId)
    //            .WillCascadeOnDelete(false);

    //        HasRequired(a => a.LimitBasis)
    //            .WithMany(b => b.MonitoringPointParameterLimits_LimitBasis)
    //            .HasForeignKey(c => c.LimitBasisId)
    //            .WillCascadeOnDelete(false);

    //        Property(x => x.IsAlertOnly).IsOptional();
          
    //    }
    //}

}