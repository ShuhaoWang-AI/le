using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a Collection Method Type.
    ///     The collection methods the users define must be identified as belonging to one of these types.
    /// </summary>
    public class CollectionMethodType
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int CollectionMethodTypeId { get; set; }

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