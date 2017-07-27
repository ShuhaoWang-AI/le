namespace Linko.LinkoExchange.Services
{
    public interface IHttpContextService
    {
        /// <summary>
        /// Returns the current httpcontext 
        /// </summary>
        /// <returns></returns>
        System.Web.HttpContext Current();

        /// <summary>
        /// Returns the fully qualified URL of the current request
        /// </summary>
        /// <returns></returns>
        string GetRequestBaseUrl();

        /// <summary>
        /// Returns the first identity claim associated with the claim type or empty string
        /// if none exists.
        /// </summary>
        /// <param name="claimType"></param>
        /// <returns></returns>
        string GetClaimValue(string claimType);

        /// <summary>
        /// Returns an object from the session cache if it exists. Null otherwise.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetSessionValue(string key);

        /// <summary>
        /// Puts an object into the session cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetSessionValue(string key, object value);

        /// <summary>
        /// Returns the tUserProfile.UserProfileId associated with the logged in user's identidy.
        /// If no such user exists, the value returned is -1.
        /// </summary>
        /// <returns></returns>
        int CurrentUserProfileId();

        /// <summary>
        /// Returns client's IP address
        /// </summary>
        /// <returns></returns>
        string CurrentUserIPAddress();

        /// <summary>
        /// Attempts to find a host name associated with the client's IP address using
        /// reverse DNS lookup. Returns null or empty string otherwise.
        /// </summary>
        /// <returns></returns>
        string CurrentUserHostName();
    }
}
