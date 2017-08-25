using System;
using Linko.LinkoExchange.Services.HttpContext;

namespace Linko.LinkoExchange.Services.Cache
{
    public class ApplicationCache : IApplicationCache
    {
        #region fields

        private readonly IHttpContextService _httpContextService;

        #endregion

        #region constructors and destructor

        public ApplicationCache(IHttpContextService httpContext)
        {
            _httpContextService = httpContext;
        }

        #endregion

        #region interface implementations

        public object Get(string key)
        {
            return _httpContextService.Current.Cache[key:key];
        }

        public void Insert(string key, object item, int hours)
        {
            _httpContextService.Current.Cache.Insert(key:key, value:item, dependencies:null, absoluteExpiration:DateTime.Now.AddHours(value:hours),
                                                     slidingExpiration:System.Web.Caching.Cache.NoSlidingExpiration);
        }

        #endregion
    }
}