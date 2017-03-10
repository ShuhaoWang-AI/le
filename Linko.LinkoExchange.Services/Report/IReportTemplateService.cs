using System.Collections.Generic;

namespace Linko.LinkoExchange.Services
{
    public interface IReportTemplateService
    {
        IEnumerable<ReportPackageTemplateDto> GetReportPackageTemplates();
    }
}