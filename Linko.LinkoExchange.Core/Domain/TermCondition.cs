using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a TermCondition
    /// </summary>
    public class TermCondition
    {
        #region public properties

        /// <summary>
        ///     Primary key
        /// </summary>
        public int TermConditionId { get; set; }

        public string Content { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }

        #endregion
    }
}