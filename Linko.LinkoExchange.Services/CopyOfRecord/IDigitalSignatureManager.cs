namespace Linko.LinkoExchange.Services.CopyOfRecord
{
    public interface IDigitalSignatureManager
    {
        string SignData(string base64Data);
        int GetLatestCertificateId();
        bool VerifySignature(string currentSignatureStr, byte[] dataToVerify, int copyOfRecordCertificateId);
    }
}