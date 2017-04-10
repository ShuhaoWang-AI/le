namespace Linko.LinkoExchange.Services.Dto
{
    public class CopyOfRecordDto
    {
        public int ReportPackageId { get; set; }
        public string Signature { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string Hash { get; set; }
        public string HashAlgorithm { get; set; }
        public byte[] Data { get; set; }
        public int CopyOfRecordCertificateId { get; set; }
        public string DownloadFileName { get; set; }
    }
}