namespace Linko.LinkoExchange.Services.Cache
{
    public interface IApplicationCache
    {
        object Get(string key);
        void Insert(string key, object item, int hours);
    }
}