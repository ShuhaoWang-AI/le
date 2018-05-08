using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a file.
    /// </summary>
    public class ImportTempFile
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int ImportTempFileId { get; set; }

        /// <summary>
        ///     Original name as given by the uploader.
        /// </summary>
        public string OriginalName { get; set; }

        /// <summary>
        ///     File size in bytes.
        /// </summary>
        public double SizeByte { get; set; }

        /// <summary>
        ///     MIME Type/Internet Media Type.
        /// </summary>
        public string MediaType { get; set; }

        public int FileTypeId { get; set; }
        public virtual FileType FileType { get; set; }

        /// <summary>
        ///     Typical value: Industry Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }

        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public DateTimeOffset UploadDateTimeUtc { get; set; }

        public int UploaderUserId { get; set; }
        
        public byte[] RawFile { get; set; }

        #endregion
    }
}