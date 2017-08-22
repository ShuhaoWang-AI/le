using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class UserProfileMap : EntityTypeConfiguration<UserProfile>
    {
        #region constructors and destructor

        public UserProfileMap()
        {
            ToTable(tableName:"tUserProfile");

            Property(x => x.UserProfileId).IsRequired().HasDatabaseGeneratedOption(databaseGeneratedOption:DatabaseGeneratedOption.Identity);

            Property(x => x.FirstName).IsRequired().HasMaxLength(value:50);

            Property(x => x.LastName).IsRequired().HasMaxLength(value:50);

            Property(x => x.TitleRole).IsOptional().HasMaxLength(value:250);

            Property(x => x.BusinessName).IsRequired().HasMaxLength(value:100);

            Property(x => x.AddressLine1).IsRequired().HasMaxLength(value:100);

            Property(x => x.AddressLine2).IsOptional().HasMaxLength(value:100);

            Property(x => x.CityName).IsRequired().HasMaxLength(value:100);

            Property(x => x.ZipCode).IsRequired().HasMaxLength(value:50);

            HasRequired(a => a.Jurisdiction)
                .WithMany()
                .HasForeignKey(c => c.JurisdictionId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.PhoneExt).IsOptional();

            Property(x => x.IsAccountLocked).IsRequired();

            Property(x => x.IsAccountResetRequired).IsRequired();

            Property(x => x.IsIdentityProofed).IsRequired();

            Property(x => x.IsInternalAccount).IsRequired();

            Property(x => x.OldEmailAddress).IsOptional().HasMaxLength(value:256);

            Property(x => x.TermConditionId).IsRequired();

            Property(x => x.TermConditionAgreedDateTimeUtc).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            // The default Identity entity is optional
            Property(x => x.PhoneNumber).IsRequired();
        }

        #endregion
    }
}