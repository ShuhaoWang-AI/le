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
            return Task.FromResult (0);
        }
    }
}
