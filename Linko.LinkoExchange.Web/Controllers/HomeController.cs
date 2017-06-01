using System.Web.Mvc;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.TermCondition;
using Linko.LinkoExchange.Web.ViewModels.Shared;
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
        private readonly ITermConditionService _termConditionService;

        public HomeController(IAuthenticationService authenticationService, ITermConditionService termConditionService)
        {
            _authenticationService = authenticationService;
            _termConditionService = termConditionService;
        }

        #endregion
        
        #region Terms And Conditions

        // GET: /TermsAndConditions
        [AllowAnonymous]
        [Route(template:"TermsAndConditions")]
        public ActionResult TermsAndConditions()
        {
            var termCondition = _termConditionService.GetTermCondtionContent();
            var model = new ConfirmationViewModel
                        {
                            Title = "Terms and Conditions",
                            HtmlStr = termCondition
                        };

            return View(viewName:"TermsAndConditions", model:model);
        }
        #endregion

        #region Terms And Conditions

        // GET: /PrivacyPolicy
        [AllowAnonymous]
        [Route(template:"PrivacyPolicy")]
        public ActionResult PrivacyPolicy()
        {
            return View();
        }
        #endregion
    }
}