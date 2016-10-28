using System.Web.Mvc;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;

namespace Linko.LinkoExchange.Web.Mvc
{
    public class CommonInfoAttribute : ActionFilterAttribute
    {
        #region constructor

        private readonly IAuthenticationService _authenticationService;
        
        public CommonInfoAttribute(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        #endregion


        #region default action
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Controller.ViewBag.PortalName = _authenticationService.GetClaimsValue(CacheKey.PortalName);
                filterContext.Controller.ViewBag.OrganizationName = _authenticationService.GetClaimsValue(CacheKey.OrganizationName);
                filterContext.Controller.ViewBag.UserName = _authenticationService.GetClaimsValue(CacheKey.UserName);
                filterContext.Controller.ViewBag.UserRole = _authenticationService.GetClaimsValue(CacheKey.UserRole);
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