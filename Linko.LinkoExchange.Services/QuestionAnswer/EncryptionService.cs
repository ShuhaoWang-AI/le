using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    public class EncryptionService : IEncryptionService
    {
        private const string CONST_PASSWORD = "L1nk0Exch@ng3";


        public string EncryptString(string readableString)
        {
            return StringCipher.Encrypt(readableString, CONST_PASSWORD);
        }

        public string DecryptString(string encryptedString)
        {
            return StringCipher.Decrypt(encryptedString, CONST_PASSWORD);
        }

    }
}
