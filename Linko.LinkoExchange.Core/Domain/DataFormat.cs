using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents Data Format.
    /// </summary>
    public class DataFormat
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int DataFormatId { get; set; }

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