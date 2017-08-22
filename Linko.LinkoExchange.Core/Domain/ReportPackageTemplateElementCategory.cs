using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a specific Report Element Category within a Report Package Template.
    /// </summary>
    public class ReportPackageTemplateElementCategory
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int ReportPackageTemplateElementCategoryId { get; set; }

        public int ReportPackageTemplateId { get; set; }
        public virtual ReportPackageTemplate ReportPackageTemplate { get; set; }

        /// <summary>
        ///     Unique within a particular ReportPackageTemplateId.
        /// </summary>
        public int ReportElementCategoryId { get; set; }

        public virtual ReportElementCategory ReportElementCategory { get; set; }

        public int SortOrder { get; set; }

        // Reverse navigation
        public virtual ICollection<ReportPackageTemplateElementType> ReportPackageTemplateElementTypes { get; set; }

        #endregion
    }
}