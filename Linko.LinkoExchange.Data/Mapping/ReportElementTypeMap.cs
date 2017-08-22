using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportElementTypeMap : EntityTypeConfiguration<ReportElementType>
    {
        #region constructors and destructor

        public ReportElementTypeMap()
        {
            ToTable(tableName:"tReportElementType");

            HasKey(x => x.ReportElementTypeId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            Property(x => x.Content).IsOptional().HasMaxLength(value:2000);

            Property(x => x.IsContentProvided).IsRequired();

            HasOptional(a => a.CtsEventType)
                .WithMany()
                .HasForeignKey(c => c.CtsEventTypeId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.ReportElementCategory)
                .WithMany()
                .HasForeignKey(c => c.ReportElementCategoryId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.ReportElementTypes)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}