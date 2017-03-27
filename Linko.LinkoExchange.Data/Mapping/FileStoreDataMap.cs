using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class FileStoreDataMap : EntityTypeConfiguration<FileStoreData>
    {
        public FileStoreDataMap()
        {
            ToTable("tFileStoreData");

            Property(t => t.FileStoreDataId).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Data).IsRequired();
        }
    }
}