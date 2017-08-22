using System.Configuration;

namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    public class EncryptionService : IEncryptionService
    {
        #region interface implementations

        public string EncryptString(string readableString)
        { 
            return StringCipher.Encrypt(readableString);
        }

        public string DecryptString(string encryptedString)
        { 
            return StringCipher.Decrypt(encryptedString);
        }

        #endregion
    }
}