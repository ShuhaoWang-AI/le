using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a specific Report Element Category within a Report Package Template.
    /// </summary>
    public partial class ReportPackageTemplateElementCategory
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int ReportPackageTemplateElementCategoryId { get; set; }

        public int ReportPackageTemplateId { get; set; }
        public virtual ReportPackageTemplate ReportPackageTemplate { get; set; }

        public int ReportElementCategoryId { get; set; }
        public virtual ReportElementCategory ReportElementCategory { get; set; }

        public bool SortOrder { get; set; }


        // Reverse navigation
        public virtual ICollection<ReportPackageTemplateElementType> ReportPackageTemplateElementTypes { get; set; }
    }
}
