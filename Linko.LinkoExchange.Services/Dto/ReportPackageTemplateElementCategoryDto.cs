using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ReportPackageTemplateElementCategoryDto
    {
        #region public properties

        public int ReportPackageTemplateElementCategoryId { get; set; }
        public int ReportPackageTemplateId { get; set; }
        public virtual ReportPackageTemplateDto ReportPackageTemplate { get; set; }
        public int ReportElementCategoryId { get; set; }
        public virtual ReportElementCategoryDto ReportElementCategory { get; set; }

        public int SortOrder { get; set; }

        public virtual ICollection<ReportPackageTemplateElementTypeDto> ReportPackageTemplateElementTypes { get; set; }

        #endregion
    }
}