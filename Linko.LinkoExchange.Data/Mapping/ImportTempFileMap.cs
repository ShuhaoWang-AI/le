using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class ImportTempFileMap : EntityTypeConfiguration<ImportTempFile>
    {
        #region constructors and destructor

        public ImportTempFileMap()
        {
            ToTable(tableName:"tImportTempFile");

            HasKey(t => t.ImportTempFileId);

            Property(t => t.OriginalName).IsRequired().HasMaxLength(value:256);

            Property(t => t.SizeByte).IsRequired();

            Property(t => t.MediaType).IsOptional().HasMaxLength(value:100);

            HasRequired(a => a.FileType)
                .WithMany()
                .HasForeignKey(c => c.FileTypeId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.ImportTempFiles)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(t => t.UploadDateTimeUtc).IsRequired();

            Property(t => t.UploaderUserId).IsRequired();

            Property(t => t.RawFile).IsRequired();
        }

        #endregion
    }
}