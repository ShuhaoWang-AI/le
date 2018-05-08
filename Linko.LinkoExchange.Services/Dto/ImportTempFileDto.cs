using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ImportTempFileDto
    {
        #region public properties

        public int? ImportTempFileId { get; set; }
        public string OriginalFileName { get; set; }
        public double SizeByte { get; set; }
        public string MediaType { get; set; }
        public int FileTypeId { get; set; }
        public string FileExtension { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgram { get; set; }
        public DateTime UploadDateTimeLocal { get; set; }
        public int UploaderUserId { get; set; }
        public byte[] RawFile { get; set; }

        #endregion
    }
}