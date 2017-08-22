using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class PermissionGroupPermissionMap : EntityTypeConfiguration<PermissionGroupPermission>
    {
        #region constructors and destructor

        public PermissionGroupPermissionMap()
        {
            ToTable(tableName:"tPermissionGroupPermission");

            HasKey(x => x.PermissionGroupPermissionId);

            HasRequired(a => a.PermissionGroup)
                .WithMany(b => b.PermissionGroupPermissions)
                .HasForeignKey(c => c.PermissionGroupId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.Permission)
                .WithMany()
                .HasForeignKey(c => c.PermissionId)
                .WillCascadeOnDelete(value:false);
        }

        #endregion
    }
}