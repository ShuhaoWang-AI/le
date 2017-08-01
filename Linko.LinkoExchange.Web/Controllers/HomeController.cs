using System.Web.Mvc;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.PrivacyPolicy;
using Linko.LinkoExchange.Services.Report;
using Linko.LinkoExchange.Services.Sample;
using Linko.LinkoExchange.Services.TermCondition;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Web.ViewModels.Shared;

namespace Linko.LinkoExchange.Web.Controllers
{
    public class HomeController : BaseController
    {
        #region fields

        private readonly IAuthenticationService _authenticationService;
        private readonly IPrivacyPolicyService _privacyPolicyService;
        private readonly ITermConditionService _termConditionService;

        #endregion

        #region constructors and destructor

        public HomeController(
            IAuthenticationService authenticationService,
            ITermConditionService termConditionService,
            IHttpContextService httpContextService,
            IUserService userService,
            IReportPackageService reportPackageService,
            ISampleService sampleService,
            IPrivacyPolicyService privacyPolicyService)
            : base(httpContextService:httpContextService, userService:userService, reportPackageService:reportPackageService, sampleService:sampleService)
        {
            _authenticationService = authenticationService;
            _termConditionService = termConditionService;
            _privacyPolicyService = privacyPolicyService;
        }

        #endregion

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

        #region Privacy Policy

        // GET: /PrivacyPolicy
        [AllowAnonymous]
        [Route(template:"PrivacyPolicy")]
        public ActionResult PrivacyPolicy()
        {
            var privacyPolicy = _privacyPolicyService.GetPrivacyPolicyContent();
            var model = new ConfirmationViewModel
                        {
                            Title = "General Privacy Policy",
                            HtmlStr = privacyPolicy
                        };
            return View(viewName:"PrivacyPolicy", model:model);
        }

        #endregion
    }
}