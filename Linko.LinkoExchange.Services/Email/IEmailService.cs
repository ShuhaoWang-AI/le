using Linko.LinkoExchange.Core.Enum;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Email
{
    public interface IEmailService
    { 
        void SendEmail(IEnumerable<string> recipients, EmailType emailType, IDictionary<string, string> contentReplacements);
    }
}