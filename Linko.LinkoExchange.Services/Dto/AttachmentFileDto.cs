using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class AttachmentFileDto
    {
        public int? FileId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OriginalName { get; set; }
        public long SizeByte { get; set; }
        public int FileDataId { get; set; }
        public byte[] Data { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public bool IsReported { get; set; }
        public DateTimeOffset UploaDateTimeUtc { get; set; }
        public int UploaderUserId { get; set; }
    }
}