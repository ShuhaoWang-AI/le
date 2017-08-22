using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class LimitBasisMap : EntityTypeConfiguration<LimitBasis>
    {
        #region constructors and destructor

        public LimitBasisMap()
        {
            ToTable(tableName:"tLimitBasis");

            HasKey(x => x.LimitBasisId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:25);

            Property(x => x.Description).IsOptional().HasMaxLength(value:30);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}