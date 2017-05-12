using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class LimitBasisMap : EntityTypeConfiguration<LimitBasis>
    {
        public LimitBasisMap()
        {
            ToTable("tLimitBasis");

            HasKey(x => x.LimitBasisId);

            Property(x => x.Name).IsRequired().HasMaxLength(25);

            Property(x => x.Description).IsOptional().HasMaxLength(30);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}
