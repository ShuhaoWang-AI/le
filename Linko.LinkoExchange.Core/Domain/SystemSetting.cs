using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a global setting for the system.
    /// </summary>
    public partial class SystemSetting
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int SystemSettingId { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}