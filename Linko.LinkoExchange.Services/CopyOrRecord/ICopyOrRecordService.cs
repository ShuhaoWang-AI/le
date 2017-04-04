
namespace Linko.LinkoExchange.Services.CopyOrRecord
{
    public interface ICopyOrRecordService
    {
        CopyOfRecordDto CreateCopyOfRecordForReportPackage(int reportPackageId);
        bool ValidCoreData(int copyOfRecordId);
        CopyOfRecordDto GetCopyOfRecordByReportPackageId(int reportPackageId);
        CopyOfRecordDto GetCopyOfRecordById(int copyOfRecordId);
    }
}
