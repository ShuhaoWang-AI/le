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
           // var owinUserId = GetValue(CacheKey.OwinUserId) as string;
           // if (string.IsNullOrWhiteSpace(owinUserId)) return null;

            var owinClaims = GetValue(CacheKey.OwinClaims) as IList<Claim>;
            
            // owinClaims == null,  restore session from identity 
            if(owinClaims == null)
            {
                RestoreClaims();
                owinClaims = GetValue(CacheKey.OwinClaims) as IList<Claim>;
            } 

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

        public void RestoreClaims()
        {
            if (_httpContextService.Current().User.Identity.IsAuthenticated)
            {
                string owinUserId; 
                var identity = _httpContextService.Current().User.Identity as ClaimsIdentity;
                var claims = identity.Claims.ToList<Claim>();
                var owinUserIdClaim = claims.FirstOrDefault(i => i.Type == CacheKey.OwinUserId);
                this.SetValue(CacheKey.OwinClaims, claims); 
                if (owinUserIdClaim != null)
                {
                    owinUserId = owinUserIdClaim.Value;
                }
                else
                {
                    owinUserId = claims.FirstOrDefault(i => i.Type.IndexOf("nameidentifier") > 0).Value;
                }

                if (owinUserId != null)
                {
                    this.SetValue(CacheKey.OwinUserId, owinUserId);
                }
            }
        }
    }
}
