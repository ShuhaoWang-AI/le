using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class FileVersionTemplateMap : EntityTypeConfiguration<FileVersionTemplate>
    {
        #region constructors and destructor

        public FileVersionTemplateMap()
        {
            ToTable(tableName:"tFileVersionTemplate");

            HasKey(x => x.FileVersionTemplateId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:50);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}