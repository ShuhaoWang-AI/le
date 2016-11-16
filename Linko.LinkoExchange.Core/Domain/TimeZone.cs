using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a time zone.
    /// </summary>
    public partial class TimeZone
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int TimeZoneId { get; set; }

        public string Abbreviation { get; set; }

        public string Name { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}