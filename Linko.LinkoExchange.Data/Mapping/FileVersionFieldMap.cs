using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class FileVersionFieldMap : EntityTypeConfiguration<FileVersionField>
    {
        #region constructors and destructor

        public FileVersionFieldMap()
        {
            ToTable(tableName:"tFileVersionField");

            HasKey(x => x.FileVersionFieldId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:50);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            HasRequired(a => a.FileVersion)
                .WithMany(b => b.FileVersionFields)
                .HasForeignKey(c => c.FileVersionId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.SystemField)
                .WithMany()
                .HasForeignKey(b => b.SystemFieldId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.DataOptionality)
                .WithMany()
                .HasForeignKey(b => b.DataOptionalityId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.Size).IsOptional();

            Property(x => x.ExampleData).IsOptional().HasMaxLength(value:500);

            Property(x => x.AdditionalComments).IsOptional().HasMaxLength(value:500);
        }

        #endregion
    }
}