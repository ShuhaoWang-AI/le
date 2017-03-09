using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents an Event Type in LinkoCTS.
    /// Anything marked as Event Category name = "Sample" will be used in Sample creation.
    /// Anything marked as Event Category name != “Sample” will be used in Report Package or Element creation.
    /// </summary>
    public partial class CtsEventType
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int CtsEventTypeId { get; set; }

        /// <summary>
        /// Unique within a particular OrganizationRegulatoryProgramId and CTS Event Category.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public string CtsEventCategoryName { get; set; }

        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public bool IsRemoved { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}

