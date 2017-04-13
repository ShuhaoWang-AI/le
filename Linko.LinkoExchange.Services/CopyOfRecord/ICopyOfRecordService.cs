
using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.CopyOfRecord
{
    public interface ICopyOfRecordService
    {
        CopyOfRecordDto CreateCopyOfRecordForReportPackage(int reportPackageId, IEnumerable<FileStoreDto> attachments,
            CopyOfRecordPdfFileDto copyOfRecordPdfFileDto, CopyOfRecordDataXmlFileDto copyOfRecordDataXmlFileDto);
        CopyOfRecordValidationResultDto ValidCopyOfRecordData(int reportPackageId);
        CopyOfRecordDto GetCopyOfRecordByReportPackage(ReportPackageDto reportPackage);
    }
}
