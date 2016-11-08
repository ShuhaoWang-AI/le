namespace Linko.LinkoExchange.Services.Cache
{
    public interface ISessionCache
    {
        void SetValue(string key, object value);
        object GetValue(string key);
        string GetClaimValue(string claimType);
        void RestoreClaims();
        void Clear();
    } 
}
