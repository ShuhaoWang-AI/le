﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class FileStoreMap : EntityTypeConfiguration<FileStore>
    {
        public FileStoreMap()
        {
            ToTable("tFileStore");

            Property(t => t.FileStoreId).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Name).IsRequired();
            Property(t => t.Description);
            Property(t => t.OriginalName).IsRequired();
            Property(t => t.SizeByte).IsRequired();
            Property(t => t.OrganizationRegulatoryProgramId).IsRequired();
            Property(t => t.IsReported).IsRequired();
            Property(t => t.UploadDateTimeUtc);
            Property(t => t.UploaderUserId).IsRequired();

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany()
                .HasForeignKey(c => c.OrganizationRegulatoryProgram)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.FileStoreData)
                .WithMany()
                .HasForeignKey(c => c.FileStoreData)
                .WillCascadeOnDelete(false);
        }
    }
}
