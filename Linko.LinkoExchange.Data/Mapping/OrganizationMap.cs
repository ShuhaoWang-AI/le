using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class OrganizationMap : EntityTypeConfiguration<Organization>
    {
        #region constructors and destructor

        public OrganizationMap()
        {
            ToTable(tableName:"tOrganization");

            HasKey(x => x.OrganizationId);

            HasRequired(a => a.OrganizationType)
                .WithMany()
                .HasForeignKey(c => c.OrganizationTypeId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.Name).IsRequired().HasMaxLength(value:254);

            Property(x => x.AddressLine1).IsOptional().HasMaxLength(value:100);

            Property(x => x.AddressLine2).IsOptional().HasMaxLength(value:100);

            Property(x => x.CityName).IsOptional().HasMaxLength(value:100);

            Property(x => x.ZipCode).IsOptional().HasMaxLength(value:50);

            HasOptional(a => a.Jurisdiction)
                .WithMany()
                .HasForeignKey(c => c.JurisdictionId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.PhoneNumber).IsOptional().HasMaxLength(value:25);

            Property(x => x.PhoneExt).IsOptional();

            Property(x => x.FaxNumber).IsOptional().HasMaxLength(value:25);

            Property(x => x.WebsiteUrl).IsOptional().HasMaxLength(value:256);

            Property(x => x.Signer).IsOptional().HasMaxLength(value:250);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}