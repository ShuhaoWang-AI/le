using Linko.LinkoExchange.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Report
{
    public class ReportElementCategoryService : IReportElementCategoryService
    {
        private readonly LinkoExchangeContext _dbContext;

        public ReportElementCategoryService(LinkoExchangeContext dbContext)
        {
            _dbContext = dbContext;
        }

        public int GetReportElementCategoryId(ReportElementCategoryName categoryName)
        {
            return _dbContext.ReportElementCategories.Single(c => c.Name == categoryName.ToString()).ReportElementCategoryId;
        }
    }
}
