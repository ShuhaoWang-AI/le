using System.Web.Mvc;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Cache;

namespace Linko.LinkoExchange.Web.Mvc
{
    public class CommonInfoAttribute : ActionFilterAttribute
    {
        #region constructor

        private readonly IHttpContextService _httpContextService;
        public CommonInfoAttribute(IHttpContextService httpContextService)
        {
            _httpContextService = httpContextService;
        }

        #endregion


        #region default action
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var portalName = _httpContextService.GetClaimValue(claimType:CacheKey.PortalName);
                filterContext.Controller.ViewBag.PortalName = string.IsNullOrWhiteSpace(value:portalName) ? "" : portalName;
                filterContext.Controller.ViewBag.OrganizationName = string.IsNullOrWhiteSpace(value:portalName) ? "" : _httpContextService.GetClaimValue(claimType:CacheKey.OrganizationName);
                filterContext.Controller.ViewBag.UserName = _httpContextService.GetClaimValue(claimType:CacheKey.UserName);
                filterContext.Controller.ViewBag.UserRole = _httpContextService.GetClaimValue(claimType:CacheKey.UserRole);
            }
            else
            {
                filterContext.Controller.ViewBag.PortalName = "";
                filterContext.Controller.ViewBag.OrganizationName = "";
                filterContext.Controller.ViewBag.UserName = "";
                filterContext.Controller.ViewBag.UserRole = "";
            }
            base.OnResultExecuting(filterContext:filterContext);
        }
        #endregion
    }
}