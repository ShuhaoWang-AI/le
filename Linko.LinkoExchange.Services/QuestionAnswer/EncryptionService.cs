namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    public class EncryptionService : IEncryptionService
    {
        #region interface implementations

        public string EncryptString(string readableString)
        {
            return StringCipher.Encrypt(plainText:readableString);
        }

        public string DecryptString(string encryptedString)
        {
            return StringCipher.Decrypt(cipherText:encryptedString);
        }

        #endregion
    }
}