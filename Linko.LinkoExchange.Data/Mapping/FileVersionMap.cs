using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class FileVersionMap : EntityTypeConfiguration<FileVersion>
    {
        #region constructors and destructor

        public FileVersionMap()
        {
            ToTable(tableName:"tFileVersion");

            HasKey(x => x.FileVersionId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:50);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany()
                .HasForeignKey(b => b.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}