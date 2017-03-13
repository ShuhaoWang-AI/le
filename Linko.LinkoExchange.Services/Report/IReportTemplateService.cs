
using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services
{
    public interface IReportTemplateService
    {
        IEnumerable<Dto.ReportPackageTemplateDto> GetReportPackageTemplates();
        IEnumerable<CtsEventTypeDto> GetCTSEventTypes();
        void SaveReportPackageTemplate(Dto.ReportPackageTemplateDto rpt);
        ReportPackageTemplateDto GetReportPackageTemplate(int reportPackageTemplateId);
        void deleteReportPackageTemplate(int reportPackageTemplateId);
    }
}