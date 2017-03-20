using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class ReportPackageTemplateMap : EntityTypeConfiguration<ReportPackageTemplate>
    {
        public ReportPackageTemplateMap()
        {
            ToTable("tReportPackageTemplate");

            HasKey(x => x.ReportPackageTemplateId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.EffectiveDateTimeUtc).IsRequired();

            Property(x => x.RetirementDateTimeUtc).IsOptional();

            Property(x => x.IsSubmissionBySignatoryRequired).IsRequired();

            HasOptional(a => a.CtsEventType)
                .WithMany()
                .HasForeignKey(c => c.CtsEventTypeId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.ReportPackageTemplates)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.IsActive).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}