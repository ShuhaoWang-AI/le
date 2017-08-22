namespace Linko.LinkoExchange.Services.Cache
{
    public interface IRequestCache
    {
        void SetValue(string key, object value);
        object GetValue(string key);
    }
}