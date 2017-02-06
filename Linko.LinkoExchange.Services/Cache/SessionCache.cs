using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace Linko.LinkoExchange.Services.Cache
{
    public class SessionCache : ISessionCache
    {
        private readonly IHttpContextService _httpContextService;
        public SessionCache(IHttpContextService httpContext)
        {
            _httpContextService = httpContext;
        }

        public void SetValue(string key, object value)
        {
            _httpContextService.SetSessionValue(key, value);
        }

        public object GetValue(string key)
        {
            var value = _httpContextService.GetSessionValue(key);
            if (value != null) return value;

            return string.Empty;
        }

        public string GetClaimValue(string claimType)
        {
            if (_httpContextService.Current().User.Identity.IsAuthenticated)
            {
                var identity = _httpContextService.Current().User.Identity as ClaimsIdentity;
                List<Claim> claims = identity.Claims.ToList<Claim>();

                var claim = claims.FirstOrDefault(i => i.Type == claimType);
                if (claim != null)
                {
                    return claim.Value;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public void Clear()
        {
            HttpContext.Current.Session.Clear();
        }
    }
}
