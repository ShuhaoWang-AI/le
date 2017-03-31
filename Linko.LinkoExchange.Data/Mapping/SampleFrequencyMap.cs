using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                 .WithMany(b => b.SampleFrequencies)
                 .HasForeignKey(c => c.CollectionMethodId)
                 .WillCascadeOnDelete(false);

            Property(x => x.IUSampleFrequency).IsOptional();

            Property(x => x.AuthoritySampleFrequency).IsOptional();

        }
    }
}
