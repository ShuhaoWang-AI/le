using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SampleRequirementMap : EntityTypeConfiguration<SampleRequirement>
    {
        #region constructors and destructor

        public SampleRequirementMap()
        {
            ToTable(tableName:"tSampleRequirement");

            HasKey(x => x.SampleRequirementId);

            HasRequired(a => a.MonitoringPointParameter)
                .WithMany(b => b.SampleRequirements)
                .HasForeignKey(c => c.MonitoringPointParameterId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.PeriodStartDateTime).IsRequired();

            Property(x => x.PeriodEndDateTime).IsRequired();

            Property(x => x.SamplesRequired).IsRequired();

            HasRequired(a => a.ByOrganizationRegulatoryProgram)
                .WithMany(b => b.SampleRequirements)
                .HasForeignKey(c => c.ByOrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}