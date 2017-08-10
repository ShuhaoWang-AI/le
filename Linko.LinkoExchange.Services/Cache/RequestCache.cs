using Linko.LinkoExchange.Services.HttpContext;

namespace Linko.LinkoExchange.Services.Cache
{
    public class RequestCache : IRequestCache
    {
        #region fields

        private readonly IHttpContextService _httpContextService;

        #endregion

        #region constructors and destructor

        public RequestCache(IHttpContextService httpContext)
        {
            _httpContextService = httpContext;
        }

        #endregion

        #region interface implementations

        public void SetValue(string key, object value)
        {
            _httpContextService.Current.Items[key:key] = value;
        }

        public object GetValue(string key)
        {
            return _httpContextService.Current.Items[key:key];
        }

        #endregion
    }
}