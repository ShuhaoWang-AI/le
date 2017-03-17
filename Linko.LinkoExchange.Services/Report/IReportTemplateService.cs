using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Report
{
    public interface IReportTemplateService
    {
        void DeleteReportPackageTemplate(int reportPackageTemplateId);
        ReportPackageTemplateDto GetReportPackageTemplate(int reportPackageTemplateId);
        IEnumerable<ReportPackageTemplateDto> GetReportPackageTemplates();
        void SaveReportPackageTemplate(ReportPackageTemplateDto rpt);

        IEnumerable<CtsEventTypeDto> GetCtsEventTypes();
        IEnumerable<ReportElementTypeDto> GetCertificationTypes();
        IEnumerable<ReportElementTypeDto> GetAttachmentTypes();
    }
}