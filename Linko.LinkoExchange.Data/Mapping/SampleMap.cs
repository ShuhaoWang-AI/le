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

            Property(x => x.Name).IsRequired().HasMaxLength(254);

            Property(x => x.MonitoringPointId).IsRequired();

            Property(x => x.MonitoringPointName).IsRequired();

            Property(x => x.CtsEventTypeId).IsRequired();

            Property(x => x.CtsEventTypeName).IsRequired();

            Property(x => x.CtsEventCategoryName).IsRequired();

            Property(x => x.CollectionMethodId).IsRequired();

            Property(x => x.CollectionMethodName).IsRequired();

            Property(x => x.LabSampleIdentifier).IsOptional();

            Property(x => x.StartDateTimeUtc).IsRequired();

            Property(x => x.EndDateTimeUtc).IsRequired();

            Property(x => x.IsCalculated).IsRequired();

            HasRequired(a => a.SampleStatus)
                .WithMany(b => b.Samples)
                .HasForeignKey(c => c.SampleStatusId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.OrganizationType)
                .WithMany(b => b.Samples)
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