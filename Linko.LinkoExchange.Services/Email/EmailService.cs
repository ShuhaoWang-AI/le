using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Linko.LinkoExchange.Services.Email
{
    public class EmailService : IIdentityMessageService
    {
        #region interface implementations

        public Task SendAsync(IdentityMessage message)
        {
            return Task.FromResult(result:0);
        }

        #endregion
    }
}