using System.Web.Mvc;
using Linko.LinkoExchange.Web.Mvc;
using Microsoft.Practices.Unity;

namespace Linko.LinkoExchange.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(filter:UnityConfig.GetConfiguredContainer().Resolve<CustomHandleErrorAttribute>());
            filters.Add(filter:new AuthorizeAttribute());
            filters.Add(filter:UnityConfig.GetConfiguredContainer().Resolve<CommonInfoAttribute>()); 
            filters.Add(filter:UnityConfig.GetConfiguredContainer().Resolve<LogAttribute>());
            // filters.Add(new RequireHttpsAttribute());
        }
    }
}