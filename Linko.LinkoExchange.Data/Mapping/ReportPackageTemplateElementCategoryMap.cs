using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageTemplateElementCategoryMap : EntityTypeConfiguration<ReportPackageTemplateElementCategory>
    {
        public ReportPackageTemplateElementCategoryMap()
        {
            ToTable("tReportPackageTemplateElementCategory");

            HasKey(x => x.ReportPackageTemplateElementCategoryId);

            HasRequired(a => a.ReportPackageTemplate)
                .WithMany(b => b.ReportPackageTemplateElementCategories)
                .HasForeignKey(c => c.ReportPackageTemplateId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.ReportElementCategory)
                .WithMany()
                .HasForeignKey(c => c.ReportElementCategoryId)
                .WillCascadeOnDelete(false);

            Property(x => x.SortOrder).IsRequired();
        }
    }
}