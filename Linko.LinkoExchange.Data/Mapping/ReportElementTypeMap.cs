using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportElementTypeMap : EntityTypeConfiguration<ReportElementType>
    {
        public ReportElementTypeMap()
        {
            ToTable("tReportElementType");
            HasKey(x => x.ReportElementTypeId);
            Property(x => x.Name);
            Property(x => x.Description);
            Property(x => x.Content);
            Property(x => x.CtsEventTypeId);
            Property(x => x.ReportElementCategoryId);
            Property(x => x.OrganizationRegulatoryProgramId);
            Property(x => x.CreationDateTimeUtc);
            Property(x => x.LastModificationDateTimeUtc);
            Property(x => x.LastModifierUserId);
        }
    }
}