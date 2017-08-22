using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ReportElementCategoryMap : EntityTypeConfiguration<ReportElementCategory>
    {
        #region constructors and destructor

        public ReportElementCategoryMap()
        {
            ToTable(tableName:"tReportElementCategory");

            HasKey(x => x.ReportElementCategoryId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}