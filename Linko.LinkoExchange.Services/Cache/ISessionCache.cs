namespace Linko.LinkoExchange.Services.Cache
{
    public interface ISessionCache
    {
        void SetValue(string key, object value);
        object GetValue(string key);
        void Clear();
    } 
}
