using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Cache
{
    public class ApplicationCache : IApplicationCache
    {
        public object Get(string key)
        {
            return System.Web.HttpContext.Current.Cache[key];
        }
        public void Insert(string key, object item)
        {
            System.Web.HttpContext.Current.Cache.Insert(key, item, null, DateTime.Now.AddDays(1), System.Web.Caching.Cache.NoSlidingExpiration);
        }
    }
}
