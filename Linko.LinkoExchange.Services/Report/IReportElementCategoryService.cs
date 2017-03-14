using Linko.LinkoExchange.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Report
{
    public interface IReportElementCategoryService
    {
        int GetReportElementCategoryId(ReportElementCategoryName categoryName);
    }
}
