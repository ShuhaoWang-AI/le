using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageTemplateElementCategoryMap : EntityTypeConfiguration<ReportPackageTemplateElementCategory>
    {
        public ReportPackageTemplateElementCategoryMap()
        {
            ToTable("tReportPackageTemplateElementCategory");
            HasKey(x => x.ReportPackageTemplateElementCategoryId);
            Property(x => x.ReportPackageTemplateId);
            Property(x => x.ReportElementCategoryId);
            Property(x => x.SortOrder);

            HasRequired(a => a.ReportPackageTemplate)
                .WithMany(b => b.ReportPackageTemplateElementCategories)
                .HasForeignKey(c => c.ReportPackageTemplateId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.ReportElementCategory)
                .WithMany()
                .HasForeignKey(c => c.ReportElementCategoryId)
                .WillCascadeOnDelete(false);
        }
    }
}