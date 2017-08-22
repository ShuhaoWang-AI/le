using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportFileMap : EntityTypeConfiguration<ReportFile>
    {
        #region constructors and destructor

        public ReportFileMap()
        {
            ToTable(tableName:"tReportFile");

            HasKey(x => x.ReportFileId);

            HasRequired(a => a.ReportPackageElementType)
                .WithMany(b => b.ReportFiles)
                .HasForeignKey(c => c.ReportPackageElementTypeId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.FileStore)
                .WithMany(b => b.ReportFiles)
                .HasForeignKey(c => c.FileStoreId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}