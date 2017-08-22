using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a Report Element Category within a Report Package or Report Package template.
    /// </summary>
    public class ReportElementCategory
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int ReportElementCategoryId { get; set; }

        /// <summary>
        ///     Unique.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        #endregion
    }
}