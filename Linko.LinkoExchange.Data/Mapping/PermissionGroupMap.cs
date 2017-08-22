using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class PermissionGroupMap : EntityTypeConfiguration<PermissionGroup>
    {
        #region constructors and destructor

        public PermissionGroupMap()
        {
            ToTable(tableName:"tPermissionGroup");

            HasKey(x => x.PermissionGroupId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            HasRequired(a => a.OrganizationRegulatoryProgram)
                .WithMany(b => b.PermissionGroups)
                .HasForeignKey(c => c.OrganizationRegulatoryProgramId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}