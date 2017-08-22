using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class LimitTypeMap : EntityTypeConfiguration<LimitType>
    {
        #region constructors and destructor

        public LimitTypeMap()
        {
            ToTable(tableName:"tLimitType");

            HasKey(x => x.LimitTypeId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:25);

            Property(x => x.Description).IsOptional().HasMaxLength(value:30);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}