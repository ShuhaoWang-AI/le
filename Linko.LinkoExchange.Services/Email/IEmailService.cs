using Linko.LinkoExchange.Core.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Email
{
    public interface IEmailService
    {
        Task SendEmail(IEnumerable<string> recipients, EmailType emailType,
            IDictionary<string, string> contentReplacements, bool perRegulatoryProgram = true);
    }
}