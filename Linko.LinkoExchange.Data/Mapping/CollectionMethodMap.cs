using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class CollectionMethodMap : EntityTypeConfiguration<CollectionMethod>
    {
        public CollectionMethodMap()
        {
            ToTable("tCollectionMethod");

            HasKey(x => x.CollectionMethodId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            HasRequired(a => a.Organization)
                .WithMany(b => b.CollectionMethods)
                .HasForeignKey(c => c.OrganizationId)
                .WillCascadeOnDelete(false);

            Property(x => x.IsEnabled).IsRequired();

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}