using System.Web.Mvc;
using Linko.LinkoExchange.Web.Mvc;
using Microsoft.Practices.Unity;

namespace Linko.LinkoExchange.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(UnityConfig.GetConfiguredContainer().Resolve<CustomHandleErrorAttribute>());
            filters.Add(new AuthorizeAttribute());
            filters.Add(UnityConfig.GetConfiguredContainer().Resolve<CommonInfoAttribute>());            
           // filters.Add(new RequireHttpsAttribute());
        }
    }
}