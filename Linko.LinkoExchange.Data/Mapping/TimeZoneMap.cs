using Linko.LinkoExchange.Core.Domain;
using System.Data.Entity.ModelConfiguration;

namespace Linko.LinkoExchange.Data.Mapping
{
    public class TimeZoneMap : EntityTypeConfiguration<TimeZone>
    {
        public TimeZoneMap()
        {
            ToTable("tTimeZone");

            HasKey(x => x.TimeZoneId);

            Property(x => x.Name).IsRequired().HasMaxLength(100);

            Property(x => x.StandardAbbreviation).IsRequired().HasMaxLength(5);

            Property(x => x.DaylightAbbreviation).IsOptional().HasMaxLength(5);

            Property(x => x.CreationDateTimeUtc).IsRequired();

            Property(x => x.LastModificationDateTimeUtc).IsOptional();

            Property(x => x.LastModifierUserId).IsOptional();
        }
    }
}