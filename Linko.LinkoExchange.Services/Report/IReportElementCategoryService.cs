using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Report
{
    public enum ReportElementCategoryName
    {
        Certifications,
        Attachments
    }
    public interface IReportElementCategoryService
    {
        int GetReportElementCategoryId(ReportElementCategoryName categoryName);
    }
}
