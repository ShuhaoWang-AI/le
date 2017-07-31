namespace Linko.LinkoExchange.Services.PrivacyPolicy
{
    public interface IPrivacyPolicyService
    {
        /// <summary>
        /// Get the latest Privacy Policy content
        /// </summary>
        /// <returns></returns>
       string GetPrivacyPolicyContent();
    }
}
