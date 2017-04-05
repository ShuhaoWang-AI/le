using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class FileTypeMap : EntityTypeConfiguration<FileType>
    {
        public FileTypeMap()
        {
            ToTable("tFileType");
            HasKey(t => t.FileTypeId);
            Property(t => t.Extension);
            Property(t => t.Description);
            Property(t => t.CreationDateTimeUtc);
            Property(t => t.LastModifierUserId);
            Property(t => t.LastModificationDateTimeUtc);
        }
    }
}
