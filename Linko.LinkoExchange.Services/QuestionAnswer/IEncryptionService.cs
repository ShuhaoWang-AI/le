using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    public interface IEncryptionService
    {
        string EncryptString(string readableString);
        string DecryptString(string encryptedString);
    }
}
