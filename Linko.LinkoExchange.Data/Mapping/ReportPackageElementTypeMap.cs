using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageElementTypeMap : EntityTypeConfiguration<ReportPackageElementType>
    {
        public ReportPackageElementTypeMap()
        {
            ToTable("tReportPackageElementType");

            HasKey(x => x.ReportPackageElementTypeId);

            HasRequired(a => a.ReportPackageElementCategory)
                .WithMany(b => b.ReportPackageElementTypes)
                .HasForeignKey(c => c.ReportPackageElementCategoryId)
                .WillCascadeOnDelete(false);

            Property(x => x.ReportElementTypeId).IsRequired();

            Property(x => x.ReportElementTypeName).IsRequired().HasMaxLength(100);

            Property(x => x.ReportElementTypeContent).IsOptional().HasMaxLength(2000);

            Property(x => x.ReportElementTypeIsContentProvided).IsRequired();

            Property(x => x.CtsEventTypeId).IsOptional();

            Property(x => x.CtsEventTypeName).IsOptional().HasMaxLength(100);

            Property(x => x.CtsEventCategoryName).IsOptional().HasMaxLength(100);

            Property(x => x.IsRequired).IsRequired();

            Property(x => x.SortOrder).IsRequired();
        }
    }
}