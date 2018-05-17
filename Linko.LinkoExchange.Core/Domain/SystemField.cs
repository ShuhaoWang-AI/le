using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents System Field.
    /// </summary>
    public class SystemField
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int SystemFieldId { get; set; }

        /// <summary>
        ///     Unique.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public int DataFormatId { get; set; }
        public virtual DataFormat DataFormat { get; set; }

        public bool IsRequired { get; set; }

        public int? Size { get; set; }

        public string ExampleData { get; set; }

        public string AdditionalComments { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        #endregion
    }
}