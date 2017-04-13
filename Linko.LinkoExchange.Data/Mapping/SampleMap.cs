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

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.MonitoringPointId).IsRequired();

            Property(x => x.MonitoringPointName).IsRequired().HasMaxLength(100);

            Property(x => x.CtsEventTypeId).IsRequired();

            Property(x => x.CtsEventTypeName).IsRequired().HasMaxLength(100);

            Property(x => x.CtsEventCategoryName).IsRequired().HasMaxLength(100);

            Property(x => x.CollectionMethodId).IsRequired();

            Property(x => x.CollectionMethodName).IsRequired().HasMaxLength(100);

            Property(x => x.LabSampleIdentifier).IsOptional().HasMaxLength(50);

            Property(x => x.StartDateTimeUtc).IsRequired();

            Property(x => x.EndDateTimeUtc).IsRequired();

            Property(x => x.IsCalculated).IsRequired();

            Property(x => x.IsReadyToReport).IsRequired();

            HasRequired(a => a.OrganizationType)
                .WithMany()
                .HasForeignKey(c => c.OrganizationTypeId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.Samples)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}