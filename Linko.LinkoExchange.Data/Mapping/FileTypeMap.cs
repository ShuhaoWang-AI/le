using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class FileTypeMap : EntityTypeConfiguration<FileType>
    {
        #region constructors and destructor

        public FileTypeMap()
        {
            ToTable(tableName:"tFileType");

            HasKey(t => t.FileTypeId);

            Property(t => t.Extension).IsRequired().HasMaxLength(value:5);

            Property(t => t.Description).IsOptional().HasMaxLength(value:500);

            Property(t => t.CreationDateTimeUtc).IsRequired();

            Property(t => t.LastModifierUserId).IsOptional();

            Property(t => t.LastModificationDateTimeUtc).IsOptional();
        }

        #endregion
    }
}