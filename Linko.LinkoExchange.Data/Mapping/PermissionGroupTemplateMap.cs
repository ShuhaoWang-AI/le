using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class PermissionGroupTemplateMap : EntityTypeConfiguration<PermissionGroupTemplate>
    {
        #region constructors and destructor

        public PermissionGroupTemplateMap()
        {
            ToTable(tableName:"tPermissionGroupTemplate");

            HasKey(x => x.PermissionGroupTemplateId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            HasRequired(a => a.OrganizationTypeRegulatoryProgram)
                .WithMany(b => b.PermissionGroupTemplates)
                .HasForeignKey(c => c.OrganizationTypeRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}