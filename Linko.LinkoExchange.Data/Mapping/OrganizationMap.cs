using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationMap : EntityTypeConfiguration<Organization>
    {
        public OrganizationMap()
        {
            ToTable("tOrganization");

            HasKey(x => x.OrganizationId);

            HasRequired(a => a.OrganizationType)
                .WithMany()
                .HasForeignKey(c => c.OrganizationTypeId)
                .WillCascadeOnDelete(false);

            Property(x => x.Name).IsRequired().HasMaxLength(254);

            Property(x => x.AddressLine1).IsRequired().HasMaxLength(100);

            Property(x => x.AddressLine2).IsOptional().HasMaxLength(100);

            Property(x => x.CityName).IsRequired().HasMaxLength(100);

            Property(x => x.ZipCode).IsRequired().HasMaxLength(50);

            HasRequired(a => a.Jurisdiction)
                .WithMany()
                .HasForeignKey(c => c.JurisdictionId)
                .WillCascadeOnDelete(false);

            Property(x => x.PhoneNumber).IsOptional().HasMaxLength(25);

            Property(x => x.PhoneExt).IsOptional();

            Property(x => x.FaxNumber).IsOptional().HasMaxLength(25);

            Property(x => x.WebsiteUrl).IsOptional().HasMaxLength(256);

            Property(x => x.Signer).IsOptional().HasMaxLength(250);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}