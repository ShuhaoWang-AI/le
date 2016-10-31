using Linko.LinkoExchange.Services.Authentication;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;

namespace Linko.LinkoExchange.Services.Cache
{
    public class SessionCache : ISessionCache
    {  
        public void SetValue(string key, object value)
        {
            HttpContext.Current.Session[key] = value;
        }

        public object GetValue(string key)
        {
            var value = HttpContext.Current.Session[key];
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
