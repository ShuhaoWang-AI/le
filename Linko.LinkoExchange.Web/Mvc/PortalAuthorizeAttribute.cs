using System.Configuration;
using System.Web;
using System.Web.Mvc;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.HttpContext;
using Microsoft.Practices.Unity;

namespace Linko.LinkoExchange.Web.Mvc
{
    public class PortalAuthorizeAttribute : AuthorizeAttribute
    {
        #region fields

        private readonly string[] _allowedPortals;
        private readonly string _unauthorizedPagePath;

        #endregion

        #region constructors and destructor

        public PortalAuthorizeAttribute(params string[] roles)
        {
            _allowedPortals = roles;
            _unauthorizedPagePath = ConfigurationManager.AppSettings[name:"UnauthorizedPagePath"];
        }

        #endregion

        #region public properties

        [Dependency]

        // ReSharper disable once MemberCanBePrivate.Global
        public IHttpContextService HttpContextService { get; set; }

        #endregion

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var authorize = false;

            if (httpContext.User.Identity.IsAuthenticated)
            {
                var usersPortal = HttpContextService.GetClaimValue(claimType:CacheKey.PortalName);
                usersPortal = string.IsNullOrWhiteSpace(value:usersPortal) ? "" : usersPortal.Trim().ToLower();
                foreach (var allowedPortal in _allowedPortals)
                {
                    if (usersPortal == allowedPortal.ToLower())
                    {
                        authorize = true;
                        break;
                    }
                }
            }

            return authorize;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            //filterContext.Result = new HttpUnauthorizedResult(); //Just redirects to login page

            var result = new ViewResult
                         {
                             ViewName = _unauthorizedPagePath
                         };
            filterContext.Result = result;
        }
    }
}