
namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    public interface IEncryptionService
    {
        string EncryptString(string readableString);
        string DecryptString(string encryptedString);
    }
}
