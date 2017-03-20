using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageTemplateAssignmentMap : EntityTypeConfiguration<ReportPackageTemplateAssignment>
    {
        public ReportPackageTemplateAssignmentMap()
        {
            ToTable("tReportPackageTemplateAssignment");

            HasKey(x => x.ReportPackageTemplateAssignmentId);

            HasRequired(a => a.ReportPackageTemplate)
                .WithMany(b => b.ReportPackageTemplateAssignments)
                .HasForeignKey(c => c.ReportPackageTemplateId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany()
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);
        }
    }
}