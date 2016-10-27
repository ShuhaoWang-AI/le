using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Email
{
    public interface IEmailService
    {
        Task SendEmail(IEnumerable<string> recipients, EmailType emailType,
            IDictionary<string, string> contentReplacements);
    }
}