using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class PermissionGroupTemplatePermissionMap : EntityTypeConfiguration<PermissionGroupTemplatePermission>
    {
        public PermissionGroupTemplatePermissionMap()
        {
            ToTable("tPermissionGroupTemplatePermission");

            HasKey(x => x.PermissionGroupTemplatePermissionId);

            HasRequired(a => a.PermissionGroupTemplate)
                .WithMany(b => b.PermissionGroupTemplatePermissions)
                .HasForeignKey(c => c.PermissionGroupTemplateId)
                .WillCascadeOnDelete(false);

            HasRequired(a => a.Permission)
                .WithMany()
                .HasForeignKey(c => c.PermissionId)
                .WillCascadeOnDelete(false);
        }
    }
}