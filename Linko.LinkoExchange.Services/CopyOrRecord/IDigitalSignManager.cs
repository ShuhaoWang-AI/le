namespace Linko.LinkoExchange.Services.CopyOrRecord
{
    public interface IDigitalSignManager
    {
        string GetDataSignature(string base64Data);
    }
}
