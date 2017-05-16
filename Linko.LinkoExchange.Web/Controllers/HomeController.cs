using System.Web.Mvc;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using NLog;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class HomeController:Controller
    {
        #region default action

        [AllowAnonymous]
        // GET: Home
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                var portalName = _authenticationService.GetClaimsValue(claimType:CacheKey.PortalName);
                portalName = string.IsNullOrWhiteSpace(value:portalName) ? "" : portalName.Trim().ToLower();
                if (portalName.Equals(value:"authority"))
                {
                    // if current portal is Authority
                    return RedirectToAction(actionName:"Index", controllerName:"Authority");
                }
                else if (portalName.Equals(value:"industry"))
                {
                    // if current portal is Industry
                    return RedirectToAction(actionName:"Index", controllerName:"Industry");
                }
                else
                {
                    // no portal selected
                    return RedirectToAction(actionName:"PortalDirector", controllerName:"Account");
                }
            }
            else
            {
                return RedirectToAction(actionName:"SignIn", controllerName:"Account");
            }
        }

        #endregion

        #region constructor

        private readonly IAuthenticationService _authenticationService;

        public HomeController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        #endregion
    }
}