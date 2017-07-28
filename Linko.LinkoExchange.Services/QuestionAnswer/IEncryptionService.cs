
namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    public interface IEncryptionService
    {
        /// <summary>
        /// Converts a human readable string to an encrypted string.
        /// </summary>
        /// <param name="readableString"></param>
        /// <returns></returns>
        string EncryptString(string readableString);
        
        /// <summary>
        /// Converts an encrypted string to a human readable string.
        /// </summary>
        /// <param name="encryptedString"></param>
        /// <returns></returns>
        string DecryptString(string encryptedString);
    }
}
