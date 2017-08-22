using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a type of a limit.
    /// </summary>
    public class LimitType
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int LimitTypeId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        #endregion
    }
}