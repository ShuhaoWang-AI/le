using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class FileStoreDataMap : EntityTypeConfiguration<FileStoreData>
    {
        public FileStoreDataMap()
        {
            ToTable("tFileStoreData");

            HasKey(t => t.FileStoreId);

            Property(t => t.Data).IsRequired();

        }
    }
}