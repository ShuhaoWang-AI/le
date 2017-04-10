using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class FileStoreDto
    {
        public int? FileStoreId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OriginalFileName { get; set; }
        public double SizeByte { get; set; }
        public int FileTypeId { get; set; }
        public string FileType { get; set; }
        public int ReportElementTypeId { get; set; }
        public string ReportElementTypeName { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgram { get; set; }
        public DateTime UploalDateTimeLocal { get; set; }
        public int UploaderUserId { get; set; }
        public string UploaderUserFullName { get; set; }
        public DateTime? LastModificationDateTimeLocal { get; set; }
        public int? LastModifierUserId { get; set; }
        public string LastModifierUserFullName { get; set; }
        public byte[] Data { get; set; }
        public string MediaType { get; set; }
        public bool UsedByReports { get; set; }
    }
}