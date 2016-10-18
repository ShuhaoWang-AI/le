using System.Web.Mvc;
using System.Web.Routing;

namespace Linko.LinkoExchange.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute(url: "{resource}.axd/{*pathInfo}");

            //Enable attribute routing.
            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
