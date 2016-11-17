using System.Web.Mvc;
using Linko.LinkoExchange.Services.Cache;

namespace Linko.LinkoExchange.Web.Mvc
{
    public class CommonInfoAttribute : ActionFilterAttribute
    {
        #region constructor
        
        private readonly ISessionCache _sessionCache; 
        public CommonInfoAttribute(ISessionCache sessionCache)
        {  
            _sessionCache = sessionCache; 
        }

        #endregion


        #region default action
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                var portalName = _sessionCache.GetClaimValue(CacheKey.PortalName);
                filterContext.Controller.ViewBag.PortalName = portalName;
                filterContext.Controller.ViewBag.OrganizationName = string.IsNullOrWhiteSpace(portalName) ? "" : _sessionCache.GetClaimValue(CacheKey.OrganizationName);
                filterContext.Controller.ViewBag.UserName = _sessionCache.GetClaimValue(CacheKey.UserName);
                filterContext.Controller.ViewBag.UserRole = _sessionCache.GetClaimValue(CacheKey.UserRole); 
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