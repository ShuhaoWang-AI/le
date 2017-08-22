using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a Sample Requirement for a certain Parameter in a Monitoring Point.
    ///     Number of samples required to be taken.
    /// </summary>
    public class SampleRequirement
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int SampleRequirementId { get; set; }

        public int MonitoringPointParameterId { get; set; }
        public virtual MonitoringPointParameter MonitoringPointParameter { get; set; }

        /// <summary>
        ///     When calculating if a report submission contains the proper number of samples,
        ///     count the number of results in LE where CollectionStartDateSampled >= PeriodStart AND CollectionEndDateSampled less
        ///     than or equal to PeriodEnd.
        /// </summary>
        public DateTime PeriodStartDateTime { get; set; }

        /// <summary>
        ///     When calculating if a report submission contains the proper number of samples,
        ///     count the number of results in LE where CollectionStartDateSampled >= PeriodStart AND CollectionEndDateSampled less
        ///     than or equal to PeriodEnd.
        /// </summary>
        public DateTime PeriodEndDateTime { get; set; }

        public int SamplesRequired { get; set; }

        /// <summary>
        ///     Identifies who needs to sample.
        ///     Typical value: Industry OrganizationRegulatoryProgramId.
        ///     However, Authority may need to sample as well.
        /// </summary>
        public int ByOrganizationRegulatoryProgramId { get; set; }

        public virtual OrganizationRegulatoryProgram ByOrganizationRegulatoryProgram { get; set; }

        #endregion
    }
}