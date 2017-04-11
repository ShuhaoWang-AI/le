using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{

    //TODO: to add more, 
    // Here is just for Cor usage
    public class ReportPackageDto
    {
        public int ReportPackageId { get; set; }
        public string Name { get; set; }
        public DateTime SubMissionDateTime { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgramDto { get; set; }
        public IEnumerable<FileStoreDto> AttachmentFiles { get; set; }
        public CopyOfRecordPdfFileDto CopyOfRecordPdfInfo { get; set; }
        public CopyOfRecordDataXmlFileDto CopyOfRecordDataXmlFileInfo { get; set; }
    }
}