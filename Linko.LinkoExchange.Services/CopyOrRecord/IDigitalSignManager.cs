using Linko.LinkoExchange.Core.Domain;

namespace Linko.LinkoExchange.Services.CopyOrRecord
{
    public interface IDigitalSignManager
    {
        CopyOfRecordCertificate GetLatestCertificate();
        string SignData(string base64Data);
    }
}
