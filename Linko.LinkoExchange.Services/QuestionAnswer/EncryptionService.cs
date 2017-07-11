using System.Configuration;

namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    public class EncryptionService : IEncryptionService
    {
        private string _encryptionPassword;

        public EncryptionService()
        {
            _encryptionPassword = ConfigurationManager.AppSettings["EncryptionPassword"];
        }

        public string EncryptString(string readableString)
        {
            return StringCipher.Encrypt(readableString, _encryptionPassword);
        }

        public string DecryptString(string encryptedString)
        {
            return StringCipher.Decrypt(encryptedString, _encryptionPassword);
        }

    }
}
