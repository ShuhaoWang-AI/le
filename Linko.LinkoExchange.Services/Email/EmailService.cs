using System.Configuration;
using System.Net.Mail;
using Microsoft.AspNet.Identity; 
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Email
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            var emailServer = ConfigurationManager.AppSettings["EmailServer"]; 
            var emailSenderEmail = ConfigurationManager.AppSettings["EmailSenderFromEmail"];

            // Plug in your email service here to send an email. 
            using (var smtpClient = new SmtpClient(emailServer))
            {
                smtpClient.Send(emailSenderEmail, message.Destination, message.Subject, message.Body);
            }
            
            return Task.FromResult (0);
        }
    }
}
