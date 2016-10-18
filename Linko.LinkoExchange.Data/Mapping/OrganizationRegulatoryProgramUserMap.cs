using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    class OrganizationRegulatoryProgramUserMap: EntityTypeConfiguration<OrganizationRegulatoryProgramUser>
    {
        public OrganizationRegulatoryProgramUserMap()
        {
            ToTable("tOrganizationRegulatoryProgramUser");

            HasKey(i => i.OrganizationRegulatoryProgramUserId);  
            Property(i => i.UserProfileId);
            Property(i => i.OrganizationRegulatoryProgramId);
            Property(i => i.PermissionGrouopId); 
            Property(i => i.RegistrationDateTime);
            Property(i => i.IsRegistrationApproved);
            Property(i => i.IsRegistrationDenied);
            Property(i => i.IsRemoved);
            Property(i => i.IsSignatory);
            Property(i => i.IsEnabled);
            Property(i => i.LastModificationDateTime);
            Property(i => i.CreationDateTimeUtc);
            Property(i => i.LastModificationUserId); 
        }
    }
}
