using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageTemplateAssignmentMap : EntityTypeConfiguration<ReportPackageTemplateAssignment>
    {
        public ReportPackageTemplateAssignmentMap()
        {
            ToTable("tReportPackageTemplateAssignment");
            HasKey(x => x.ReportPackageTemplateAssignmentId);
            Property(x => x.ReportPackageTemplateId);
            Property(x => x.OrganizationRegulatoryProgramId);
        }
    }
}