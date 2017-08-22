using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SampleFrequencyMap : EntityTypeConfiguration<SampleFrequency>
    {
        #region constructors and destructor

        public SampleFrequencyMap()
        {
            ToTable(tableName:"tSampleFrequency");

            HasKey(x => x.SampleFrequencyId);

            HasRequired(a => a.MonitoringPointParameter)
                .WithMany(b => b.SampleFrequencies)
                .HasForeignKey(c => c.MonitoringPointParameterId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.CollectionMethod)
                .WithMany()
                .HasForeignKey(c => c.CollectionMethodId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IUSampleFrequency).IsOptional().HasMaxLength(value:50);

            Property(x => x.AuthoritySampleFrequency).IsOptional().HasMaxLength(value:50);
        }

        #endregion
    }
}