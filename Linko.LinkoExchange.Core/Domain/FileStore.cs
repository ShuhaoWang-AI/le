using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a file.
    /// </summary>
    public class FileStore
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int FileStoreId { get; set; }

        /// <summary>
        ///     Unique within a particular OrganizationRegulatoryProgramId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        ///     Original name as given by the uploader.
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        ///     File size in bytes.
        /// </summary>
        public double SizeByte { get; set; }

        /// <summary>
        ///     MIME Type/Internet Meda Type.
        /// </summary>
        public string MediaType { get; set; }

        public int FileTypeId { get; set; }
        public virtual FileType FileType { get; set; }

        public int ReportElementTypeId { get; set; }

        /// <summary>
        ///     Denormalized data.
        /// </summary>
        public string ReportElementTypeName { get; set; }

        /// <summary>
        ///     Typical value: Industry Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }

        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public DateTimeOffset UploadDateTimeUtc { get; set; }

        public int UploaderUserId { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        // Reverse navigation
        public virtual FileStoreData FileStoreData { get; set; }

        public virtual ICollection<ReportFile> ReportFiles { get; set; }

        #endregion
    }
}