using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a specific Report Element Category within a Report Package.
    /// </summary>
    public class ReportPackageElementCategory
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int ReportPackageElementCategoryId { get; set; }

        public int ReportPackageId { get; set; }
        public virtual ReportPackage ReportPackage { get; set; }

        /// <summary>
        ///     Unique within a Report Package.
        /// </summary>
        public int ReportElementCategoryId { get; set; }

        public virtual ReportElementCategory ReportElementCategory { get; set; }

        public int SortOrder { get; set; }

        // Reverse navigation
        public virtual ICollection<ReportPackageElementType> ReportPackageElementTypes { get; set; }

        #endregion
    }
}