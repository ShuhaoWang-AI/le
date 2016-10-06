using Owin;
using Microsoft.Owin;

[assembly: OwinStartupAttribute (typeof (Linko.LinkoExchange.Web.Startup))]
namespace Linko.LinkoExchange.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth (app);
        }
    }
}