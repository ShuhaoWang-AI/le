using System.Web.Mvc;
using Linko.LinkoExchange.Services.Cache;
using System.Web.Routing;
using Linko.LinkoExchange.Services.Organization;
using Microsoft.Practices.Unity;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services.Dto;
using System.Configuration;
using Linko.LinkoExchange.Services;

namespace Linko.LinkoExchange.Web.Mvc
{
    public class AuthorizeAuthorityOnly : ActionFilterAttribute
    {

        [Dependency]
        public IHttpContextService _httpContextService { get; set; }


        private string _unauthorizedPagePath;

        public AuthorizeAuthorityOnly()
        {
            _unauthorizedPagePath = ConfigurationManager.AppSettings["UnauthorizedPagePath"];
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewResult result;

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                string portalName = _httpContextService.GetClaimValue(CacheKey.PortalName);
                portalName = string.IsNullOrWhiteSpace(portalName) ? "" : portalName.Trim().ToLower();
                if (portalName.Equals(value: "authority"))
                {
                    return;
                }
            }

            //Not authorized
            result = new ViewResult
            {
                ViewName = _unauthorizedPagePath,
            };
            filterContext.Result = result;

        }

    }
}