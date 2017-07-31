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

        /// <inheritdoc />
        public string GetPrivacyPolicyContent()
        {
            _logger.Info($"Enter PrivacyPolicyService.GetPrivacyPolicyContent.");

            _logger.Info($"Enter PrivacyPolicyService.GetPrivacyPolicyContent.");

            return "test Privacy policy content!";
        }

        #endregion
    }
}