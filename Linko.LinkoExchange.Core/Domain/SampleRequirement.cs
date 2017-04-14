using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a Sample Requirement for a certain Parameter in a Monitoring Point.
    /// Number of samples required to be taken.
    /// </summary>
    public partial class SampleRequirement
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int SampleRequirementId { get; set; }

        public int MonitoringPointParameterId { get; set; }
        public virtual MonitoringPointParameter MonitoringPointParameter { get; set; }

        /// <summary>
        /// When calculating if a report submission contains the proper number of samples, 
        /// count the number of results in LE where CollectionStartDateSampled >= PeriodStart AND CollectionEndDateSampled less than or equal to PeriodEnd. 
        /// </summary>
        public DateTimeOffset PeriodStartDateTimeUtc { get; set; }

        /// <summary>
        /// When calculating if a report submission contains the proper number of samples, 
        /// count the number of results in LE where CollectionStartDateSampled >= PeriodStart AND CollectionEndDateSampled less than or equal to PeriodEnd. 
        /// </summary>
        public DateTimeOffset PeriodEndDateTimeUtc { get; set; }

        public int SamplesRequired { get; set; }

        /// <summary>
        /// Future use: to communicate to IU if the # of samples required could be incorrect 
        /// because the Authority placed a parameter in effect in the middle of the sample frequency period.
        /// </summary>
        public DateTimeOffset LimitEffectiveDateTimeUtc { get; set; }

        /// <summary>
        /// Future use: to communicate to IU if the # of samples required could be incorrect 
        /// because the Authority placed a parameter in effect in the middle of the sample frequency period.
        /// </summary>
        public DateTimeOffset? LimitRetirementDateTimeUtc { get; set; }

        /// <summary>
        /// Identifies who needs to sample.
        /// Typical value: Industry Org Reg Program  
        /// However, Authority may need to sample as well.
        /// </summary>
        public int ByOrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram ByOrganizationRegulatoryProgram { get; set; }
    }
}

