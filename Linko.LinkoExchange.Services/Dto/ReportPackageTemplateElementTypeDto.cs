using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ReportPackageTemplateElementTypeDto
    {
        public int ReportPackageTemplateElementTypeId { get; set; }

        public int ReportPackageTemplateElementCategoryId { get; set; }
        public ReportPackageTemplateElementCategoryDto ReportPackageTemplateElementCategory { get; set; }

        public int ReportElementTypeId { get; set; }
        public virtual ReportElementTypeDto ReportElementType { get; set; }

        public bool IsRequired { get; set; }

        public bool SortOrder { get; set; }
    }
}