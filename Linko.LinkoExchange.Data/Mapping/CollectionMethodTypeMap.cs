using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class CollectionMethodTypeMap : EntityTypeConfiguration<CollectionMethodType>
    {
        public CollectionMethodTypeMap()
        {
            ToTable("tCollectionMethodType");

            HasKey(x => x.CollectionMethodTypeId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.Description).IsOptional().HasMaxLength(500);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}