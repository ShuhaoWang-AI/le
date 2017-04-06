using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class SampleStatusMap : EntityTypeConfiguration<SampleStatus>
    {
        public SampleStatusMap()
        {
            ToTable("tSampleStatus");

            HasKey(x => x.SampleStatusId);

            Property(x => x.Name).IsRequired().HasMaxLength(254);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}