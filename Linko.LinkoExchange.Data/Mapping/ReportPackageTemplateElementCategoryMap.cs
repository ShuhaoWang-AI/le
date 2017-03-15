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
        }
    }
}