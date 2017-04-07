namespace Linko.LinkoExchange.Services.CopyOrRecord
{
    public interface IDigitalSignatureManager
    {
        string SignData(string base64Data);
        int LatestCertificateId();
    }
}
