namespace Linko.LinkoExchange.Services.Cache
{
    public class RequestCache : IRequestCache
    {
        public void SetValue(string key, object value)
        {
            System.Web.HttpContext.Current.Items[key] = value;
        }

        public object GetValue(string key)
        {
            return System.Web.HttpContext.Current.Items[key]; 
        }
    }
}