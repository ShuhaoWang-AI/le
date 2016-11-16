using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class UserPasswordHistoryMap : EntityTypeConfiguration<UserPasswordHistory>
    {
        public UserPasswordHistoryMap()
        {
            ToTable("tUserPasswordHistory");

            HasKey(x => x.UserPasswordHistoryId);

            Property(x => x.PasswordHash).IsRequired();

            Property(x => x.UserProfileId).IsRequired();
            //HasRequired(a => a.UserProfile)
            //    .WithMany()
            //    .HasForeignKey(c => c.UserProfileId)
            //    .WillCascadeOnDelete(false);

            Property(x => x.LastModificationDateTimeUtc).IsRequired();
        }
    }
}