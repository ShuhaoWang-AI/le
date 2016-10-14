using Linko.LinkoExchange.Data;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web.UI.WebControls;
using Linko.LinkoExchange.Core.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Email
{
    public class LinkoExchangeEmailService : IEmailService
    {
        private readonly ApplicationDbContext _linkoExchangeDbContext = new ApplicationDbContext();
        
        private readonly string _emailServer = ConfigurationManager.AppSettings["EmailServer"];
        private readonly string _fromEmailAddress = ConfigurationManager.AppSettings["EmailSenderFromEmail"];

        public async void SendEmail(IEnumerable<string> recipients, EmailType emailType, IDictionary<string, string> contentReplacements)
        {
            string sendTo = string.Join(",", recipients);

            MailMessage msg = await GetMailMessage(sendTo, emailType, contentReplacements);
            using (var smtpClient = new SmtpClient(_emailServer))
            {
                smtpClient.Send(msg);
            }
        }
        
        private Task<MailMessage> GetMailMessage(string sendTo, EmailType emailType, IDictionary<string,string> replacements)
        {         
            var keyValues = replacements.Select(i =>
            {
                return new KeyValuePair<string, string>("{" + i.Key + "}", i.Value);
            }); 

            replacements = keyValues.ToDictionary(i=>i.Key, i=>i.Value);
            
            var emailTemplateName = string.Format("Email_{0}", emailType.ToString());
            var emailTemplate = _linkoExchangeDbContext.AuditLogTemplates.First(i => i.Name == emailTemplateName);

            if (emailTemplate == null)
                return null;

            var mailDefinition = new MailDefinition
            {
                IsBodyHtml = true,
                From = _fromEmailAddress,
                Subject = ReplaceUsingTemplates(emailTemplate.SubjectTemplate, keyValues)
            };

            var emailMessage = mailDefinition.CreateMailMessage(sendTo, (IDictionary)replacements, emailTemplate.MessageTemplate, new System.Web.UI.Control()); 
            return Task.FromResult(emailMessage);
        }

        private string ReplaceUsingTemplates(string originText, IEnumerable<KeyValuePair<string, string>> replacements)
        {
            foreach (var kv in replacements)
            {
                originText = originText.Replace(kv.Key, kv.Value);
            }

            return originText;
        }
    }
}