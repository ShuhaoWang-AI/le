namespace Linko.LinkoExchange.Services
{
    public class CopyOfRecordDto
    {
        public int CopyOfRecordId { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public string Signature { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string Hash { get; set; }
        public string HashAlgorithm { get; set; }
        public byte[] Data { get; set; }
    }
}