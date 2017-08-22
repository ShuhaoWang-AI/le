using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using Linko.LinkoExchange.Services.HttpContext;

namespace Linko.LinkoExchange.Services.Cache
{
    public class SessionCache : ISessionCache
    {
        #region fields

        private readonly IHttpContextService _httpContextService;

        #endregion

        #region constructors and destructor

        public SessionCache(IHttpContextService httpContext)
        {
            _httpContextService = httpContext;
        }

        #endregion

        #region interface implementations

        public void SetValue(string key, object value)
        {
            _httpContextService.SetSessionValue(key:key, value:value);
        }

        public object GetValue(string key)
        {
            var value = _httpContextService.GetSessionValue(key:key);
            return value;
        }

        public void Clear()
        {
            _httpContextService.Current.Session.Clear();
        }

        #endregion

        public string GetClaimValue(string claimType)
        {
            if (_httpContextService.Current.User.Identity.IsAuthenticated)
            {
                var identity = _httpContextService.Current.User.Identity as ClaimsIdentity;
                var claims = identity?.Claims.ToList();

                Debug.Assert(condition:claims != null, message:"claims != null");
                var claim = claims.FirstOrDefault(i => i.Type == claimType);
                return claim != null ? claim.Value : string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}