using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportSampleMap : EntityTypeConfiguration<ReportSample>
    {
        public ReportSampleMap()
        {
            ToTable("tReportSample");

            HasKey(x => x.ReportSampleId);

            HasRequired(a => a.ReportPackageElementType)
                .WithMany(b => b.ReportSamples)
                .HasForeignKey(c => c.ReportPackageElementTypeId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.Sample)
                .WithMany(b => b.ReportSamples)
                .HasForeignKey(c => c.SampleId)
                .WillCascadeOnDelete(false);
        }
    }
}