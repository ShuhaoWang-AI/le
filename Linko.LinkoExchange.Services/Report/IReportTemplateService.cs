using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Report
{
    public interface IReportTemplateService
    {
        void DeleteReportPackageTemplate(int reportPackageTemplateId);
        ReportPackageTemplateDto GetReportPackageTemplate(int reportPackageTemplateId);
        IEnumerable<ReportPackageTemplateDto> GetReportPackageTemplates(bool isGetAssignedToThisUserOnly = false, bool includeChildObjects = true);
        int SaveReportPackageTemplate(ReportPackageTemplateDto rpt);

        IEnumerable<CtsEventTypeDto> GetCtsEventTypes(bool isForSample);
        CtsEventTypeDto GetCtsEventType(int ctsEventTypeId);
        IEnumerable<ReportElementTypeDto> GetCertificationTypes();
        IEnumerable<ReportElementTypeDto> GetAttachmentTypes();
        IEnumerable<ReportElementTypeDto> GetSampleAndResultTypes();
        IEnumerable<ReportElementCategoryDto> GetReportElementCategories();
        IEnumerable<ReportElementCategoryName> GetReportElementCategoryNames();
        
    }
}