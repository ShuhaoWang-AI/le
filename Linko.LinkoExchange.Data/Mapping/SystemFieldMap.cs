using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class SystemFieldMap : EntityTypeConfiguration<SystemField>
    {
        #region constructors and destructor

        public SystemFieldMap()
        {
            ToTable(tableName:"tSystemField");

            HasKey(x => x.SystemFieldId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:50);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            HasRequired(a => a.DataFormat)
                .WithMany()
                .HasForeignKey(b => b.DataFormatId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IsRequired).IsRequired();

            Property(x => x.Size).IsOptional();

            Property(x => x.ExampleData).IsOptional().HasMaxLength(value:500);

            Property(x => x.AdditionalComments).IsOptional().HasMaxLength(value:500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}