using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class DataSourceMap : EntityTypeConfiguration<DataSource>
    {
        #region constructors and destructor

        public DataSourceMap()
        {
            ToTable(tableName:"tDataSource");

            HasKey(x => x.DataSourceId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            HasRequired(x => x.OrganizationRegulatoryProgram)
                .WithMany()
                .HasForeignKey(x => x.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}