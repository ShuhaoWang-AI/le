
namespace Linko.LinkoExchange.Services.CopyOrRecord
{
    public interface ICopyOrRecordService
    {
        int CreateCopyOfRecordForReportPackage(int reportPackageId);
        bool ValidCoreData(int copyOfRecordId);
        CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId);
        CopyOfRecordDto GetCopyOfRecordById(int copyOfRecordId);
    }
}
