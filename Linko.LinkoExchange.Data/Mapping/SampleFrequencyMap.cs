using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SampleFrequencyMap : EntityTypeConfiguration<SampleFrequency>
    {
        public SampleFrequencyMap()
        {
            ToTable("tSampleFrequency");

            HasKey(x => x.SampleFrequencyId);

            HasRequired(a => a.MonitoringPointParameter)
                 .WithMany(b => b.SampleFrequencies)
                 .HasForeignKey(c => c.MonitoringPointParameterId)
                 .WillCascadeOnDelete(false);

            HasRequired(a => a.CollectionMethod)
                 .WithMany()
                 .HasForeignKey(c => c.CollectionMethodId)
                 .WillCascadeOnDelete(false);

            Property(x => x.IUSampleFrequency).IsOptional().HasMaxLength(50);

            Property(x => x.AuthoritySampleFrequency).IsOptional().HasMaxLength(50);

        }
    }
}
