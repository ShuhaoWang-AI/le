using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class CollectionMethodMap : EntityTypeConfiguration<CollectionMethod>
    {
        #region constructors and destructor

        public CollectionMethodMap()
        {
            ToTable(tableName:"tCollectionMethod");

            HasKey(x => x.CollectionMethodId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:50);

            Property(x => x.Description).IsOptional().HasMaxLength(value:500);

            HasRequired(a => a.Organization)
                .WithMany(b => b.CollectionMethods)
                .HasForeignKey(c => c.OrganizationId)
                .WillCascadeOnDelete(value:false);

            HasRequired(a => a.CollectionMethodType)
                .WithMany()
                .HasForeignKey(c => c.CollectionMethodTypeId)
                .WillCascadeOnDelete(value:false);

            Property(x => x.IsEnabled).IsRequired();

            Property(x => x.IsRemoved).IsRequired();

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}