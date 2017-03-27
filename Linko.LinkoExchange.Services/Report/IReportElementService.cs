using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Report
{
    public interface IReportElementService
    {
        IEnumerable<ReportElementTypeDto> GetReportElementTypes(ReportElementCategoryName categoryName);
        ReportElementTypeDto GetReportElementType(int reportElementTypeId);
        void SaveReportElementType(ReportElementTypeDto reportElementType);
        void DeleteReportElementType(int reportElementTypeId);
        bool IsReportElementTypeInUse(int reportElementTypeId);
    }
}
