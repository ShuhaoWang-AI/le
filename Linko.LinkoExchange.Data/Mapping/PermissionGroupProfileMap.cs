using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class PermissionGroupProfileMap : EntityTypeConfiguration<PermissionGroup>
    {
        public PermissionGroupProfileMap()
        {
            ToTable("tPermissionGroup");
            HasKey(i => i.PermissionGroupId);
            Property(i => i.Name);
            Property(i => i.Description);
            Property(i => i.OrganizationRegulatoryProgramId);
            this.Property(i => i.OrganizationRegulatoryProgramId);
            this.HasRequired(i => i.OrganizationRegulatoryProgram)
                .WithMany()
                .HasForeignKey(i => i.OrganizationRegulatoryProgramId);
            Property(i => i.CreationDateTimeUtc);
            Property(i => i.LastModificationDateTimeUtc);
        }
    }
}
