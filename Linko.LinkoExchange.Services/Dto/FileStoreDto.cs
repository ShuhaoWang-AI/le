using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class FileStoreDto
    {
        public int? FileStoreId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OriginalName { get; set; }
        public double SizeByte { get; set; }
        public int ReportElementTypeId { get; set; }
        public string ReportElementTypeName { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgram { get; set; }
        public DateTimeOffset UploadDateTimeUtc { get; set; }
        public int? UploaderUserId { get; set; }
        public byte[] Data { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
    }
}