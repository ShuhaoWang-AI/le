using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents what a limit is based on.
    /// </summary>
    public class LimitBasis
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int LimitBasisId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}
