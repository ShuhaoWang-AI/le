using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageTemplateElementTypeMap : EntityTypeConfiguration<ReportPackageTemplateElementType>
    {
        public ReportPackageTemplateElementTypeMap()
        {
            ToTable("tReportPackageTemplateElementType");

            HasKey(x => x.ReportPackageTemplateElementTypeId);

            HasRequired(a => a.ReportPackageTemplateElementCategory)
                .WithMany(b => b.ReportPackageTemplateElementTypes)
                .HasForeignKey(c => c.ReportPackageTemplateElementCategoryId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.ReportElementType)
                .WithMany()
                .HasForeignKey(c => c.ReportElementTypeId)
                .WillCascadeOnDelete(false);

            Property(x => x.IsRequired).IsRequired();

            Property(x => x.SortOrder).IsRequired();
        }
    }
}