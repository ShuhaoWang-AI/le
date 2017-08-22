using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageElementTypeMap : EntityTypeConfiguration<ReportPackageElementType>
    {
        #region constructors and destructor

        public ReportPackageElementTypeMap()
        {
            ToTable(tableName:"tReportPackageElementType");

            HasKey(x => x.ReportPackageElementTypeId);

            HasRequired(a => a.ReportPackageElementCategory)
                .WithMany(b => b.ReportPackageElementTypes)
                .HasForeignKey(c => c.ReportPackageElementCategoryId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.ReportElementTypeId).IsRequired();

            Property(x => x.ReportElementTypeName).IsRequired().HasMaxLength(value:100);

            Property(x => x.ReportElementTypeContent).IsOptional().HasMaxLength(value:2000);

            Property(x => x.ReportElementTypeIsContentProvided).IsRequired();

            Property(x => x.CtsEventTypeId).IsOptional();

            Property(x => x.CtsEventTypeName).IsOptional().HasMaxLength(value:50);

            Property(x => x.CtsEventCategoryName).IsOptional().HasMaxLength(value:50);

            Property(x => x.IsRequired).IsRequired();

            Property(x => x.SortOrder).IsRequired();
        }

        #endregion
    }
}