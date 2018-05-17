using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class FileVersionTemplateFieldMap : EntityTypeConfiguration<FileVersionTemplateField>
    {
        #region constructors and destructor

        public FileVersionTemplateFieldMap()
        {
            ToTable(tableName:"tFileVersionTemplateField");

            HasKey(x => x.FileVersionTemplateFieldId);

            HasRequired(a => a.FileVersionTemplate)
                .WithMany(b => b.FileVersionTemplateFields)
                .HasForeignKey(c => c.FileVersionTemplateId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.SystemField)
                .WithMany()
                .HasForeignKey(b => b.SystemFieldId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}