using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportPackageTemplateElementCategoryMap : EntityTypeConfiguration<ReportPackageTemplateElementCategory>
    {
        #region constructors and destructor

        public ReportPackageTemplateElementCategoryMap()
        {
            ToTable(tableName:"tReportPackageTemplateElementCategory");

            HasKey(x => x.ReportPackageTemplateElementCategoryId);

            HasRequired(a => a.ReportPackageTemplate)
                .WithMany(b => b.ReportPackageTemplateElementCategories)
                .HasForeignKey(c => c.ReportPackageTemplateId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.ReportElementCategory)
                .WithMany()
                .HasForeignKey(c => c.ReportElementCategoryId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.SortOrder).IsRequired();
        }

        #endregion
    }
}