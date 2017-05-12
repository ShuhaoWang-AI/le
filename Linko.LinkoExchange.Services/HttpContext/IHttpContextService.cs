namespace Linko.LinkoExchange.Services
{
    public interface IHttpContextService
    {
        System.Web.HttpContext Current();
        string GetRequestBaseUrl();
        string GetClaimValue(string claimType);
        object GetSessionValue(string key);
        void SetSessionValue(string key, object value);

        int CurrentUserProfileId();
        string CurrentUserIPAddress();
        string CurrentUserHostName();
        string GetCurrentWebSiteRootUrl();
    }
}
