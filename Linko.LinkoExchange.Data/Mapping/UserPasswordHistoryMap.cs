using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class UserPasswordHistoryMap : EntityTypeConfiguration<UserPasswordHistory>
    {
        #region constructors and destructor

        public UserPasswordHistoryMap()
        {
            ToTable(tableName:"tUserPasswordHistory");

            HasKey(x => x.UserPasswordHistoryId);

            Property(x => x.PasswordHash).IsRequired();

            Property(x => x.UserProfileId).IsRequired();

            //HasRequired(a => a.UserProfile)
            //    .WithMany()
            //    .HasForeignKey(c => c.UserProfileId)
            //    .WillCascadeOnDelete(false);

            Property(x => x.LastModificationDateTimeUtc).IsRequired();
        }

        #endregion
    }
}