using System.Collections.Generic;
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
            var owinUserId = GetValue(CacheKey.OwinUserId) as string;
            if (string.IsNullOrWhiteSpace(owinUserId)) return null;

            var owinClaims = GetValue(CacheKey.OwinClaims) as IList<Claim>;
             
            if (owinClaims != null)
            {
                foreach (var claim in owinClaims)
                {
                    if (claim.Type == claimType)
                    {
                        return claim.Value;
                    }
                } 
            }

            return string.Empty;
        }

        public void Clear()
        {
            HttpContext.Current.Session.Clear();
        }
    }
}
