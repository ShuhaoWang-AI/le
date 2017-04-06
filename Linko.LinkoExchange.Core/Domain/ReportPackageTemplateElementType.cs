﻿namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a specific Report Element Type for a Report Element Category within a Report Package Template.
    /// </summary>
    public partial class ReportPackageTemplateElementType
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int ReportPackageTemplateElementTypeId { get; set; }

        public int ReportPackageTemplateElementCategoryId { get; set; }
        public virtual ReportPackageTemplateElementCategory ReportPackageTemplateElementCategory { get; set; }

        /// <summary>
        /// Unique within a particular ReportPackageTemplateElementCategoryId. 
        /// </summary>
        public int ReportElementTypeId { get; set; }
        public virtual ReportElementType ReportElementType { get; set; }

        public bool IsRequired { get; set; }

        public int SortOrder { get; set; }
    }
}
