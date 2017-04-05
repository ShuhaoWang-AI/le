﻿using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class FileStoreMap : EntityTypeConfiguration<FileStore>
    {
        public FileStoreMap()
        {
            ToTable("tFileStore");

            HasKey(t => t.FileStoreId);
            Property(t => t.Name).IsRequired();
            Property(t => t.Description);
            Property(t => t.OriginalName).IsRequired();
            Property(t => t.SizeByte).IsRequired();
            Property(t => t.OrganizationRegulatoryProgramId).IsRequired();
            Property(t => t.UploadDateTimeUtc);
            Property(t => t.UploaderUserId).IsRequired();

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany()
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(false);

            HasRequired(t => t.FileStoreData)
                .WithRequiredPrincipal(fd => fd.FileStore);

            HasRequired(t => t.FileType)
                .WithMany()
                .HasForeignKey(i => i.FileTypeId);
        }
    }
}
