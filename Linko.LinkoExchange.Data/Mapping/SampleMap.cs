using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class SampleMap : EntityTypeConfiguration<Sample>
    {
        public SampleMap()
        {
            ToTable("tSample");

            HasKey(x => x.SampleId);

            Property(x => x.Name).IsRequired().HasMaxLength(50);

            Property(x => x.MonitoringPointId).IsRequired();

            Property(x => x.MonitoringPointName).IsRequired().HasMaxLength(50);

            Property(x => x.CtsEventTypeId).IsRequired();

            Property(x => x.CtsEventTypeName).IsRequired().HasMaxLength(50);

            Property(x => x.CtsEventCategoryName).IsRequired().HasMaxLength(50);

            Property(x => x.CollectionMethodId).IsRequired();

            Property(x => x.CollectionMethodName).IsRequired().HasMaxLength(50);

            Property(x => x.LabSampleIdentifier).IsOptional().HasMaxLength(50);

            Property(x => x.StartDateTimeUtc).IsRequired();

            Property(x => x.EndDateTimeUtc).IsRequired();

            Property(x => x.IsSystemGenerated).IsRequired();

            Property(x => x.IsReadyToReport).IsRequired();

            Property(x => x.FlowUnitValidValues).IsOptional().HasMaxLength(50);

            Property(x => x.ResultQualifierValidValues).IsOptional().HasMaxLength(50);

            Property(x => x.MassLoadingConversionFactorPounds).IsOptional();

            Property(x => x.MassLoadingCalculationDecimalPlaces).IsOptional();

            Property(x => x.IsMassLoadingResultToUseLessThanSign).IsRequired();

            HasRequired(a => a.ByOrganizationRegulatoryProgram)
                .WithMany(b => b.SampledBySamples)
                .HasForeignKey(c => c.ByOrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.ForOrganizationRegulatoryProgram)
                .WithMany(b => b.SampledForSamples)
                .HasForeignKey(c => c.ForOrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}