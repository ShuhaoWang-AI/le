using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a TermCondition
    /// </summary>
    public partial class TermCondition
    {
        /// <summary>
        /// Primary key
        /// </summary>
        public int TermConditionId { get; set; }
        public string Content { get; set; }
        public DateTimeOffset CreationDateTimeUtc;
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }
    }
}