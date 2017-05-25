using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Services.Cache;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Linko.LinkoExchange.Web.Mvc
{
    public class PortalAuthorizeAttribute : AuthorizeAttribute
    {
        [Dependency]
        public IHttpContextService _httpContextService { get; set; }
        private readonly string[] _allowedPortals;
        private readonly string _unauthorizedPagePath;
        public PortalAuthorizeAttribute(params string[] roles)
        {
            _allowedPortals = roles;
            _unauthorizedPagePath = ConfigurationManager.AppSettings["UnauthorizedPagePath"];
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorize = false;

            if (httpContext.User.Identity.IsAuthenticated)
            {
                string usersPortal = _httpContextService.GetClaimValue(CacheKey.PortalName);
                usersPortal = string.IsNullOrWhiteSpace(usersPortal) ? "" : usersPortal.Trim().ToLower();
                foreach (var allowedPortal in _allowedPortals)
                {
                    if (usersPortal == allowedPortal)
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
                ViewName = _unauthorizedPagePath,
            };
            filterContext.Result = result;
        }
    }
}