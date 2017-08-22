using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class PermissionGroupTemplatePermissionMap : EntityTypeConfiguration<PermissionGroupTemplatePermission>
    {
        #region constructors and destructor

        public PermissionGroupTemplatePermissionMap()
        {
            ToTable(tableName:"tPermissionGroupTemplatePermission");

            HasKey(x => x.PermissionGroupTemplatePermissionId);

            HasRequired(a => a.PermissionGroupTemplate)
                .WithMany(b => b.PermissionGroupTemplatePermissions)
                .HasForeignKey(c => c.PermissionGroupTemplateId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.Permission)
                .WithMany()
                .HasForeignKey(c => c.PermissionId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}