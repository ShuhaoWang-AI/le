namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a Copy of Record (COR).
    /// </summary>
    public class CopyOfRecord
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int ReportPackageId { get; set; }

        public virtual ReportPackage ReportPackage { get; set; }

        public string Signature { get; set; }

        /// <summary>
        ///     Encryption algorithm used in the creation of the digital signature.
        /// </summary>
        public string SignatureAlgorithm { get; set; }

        public string Hash { get; set; }

        /// <summary>
        ///     Hashing algorithm used in the creation of the data hash.
        /// </summary>
        public string HashAlgorithm { get; set; }

        public byte[] Data { get; set; }

        public int CopyOfRecordCertificateId { get; set; }
        public virtual CopyOfRecordCertificate CopyOfRecordCertificate { get; set; }

        #endregion
    }
}