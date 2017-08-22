using Linko.LinkoExchange.Web;
using Microsoft.Owin;
using Owin;

[assembly:OwinStartup(startupType:typeof(Startup))]

namespace Linko.LinkoExchange.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app:app);
        }
    }
}