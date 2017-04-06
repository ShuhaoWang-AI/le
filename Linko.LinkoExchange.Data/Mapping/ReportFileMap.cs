using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportFileMap : EntityTypeConfiguration<ReportFile>
    {
        public ReportFileMap()
        {
            ToTable("tReportFile");

            HasKey(x => x.ReportFileId);

            HasRequired(a => a.ReportPackageElementType)
                .WithMany(b => b.ReportFiles)
                .HasForeignKey(c => c.ReportPackageElementTypeId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.FileStore)
                .WithMany(b => b.ReportFiles)
                .HasForeignKey(c => c.FileStoreId)
                .WillCascadeOnDelete(false);
        }
    }
}