using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SampleRequirementMap : EntityTypeConfiguration<SampleRequirement>
    {
        public SampleRequirementMap()
        {
            ToTable("tSampleRequirement");

            HasKey(x => x.SampleRequirementId);

            HasRequired(a => a.MonitoringPointParameter)
                 .WithMany(b => b.SampleRequirements)
                 .HasForeignKey(c => c.MonitoringPointParameterId)
                 .WillCascadeOnDelete(false);

            Property(x => x.PeriodStartDateTimeUtc).IsRequired();

            Property(x => x.PeriodEndDateTimeUtc).IsRequired();

            Property(x => x.SamplesRequired).IsRequired();

            Property(x => x.LimitEffectiveDateTimeUtc).IsRequired();

            Property(x => x.LimitRetirementDateTimeUtc).IsOptional();

            HasRequired(a => a.ByOrganizationRegulatoryProgram)
                .WithMany(b => b.SampleRequirements)
                .HasForeignKey(c => c.ByOrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);
        }
    }
}
