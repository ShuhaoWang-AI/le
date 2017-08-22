using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a Copy of Record Certificate used in the creation of the digital signature.
    /// </summary>
    public class CopyOfRecordCertificate
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int CopyOfRecordCertificateId { get; set; }

        public string PhysicalPath { get; set; }

        public string FileName { get; set; }

        public string Password { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        #endregion
    }
}