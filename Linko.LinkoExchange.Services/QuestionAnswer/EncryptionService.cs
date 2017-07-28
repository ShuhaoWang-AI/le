using System.Configuration;

namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    public class EncryptionService : IEncryptionService
    {
        public EncryptionService()
        {
        }

        public string EncryptString(string readableString)
        {
            var encryptionPassword = ConfigurationManager.AppSettings["EncryptionPassword"];
            return StringCipher.Encrypt(readableString, encryptionPassword);
        }

        public string DecryptString(string encryptedString)
        {
            var encryptionPassword = ConfigurationManager.AppSettings["EncryptionPassword"];
            return StringCipher.Decrypt(encryptedString, encryptionPassword);
        }

    }
}
