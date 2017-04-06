using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a specific Report Element Type for a Report Element Category within a Report Package.
    /// </summary>
    public partial class ReportPackageElementType
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int ReportPackageElementTypeId { get; set; }

        public int ReportPackageElementCategoryId { get; set; }
        public virtual ReportPackageElementCategory ReportPackageElementCategory { get; set; }

        public int ReportElementTypeId { get; set; }

        /// <summary>
        /// Denormalized data.
        /// </summary>
        public string ReportElementTypeName { get; set; }

        /// <summary>
        /// Denormalized data.
        /// </summary>
        public string ReportElementTypeContent { get; set; }

        /// <summary>
        /// Denormalized data.
        /// </summary>
        public bool ReportElementTypeIsContentProvided { get; set; }

        /// <summary>
        /// Denormalized data.
        /// </summary>
        public int? CtsEventTypeId { get; set; }

        /// <summary>
        /// Denormalized data.
        /// </summary>
        public string CtsEventTypeName { get; set; }

        /// <summary>
        /// Denormalized data.
        /// </summary>
        public string CtsEventCategoryName { get; set; }

        public bool IsRequired { get; set; }

        public int SortOrder { get; set; }


        // Reverse navigation
        public virtual ICollection<ReportSample> ReportSamples { get; set; }

        public virtual ICollection<ReportFile> ReportFiles { get; set; }
    }
}
