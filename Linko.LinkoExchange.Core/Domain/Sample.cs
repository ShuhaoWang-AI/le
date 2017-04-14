using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a Sample.
    /// </summary>
    public partial class Sample
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int SampleId { get; set; }

        public string Name { get; set; }

        public int MonitoringPointId { get; set; }

        /// <summary>
        /// Denormalized data.
        /// </summary>
        public string MonitoringPointName { get; set; }

        public int CtsEventTypeId { get; set; }

        /// <summary>
        /// Denormalized data.
        /// </summary>
        public string CtsEventTypeName { get; set; }

        /// <summary>
        /// Denormalized data.
        /// </summary>
        public string CtsEventCategoryName { get; set; }

        public int CollectionMethodId { get; set; }

        /// <summary>
        /// Denormalized data.
        /// </summary>
        public string CollectionMethodName { get; set; }

        public string LabSampleIdentifier { get; set; }

        public DateTimeOffset StartDateTimeUtc { get; set; }

        public DateTimeOffset EndDateTimeUtc { get; set; }

        public bool IsCalculated { get; set; }

        public bool IsReadyToReport { get; set; }

        /// <summary>
        /// Typical value: Industry Regulatory Program id. May be the Authority in the future.
        /// </summary>
        public int ByOrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram ByOrganizationRegulatoryProgram { get; set; }

        /// <summary>
        /// Typical value: Industry Regulatory Program id.
        /// </summary>
        public int ForOrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram ForOrganizationRegulatoryProgram { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        // Reverse navigation
        public virtual ICollection<SampleResult> SampleResults { get; set; }

        public virtual ICollection<ReportSample> ReportSamples { get; set; }
    }
}

