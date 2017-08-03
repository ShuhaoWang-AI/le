using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Report
{
    public interface IReportTemplateService
    {
        void DeleteReportPackageTemplate(int reportPackageTemplateId);
        ReportPackageTemplateDto GetReportPackageTemplate(int reportPackageTemplateId);
        IEnumerable<ReportPackageTemplateDto> GetReportPackageTemplates(bool isForCreatingDraft = false, bool includeChildObjects = true);
        int SaveReportPackageTemplate(ReportPackageTemplateDto rpt);

        /// <summary>
        /// Returns a collection of CTS event types associated with the authority of the current regulatory program.
        /// Can optionally filter for event types in the "sample" category.
        /// </summary>
        /// <param name="isForSample"></param>
        /// <returns></returns>
        IEnumerable<CtsEventTypeDto> GetCtsEventTypes(bool isForSample);

        /// <summary>
        /// Gets a specific CTS event type with its date/time fields localized to the current regulatory program's authority.
        /// </summary>
        /// <param name="ctsEventTypeId"></param>
        /// <returns></returns>
        CtsEventTypeDto GetCtsEventType(int ctsEventTypeId);

        IEnumerable<ReportElementTypeDto> GetCertificationTypes();
        IEnumerable<ReportElementTypeDto> GetAttachmentTypes();
        IEnumerable<ReportElementTypeDto> GetSampleAndResultTypes();
        IEnumerable<ReportElementCategoryDto> GetReportElementCategories();
        IEnumerable<ReportElementCategoryName> GetReportElementCategoryNames();
        
    }
}