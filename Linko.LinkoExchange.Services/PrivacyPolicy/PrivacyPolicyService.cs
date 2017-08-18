using System.Linq;
using Linko.LinkoExchange.Data;
using NLog;

namespace Linko.LinkoExchange.Services.PrivacyPolicy
{
    public class PrivacyPolicyService : IPrivacyPolicyService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly ILogger _logger;

        #region Implementation of IPrivacyPolicyService

        public PrivacyPolicyService(LinkoExchangeContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Always get the latest privacy policy
        /// </summary>
        /// <returns></returns>
        public string GetPrivacyPolicyContent()
        { 
            _logger.Info($"Enter PrivacyPolicyService.GetPrivacyPolicyContent.");

            var privacyPolicy = _dbContext.PrivacyPolicies.OrderByDescending(i=>i.EffectiveDateTimeUtc).First();  

            _logger.Info($"Enter PrivacyPolicyService.GetPrivacyPolicyContent.");

            return privacyPolicy.Content;
        }

        #endregion
    }
}