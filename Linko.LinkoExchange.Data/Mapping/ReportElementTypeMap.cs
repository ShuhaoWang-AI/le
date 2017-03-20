using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public partial class ReportElementTypeMap : EntityTypeConfiguration<ReportElementType>
    {
        public ReportElementTypeMap()
        {
            ToTable("tReportElementType");

            HasKey(x => x.ReportElementTypeId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.Content).IsOptional().HasMaxLength(2000);

            Property(x => x.IsContentProvided).IsRequired();

            HasOptional(a => a.CtsEventType)
                .WithMany()
                .HasForeignKey(c => c.CtsEventTypeId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.ReportElementCategory)
                .WithMany()
                .HasForeignKey(c => c.ReportElementCategoryId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.ReportElementTypes)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}