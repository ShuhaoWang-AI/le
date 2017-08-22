using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageElementCategoryMap : EntityTypeConfiguration<ReportPackageElementCategory>
    {
        #region constructors and destructor

        public ReportPackageElementCategoryMap()
        {
            ToTable(tableName:"tReportPackageElementCategory");

            HasKey(x => x.ReportPackageElementCategoryId);

            HasRequired(a => a.ReportPackage)
                .WithMany(b => b.ReportPackageElementCategories)
                .HasForeignKey(c => c.ReportPackageId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.ReportElementCategory)
                .WithMany()
                .HasForeignKey(c => c.ReportElementCategoryId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.SortOrder).IsRequired();
        }

        #endregion
    }
}