using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a type of a file.
    /// Contains all accepted file types in the system.
    /// </summary>
    public partial class FileType
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int FileTypeId { get; set; }

        /// <summary>
        /// Unique.
        /// </summary>
        public string Extension { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}

