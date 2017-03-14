using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Report
{
    public interface IReportElementService
    {
        IEnumerable<ReportElementTypeDto> GetReportElementTypes();
        void SaveReportElementType(ReportElementTypeDto certType);
        void DeleteReportElementType(int reportElementTypeId);
    }
}
