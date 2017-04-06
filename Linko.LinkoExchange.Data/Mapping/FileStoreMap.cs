using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class FileStoreMap : EntityTypeConfiguration<FileStore>
    {
        public FileStoreMap()
        {
            ToTable("tFileStore");

            HasKey(t => t.FileStoreId);

            Property(t => t.Name).IsRequired().HasMaxLength(256);

            Property(t => t.Description).IsOptional().HasMaxLength(500);

            Property(t => t.OriginalName).IsRequired().HasMaxLength(256);

            Property(t => t.SizeByte).IsRequired();

            Property(t => t.MediaType).IsOptional().HasMaxLength(100);

            HasRequired(a => a.FileType)
                .WithMany()
                .HasForeignKey(c => c.FileTypeId)
                .WillCascadeOnDelete(false);

            Property(t => t.ReportElementTypeId).IsRequired();

            Property(t => t.ReportElementTypeName).IsRequired().HasMaxLength(100);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.FileStores)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            Property(t => t.UploadDateTimeUtc).IsRequired();

            Property(t => t.UploaderUserId).IsRequired();

            Property(t => t.LastModificationDateTimeUtc).IsOptional();

            Property(t => t.LastModifierUserId).IsOptional();

            HasRequired(t => t.FileStoreData)
                .WithRequiredPrincipal(fd => fd.FileStore);
        }
    }
}
