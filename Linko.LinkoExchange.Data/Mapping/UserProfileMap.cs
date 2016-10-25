using Linko.LinkoExchange.Core.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class UserProfileMap : EntityTypeConfiguration<UserProfile>
    {
        public UserProfileMap()
        {
            ToTable("tUserProfile");
            HasKey(i => i.Id);

            Property(i => i.UserProfileId);
            Property(i => i.FirstName);
            Property(i => i.LastName);
            Property(i => i.Email); 
            Property(i => i.IsAccountLocked); 
            Property(i => i.OldEmailAddress);
            Property(i => i.PasswordHash);
            Property(i => i.AddressLine1);
            Property(i => i.AddressLine2);
            Property(i => i.CityName);
            Property(i => i.ZipCode);
            Property(i => i.IsInternalAccount);
            Property(i => i.IsIdentityProofed);

            Ignore(i => i.CurrentOrganizationId);
            Ignore(i => i.CurrentOrgRegProgramId);
            Ignore(i => i.CurrentOrgRegProgUserId);

        }
    }
}
