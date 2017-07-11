using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a Report Element Type for a Report Element Category within a Report Package or Report Package Template.
    /// </summary>
    public partial class ReportElementType
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int ReportElementTypeId { get; set; }

        /// <summary>
        /// Unique within a particular OrganizationRegulatoryProgramId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Text to be displayed when IsContentProvided is true, e.g. certification text the submitter will agree to.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// True: the content is provided and should be displayed on the UI. False, otherwise.
        /// </summary>
        public bool IsContentProvided { get; set; }

        public int? CtsEventTypeId { get; set; }
        public virtual CtsEventType CtsEventType { get; set; }

        public int ReportElementCategoryId { get; set; }
        public virtual ReportElementCategory ReportElementCategory { get; set; }

        /// <summary>
        /// Typical value: Authority Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}
