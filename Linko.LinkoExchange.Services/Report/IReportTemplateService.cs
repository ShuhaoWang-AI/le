using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Report
{
    public interface IReportTemplateService
    {
        IEnumerable<ReportPackageTemplateDto> GetReportPackageTemplates();
        IEnumerable<CtsEventTypeDto> GetCtsEventTypes();
        void SaveReportPackageTemplate(ReportPackageTemplateDto rpt);
        ReportPackageTemplateDto GetReportPackageTemplate(int reportPackageTemplateId);
        void DeleteReportPackageTemplate(int reportPackageTemplateId);
    }
}