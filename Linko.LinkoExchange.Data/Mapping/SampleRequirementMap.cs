﻿using Linko.LinkoExchange.Core.Domain;
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

            Property(x => x.PeriodStartDateTime).IsRequired();

            Property(x => x.PeriodEndDateTime).IsRequired();

            Property(x => x.SamplesRequired).IsRequired();

            HasRequired(a => a.ByOrganizationRegulatoryProgram)
                .WithMany(b => b.SampleRequirements)
                .HasForeignKey(c => c.ByOrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);
        }
    }
}
