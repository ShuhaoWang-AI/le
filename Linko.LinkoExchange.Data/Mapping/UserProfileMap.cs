using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class UserProfileMap : EntityTypeConfiguration<UserProfile>
    {
        public UserProfileMap()
        {
            ToTable("tUserProfile");

            Property(x => x.UserProfileId).IsRequired().HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(x => x.FirstName).IsRequired().HasMaxLength(50);

            Property(x => x.LastName).IsRequired().HasMaxLength(50);

            Property(x => x.TitleRole).IsOptional().HasMaxLength(250);

            Property(x => x.BusinessName).IsRequired().HasMaxLength(100);

            Property(x => x.AddressLine1).IsRequired().HasMaxLength(100);

            Property(x => x.AddressLine2).IsOptional().HasMaxLength(100);

            Property(x => x.CityName).IsRequired().HasMaxLength(100);

            Property(x => x.ZipCode).IsRequired().HasMaxLength(50);

            HasRequired(a => a.Jurisdiction)
                .WithMany()
                .HasForeignKey(c => c.JurisdictionId)
                .WillCascadeOnDelete(false);

            Property(x => x.PhoneExt).IsOptional();

            Property(x => x.IsAccountLocked).IsRequired();

            Property(x => x.IsAccountResetRequired).IsRequired();

            Property(x => x.IsIdentityProofed).IsRequired();

            Property(x => x.IsInternalAccount).IsRequired();

            Property(x => x.OldEmailAddress).IsOptional().HasMaxLength(256);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();


            // The default Identity entity is optional
            Property(x => x.PhoneNumber).IsRequired();
        }
    }
}