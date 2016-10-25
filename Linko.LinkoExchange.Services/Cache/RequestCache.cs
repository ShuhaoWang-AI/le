using System.Web;

namespace Linko.LinkoExchange.Services.Cache
{
    public class RequestCache : IRequestCache
    {
        public void SetValue(string key, object value)
        {
            HttpContext.Current.Items[key] = value;
        }

        public object GetValue(string key)
        {
            return HttpContext.Current.Items[key]; 
        }
    }
}