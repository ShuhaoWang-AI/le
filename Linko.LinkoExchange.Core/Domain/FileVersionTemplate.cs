using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents File Version Template.
    /// </summary>
    public class FileVersionTemplate
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int FileVersionTemplateId { get; set; }

        /// <summary>
        ///     Unique.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
        
        // Reverse navigation
        public virtual ICollection<FileVersionTemplateField> FileVersionTemplateFields { get; set; }
        #endregion
    }
}