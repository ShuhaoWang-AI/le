using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageTemplateElementTypemap : EntityTypeConfiguration<ReportPackageTemplateElementType>
    {
        public ReportPackageTemplateElementTypemap()
        {
            ToTable("tReportPackageTemplateElementType");
            HasKey(x => x.ReportPackageTemplateElementTypeId);
            Property(x => x.ReportPackageTemplateElementCategoryId);
            Property(x => x.ReportElementTypeId);
            Property(x => x.SortOrder);
        }
    }
}