
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.CopyOfRecord
{
    public interface ICopyOfRecordService
    {
        int CreateCopyOfRecordForReportPackage(ReportPackageDto reportPackageDto);
        bool ValidCoreData(int copyOfRecordId);
        CopyOfRecordDto GetCopyOfRecordByReportPackage(ReportPackageDto reportPackage);
        CopyOfRecordDto GetCopyOfRecordById(int copyOfRecordId);
    }
}
