using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportElementCategoryMap : EntityTypeConfiguration<ReportElementCategory>
    {
        public ReportElementCategoryMap()
        {
            ToTable("tReportElementCategory");
            HasKey(x => x.ReportElementCategoryId);
            Property(x => x.Name);
            Property(x => x.Description);
            Property(x => x.CreationDateTimeUtc);
            Property(x => x.LastModificationDateTimeUtc);
            Property(x => x.LastModifierUserId);
        }
    }
}