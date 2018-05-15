using System.Linq;
using Linko.LinkoExchange.Data;
using NLog;

namespace Linko.LinkoExchange.Services.TermCondition
{
    public class TermConditionService : ITermConditionService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly ILogger _logger;

        #endregion

        #region constructors and destructor

        public TermConditionService(LinkoExchangeContext dbContext, ILogger logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        #endregion

        #region interface implementations

        public string GetTermConditionContent()
        {
            _logger.Info(message:"Start: TermConditionService.GetTermConditionContent.");
            var termCondition = _dbContext.TermConditions.OrderByDescending(i => i.TermConditionId).First();
            _logger.Info(message:"End: TermConditionService.GetTermConditionContent.");
            return termCondition.Content;
        }

        public int GetLatestTermConditionId()
        {
            _logger.Info(message:"Start: TermConditionService.GetLatestTermConditionId.");
            var termCondition = _dbContext.TermConditions.OrderByDescending(i => i.TermConditionId).First();
            _logger.Info(message:$"End: TermConditionService.GetLatestTermConditionId. termConditionId={termCondition.TermConditionId}");
            return termCondition.TermConditionId;
        }

        #endregion
    }
}