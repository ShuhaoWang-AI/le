using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a time zone.
    /// </summary>
    public class TimeZone
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int TimeZoneId { get; set; }

        public string Name { get; set; }

        public string StandardAbbreviation { get; set; }

        public string DaylightAbbreviation { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        #endregion
    }
}