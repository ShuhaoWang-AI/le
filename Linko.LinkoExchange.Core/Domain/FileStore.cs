using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a file.
    /// </summary>
    public partial class FileStore
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int FileStoreId { get; set; }

        /// <summary>
        /// Unique within a particular OrganizationRegulatoryProgramId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public string OriginalName { get; set; }

        /// <summary>
        /// File size in bytes.
        /// </summary>
        public double SizeByte { get; set; }

        public int ReportElementTypeId { get; set; }

        /// <summary>
        /// Denormalized data.
        /// </summary>
        public string ReportElementTypeName { get; set; }

        /// <summary>
        /// Typical value: Industry Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public bool IsReported { get; set; }

        public DateTimeOffset UploadDateTimeUtc { get; set; }

        public int? UploaderUserId { get; set; }


        // Reverse navigation
        public virtual ICollection<FileStoreData> FileStoreData { get; set; }
    }
}

