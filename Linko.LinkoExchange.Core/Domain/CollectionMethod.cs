using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a Collection Method for a particular organization.
    /// </summary>
    public partial class CollectionMethod
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int CollectionMethodId { get; set; }

        /// <summary>
        /// Unique within a particular OrganizationId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Typical value: Authority id.
        /// </summary>
        public int OrganizationId { get; set; }
        public virtual Organization Organization { get; set; }

        /// <summary>
        /// True: the current collection method is visible to the Industry. False, otherwise.
        /// </summary>
        public bool IsEnabled { get; set; }

        public bool IsRemoved { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        public virtual ICollection<MonitoringPointParameterLimit> MonitoringPointParameterLimits { get; set; }

        public virtual ICollection<SampleFrequency> SampleFrequencies { get; set; }
    }
}

