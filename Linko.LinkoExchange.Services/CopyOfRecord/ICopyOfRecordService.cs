
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.CopyOrRecord
{
    public interface ICopyOfRecordService
    {
        void CreateCopyOfRecordForReportPackage(int reportPackageId);
        bool ValidCoreData(int copyOfRecordId);
        CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId);
        CopyOfRecordDto GetCopyOfRecordById(int copyOfRecordId);
    }
}
