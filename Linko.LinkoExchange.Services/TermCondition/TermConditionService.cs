using System.Linq;
using Linko.LinkoExchange.Data;
using NLog;

namespace Linko.LinkoExchange.Services.TermCondition
{
    public class TermConditionService : ITermConditionService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly ILogger _logger;

        public TermConditionService(LinkoExchangeContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public string GetTermCondtionContent()
        {
            _logger.Info($"Enter TermConditionService.GetTermCondtionContent.");
            var termCondition = _dbContext.TermConditions.OrderByDescending(i => i.TermConditionId).First();
            _logger.Info($"Leave TermConditionService.GetTermCondtionContent.");
            return termCondition.Content;
        }

        public int GetLatestTermConditionId()
        {
            _logger.Info($"Enter TermConditionService.GetLatestTermConditionId.");
            var termCondition = _dbContext.TermConditions.OrderByDescending(i => i.TermConditionId).First();
            _logger.Info($"Leave TermConditionService.GetLatestTermConditionId. termConditionId={termCondition.TermConditionId}");
            return termCondition.TermConditionId;
        }
    }
}