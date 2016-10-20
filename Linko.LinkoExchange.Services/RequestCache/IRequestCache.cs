namespace Linko.LinkoExchange.Services.RequestCache
{
    public interface IRequestCache
    {
        void SetValue(string key, object value);
        object GetValue(string key);
    }
}
