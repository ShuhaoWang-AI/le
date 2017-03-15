using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageTemplateMap : EntityTypeConfiguration<ReportPackageTemplate>
    {
        public ReportPackageTemplateMap()
        {
            ToTable("tReportPackageTemplate");

            HasKey(x => x.ReportPackageTemplateId);
            Property(x => x.Name);
            Property(x => x.Description);
            Property(x => x.EffectiveDateTimeUtc);
            Property(x => x.RetirementDateTimeUtc);
            Property(x => x.CtsEventTypeId);
            Property(x => x.OrganizationRegulatoryProgramId);
            Property(x => x.IsActive);
            Property(x => x.CreationDateTimeUtc);
            Property(x => x.LastModificationDateTimeUtc);
            Property(x => x.LastModifierUserId);
        }
    }
}