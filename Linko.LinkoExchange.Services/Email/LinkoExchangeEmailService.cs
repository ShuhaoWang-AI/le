using Linko.LinkoExchange.Data;
using System.Collections;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Email
{
    public class LinkoExchangeEmailService
    { 
        private static readonly ApplicationDbContext LinkoExchangeDbContext = new ApplicationDbContext(); 

        private static readonly string EmailServer = ConfigurationManager.AppSettings["EmailServer"];  

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="sendTo">The target email address</param>
        /// <param name="subject">The subject of the email to send</param>
        /// <param name="emailType">The email type to send</param>
        /// <param name="replacements">The values to replace the place holders in the email templates</param>
        public static void SendEmail(string sendTo, string subject, EmailType emailType, IDictionary replacements)
        {
            var msg = GetMailMessage(sendTo, subject, emailType, replacements).Result;
            using (var smtpClient = new SmtpClient(EmailServer))
            {
                smtpClient.Send(msg);
            }
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="sendTo">The target email address</param>
        /// <param name="subject">The subject of the email to send</param>
        /// <param name="emailType">The email type to send</param>
        /// <param name="replacements">The values to replace the place holders in the email templates</param>
        public static async Task<MailMessage> GenerateMailMessage(string sendTo, string subject, EmailType emailType, IDictionary replacements)
        {
            var msg = await GetMailMessage(sendTo, subject, emailType, replacements); 
            return msg;  
        }

        private static async Task<MailMessage> GetMailMessage(string sendTo, string subject, EmailType emailType, IDictionary replacements)
        {
            // TODO replace with the real email address
            sendTo = "shuhao.wang@watertrax.com";

            //LinkoExchangeEntities entities = new LinkoExchangeEntities();
            //var emailTemplate = entities.EmailTemplates.First(i => i.EmailType == emailType.ToString()).Template;
            var emailTemplate = LinkoExchangeDbContext.EmailTemplates.FirstOrDefaultAsync(i => i.EmailType == emailType.ToString()).Result; 
            
            if(emailTemplate == null || emailTemplate.Template == null) 
                 return null; 

            MailDefinition md = new MailDefinition
            {
                IsBodyHtml = true,
                From = sendTo,
                Subject = subject
            };
            
            MailMessage msg = md.CreateMailMessage(sendTo, replacements, emailTemplate.Template, new System.Web.UI.Control());
            return msg;
        }
    } 
}