using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageTemplateElementTypeMap : EntityTypeConfiguration<ReportPackageTemplateElementType>
    {
        #region constructors and destructor

        public ReportPackageTemplateElementTypeMap()
        {
            ToTable(tableName:"tReportPackageTemplateElementType");

            HasKey(x => x.ReportPackageTemplateElementTypeId);

            HasRequired(a => a.ReportPackageTemplateElementCategory)
                .WithMany(b => b.ReportPackageTemplateElementTypes)
                .HasForeignKey(c => c.ReportPackageTemplateElementCategoryId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.ReportElementType)
                .WithMany()
                .HasForeignKey(c => c.ReportElementTypeId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IsRequired).IsRequired();

            Property(x => x.SortOrder).IsRequired();
        }

        #endregion
    }
}