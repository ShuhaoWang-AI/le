using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class UserPasswordHistoryMap : EntityTypeConfiguration<UserPasswordHistory>
    {
        public UserPasswordHistoryMap()
        {
            ToTable("tUserPasswordHistory"); 
             
            HasKey(i => i.UserPasswordHistoryId); 

            Property(i => i.PasswordHash).HasMaxLength(value: 100);
            Property(i => i.LastModificationDateTimeUtc);
            Property(i => i.UserProfileId);
        } 
    } 
}
