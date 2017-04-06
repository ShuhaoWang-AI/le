using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a Parameter in a Monitoring Point.
    /// </summary>
    public class MonitoringPointParameter
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int MonitoringPointParameterId { get; set; }

        /// <summary>
        /// Typical value: Industry Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public int MonitoringPointId { get; set; }
        public virtual MonitoringPoint MonitoringPoint { get; set; }

        /// <summary>
        /// Unique within a particular effective date period and Monitoring Point for an OrganizationRegulatoryProgramId.
        /// </summary>
        public int ParameterId { get; set; }
        public virtual Parameter Parameter { get; set; }

        public int? DefaultUnitId { get; set; }
        public virtual Unit DefaultUnit { get; set; }

        public DateTimeOffset EffectiveDateTimeUtc { get; set; }

        public DateTimeOffset RetirementDateTimeUtc { get; set; }


        // Reverse navigation
        public virtual ICollection<MonitoringPointParameterLimit> MonitoringPointParameterLimits { get; set; }

        public virtual ICollection<SampleFrequency> SampleFrequencies { get; set; }

        public virtual ICollection<SampleRequirement> SampleRequirements { get; set; }
    }
}
