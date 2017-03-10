using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services
{
    public interface IReportTemplateService
    {
        IEnumerable<ReportPackageTemplateDto> GetReportPackageTemplates();
        IEnumerable<CTSEventTypeDto> GetCTSEventTypes();
        void SaveReportPackageTemplate(ReportPackageTemplateDto rpt);
        ReportPackageTemplateDto GetReportPackageTemplate(int reportPackageTemplateId);
        void deleteReportPackageTemplate(int reportPackageTemplateId);
    }
}