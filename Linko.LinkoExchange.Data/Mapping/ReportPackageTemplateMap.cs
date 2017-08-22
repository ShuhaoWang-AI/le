using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageTemplateMap : EntityTypeConfiguration<ReportPackageTemplate>
    {
        #region constructors and destructor

        public ReportPackageTemplateMap()
        {
            ToTable(tableName:"tReportPackageTemplate");

            HasKey(x => x.ReportPackageTemplateId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            Property(x => x.EffectiveDateTimeUtc).IsRequired();

            Property(x => x.RetirementDateTimeUtc).IsOptional();

            Property(x => x.IsSubmissionBySignatoryRequired).IsRequired();

            HasOptional(a => a.CtsEventType)
                .WithMany()
                .HasForeignKey(c => c.CtsEventTypeId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.ReportPackageTemplates)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IsActive).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}