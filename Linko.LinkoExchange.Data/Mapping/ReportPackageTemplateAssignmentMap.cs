using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageTemplateAssignmentMap : EntityTypeConfiguration<ReportPackageTemplateAssignment>
    {
        #region constructors and destructor

        public ReportPackageTemplateAssignmentMap()
        {
            ToTable(tableName:"tReportPackageTemplateAssignment");

            HasKey(x => x.ReportPackageTemplateAssignmentId);

            HasRequired(a => a.ReportPackageTemplate)
                .WithMany(b => b.ReportPackageTemplateAssignments)
                .HasForeignKey(c => c.ReportPackageTemplateId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.ReportPackageTemplateAssignments)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}