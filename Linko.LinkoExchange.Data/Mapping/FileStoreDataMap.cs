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

            HasKey(t => t.FileStoreId);
            Property(t => t.FileStoreId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Data).IsRequired();

            HasRequired(t => t.FileStore).WithRequiredPrincipal(a => a.FileStoreData);

        }
    }
}