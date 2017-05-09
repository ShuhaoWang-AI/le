using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a Monitoring Point.
    /// </summary>
    public partial class MonitoringPoint
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int MonitoringPointId { get; set; }

        /// <summary>
        /// Unique within a particular OrganizationRegulatoryProgramId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Typical value: Industry Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        /// <summary>
        /// True: the current monitoring point is visible to the Industry. False, otherwise.
        /// </summary>
        public bool IsEnabled { get; set; }

        public bool IsRemoved { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }


        // Reverse navigation
        public virtual ICollection<MonitoringPointParameter> MonitoringPointParameters { get; set; }
    }
}

