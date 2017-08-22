using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportSampleMap : EntityTypeConfiguration<ReportSample>
    {
        #region constructors and destructor

        public ReportSampleMap()
        {
            ToTable(tableName:"tReportSample");

            HasKey(x => x.ReportSampleId);

            HasRequired(a => a.ReportPackageElementType)
                .WithMany(b => b.ReportSamples)
                .HasForeignKey(c => c.ReportPackageElementTypeId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.Sample)
                .WithMany(b => b.ReportSamples)
                .HasForeignKey(c => c.SampleId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}