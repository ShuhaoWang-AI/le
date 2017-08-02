using Linko.LinkoExchange.Core.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Email
{
    public interface IEmailService
    {
        /// <summary>
        /// Used to send email and email audit log for various email types.
        /// </summary>
        /// <param name="recipients"></param>
        /// <param name="emailType"></param>
        /// <param name="contentReplacements">Data items to be swapped into placeholders in email templates</param>
        /// <param name="perRegulatoryProgram">Switch used to indicate if email audit logged for all regulatory programs</param>
        /// <returns></returns>
        Task SendEmail(IEnumerable<string> recipients, EmailType emailType,
            IDictionary<string, string> contentReplacements, bool perRegulatoryProgram = true);

        /// <summary>
        /// Sends any emails stored in the request cache to be sent in bulk.
        /// </summary>
        void SendCachedEmailEntries();
    }
}