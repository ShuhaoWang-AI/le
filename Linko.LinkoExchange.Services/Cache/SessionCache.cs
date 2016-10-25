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
            return HttpContext.Current.Session[key];
        }

        public void Clear()
        {
            HttpContext.Current.Session.Clear();
        }
    }
}
