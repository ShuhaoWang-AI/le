namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a Copy of Record (COR).
    /// </summary>
    public partial class CopyOfRecord
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int CopyOfRecordId { get; set; }

        /// <summary>
        /// Typical value: Industry Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public int ReportPackageId { get; set; }
        public virtual ReportPackage ReportPackage { get; set; }

        public string Signature { get; set; }
        public string SignatureAlgorithm { get; set; }

        public string Hash { get; set; }
        public string HashAlgorithm { get; set; }
        public byte[] Data { get; set; }
        public int CopyOfRecordCertificateId { get; set; }
        public virtual CopyOfRecordCertificate CopyOfRecordCertificate { get; set; }
    }
}

