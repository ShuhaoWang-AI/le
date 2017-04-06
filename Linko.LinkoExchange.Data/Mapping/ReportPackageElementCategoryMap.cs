using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageElementCategoryMap : EntityTypeConfiguration<ReportPackageElementCategory>
    {
        public ReportPackageElementCategoryMap()
        {
            ToTable("tReportPackageElementCategory");

            HasKey(x => x.ReportPackageElementCategoryId);

            HasRequired(a => a.ReportPackage)
                .WithMany(b => b.ReportPackageElementCategories)
                .HasForeignKey(c => c.ReportPackageId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.ReportElementCategory)
                .WithMany()
                .HasForeignKey(c => c.ReportElementCategoryId)
                .WillCascadeOnDelete(false);

            Property(x => x.SortOrder).IsRequired();
        }
    }
}