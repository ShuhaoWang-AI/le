using System.Web.Mvc;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services;

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
                var portalName = _httpContextService.GetClaimValue(CacheKey.PortalName);
                filterContext.Controller.ViewBag.PortalName = string.IsNullOrWhiteSpace(portalName) ? "" : portalName;
                filterContext.Controller.ViewBag.OrganizationName = string.IsNullOrWhiteSpace(portalName) ? "" : _httpContextService.GetClaimValue(CacheKey.OrganizationName);
                filterContext.Controller.ViewBag.UserName = _httpContextService.GetClaimValue(CacheKey.UserName);
                filterContext.Controller.ViewBag.UserRole = _httpContextService.GetClaimValue(CacheKey.UserRole);
            }
            else
            {
                filterContext.Controller.ViewBag.PortalName = "";
                filterContext.Controller.ViewBag.OrganizationName = "";
                filterContext.Controller.ViewBag.UserName = "";
                filterContext.Controller.ViewBag.UserRole = "";
            }
        }
        #endregion
    }
}