﻿using System.Linq;
using Linko.LinkoExchange.Data;
using NLog;

namespace Linko.LinkoExchange.Services.PrivacyPolicy
{
    public class PrivacyPolicyService : IPrivacyPolicyService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly ILogger _logger;

        #endregion

        #region Implementation of IPrivacyPolicyService

        public PrivacyPolicyService(LinkoExchangeContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        ///     Always get the latest privacy policy
        /// </summary>
        /// <returns> </returns>
        public string GetPrivacyPolicyContent()
        {
            _logger.Info(message:$"Enter PrivacyPolicyService.GetPrivacyPolicyContent.");

            var privacyPolicy = _dbContext.PrivacyPolicies.OrderByDescending(i=>i.EffectiveDateTimeUtc).First();  

            _logger.Info(message:$"Enter PrivacyPolicyService.GetPrivacyPolicyContent.");

            return privacyPolicy.Content;
        }

        #endregion
    }
}