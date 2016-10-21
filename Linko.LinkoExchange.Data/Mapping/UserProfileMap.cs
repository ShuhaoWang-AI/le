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
            HasKey(i => i.UserProfileId);
            Property(i => i.FirstName);
            Property(i => i.LastName);
            Property(i => i.Email);
            Property(i => i.IsEmailConfirmed).HasColumnName("EmailConfirmed");
            Property(i => i.IsAccountLocked);
            Property(i => i.IsPhoneNumberConfirmed).HasColumnName("PhoneNumberConfirmed");
            Property(i => i.OldEmailAddress);
            Property(i => i.PasswordHash);
        }
    }
}
