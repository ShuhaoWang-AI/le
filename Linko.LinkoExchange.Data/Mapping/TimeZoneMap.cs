using System.Data.Entity.ModelConfiguration;
using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class TimeZoneMap : EntityTypeConfiguration<TimeZone>
    {
        #region constructors and destructor

        public TimeZoneMap()
        {
            ToTable(tableName:"tTimeZone");

            HasKey(x => x.TimeZoneId);

            Property(x => x.Name).IsRequired().HasMaxLength(value:100);

            Property(x => x.StandardAbbreviation).IsRequired().HasMaxLength(value:5);

            Property(x => x.DaylightAbbreviation).IsOptional().HasMaxLength(value:5);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }

        #endregion
    }
}