using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a Unit.
    /// </summary>
    public partial class Unit
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int UnitId { get; set; }

        /// <summary>
        /// Unique within a particular OrganizationRegulatoryProgramId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// True: the current unit is for flow. False, otherwise.
        /// </summary>
        public bool IsFlowUnit { get; set; }

        /// <summary>
        /// True: the flow unit is displayed to the Industry. False, otherwise.
        /// </summary>
        public bool IsFlowUnitVisible { get; set; }

        /// <summary>
        /// Typical value: Authority id.
        /// </summary>
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        public bool IsRemoved { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}

