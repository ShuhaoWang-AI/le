using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class PermissionGroupPermissionMap : EntityTypeConfiguration<PermissionGroupPermission>
    {
        public PermissionGroupPermissionMap()
        {
            ToTable("tPermissionGroupPermission");

            HasKey(x => x.PermissionGroupPermissionId);

            HasRequired(a => a.PermissionGroup)
                .WithMany(b => b.PermissionGroupPermissions)
                .HasForeignKey(c => c.PermissionGroupId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.Permission)
                .WithMany()
                .HasForeignKey(c => c.PermissionId)
                .WillCascadeOnDelete(false);
        }
    }
}