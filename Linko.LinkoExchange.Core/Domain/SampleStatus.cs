using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a sample status.
    /// </summary>
    public partial class SampleStatus
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int SampleStatusId { get; set; }

        /// <summary>
        /// Unique.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}

