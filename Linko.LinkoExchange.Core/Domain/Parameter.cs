using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    ///     Represents a Parameter.
    /// </summary>
    public class Parameter
    {
        #region public properties

        /// <summary>
        ///     Primary key.
        /// </summary>
        public int ParameterId { get; set; }

        /// <summary>
        ///     Unique within a particular OrganizationRegulatoryProgramId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public int DefaultUnitId { get; set; }
        public virtual Unit DefaultUnit { get; set; }

        public double? TrcFactor { get; set; }

        public bool IsFlowForMassLoadingCalculation { get; set; }

        /// <summary>
        ///     Typical value: Authority Organization Regulatory Program id.
        /// </summary>
        public int OrganizationRegulatoryProgramId { get; set; }

        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public bool IsRemoved { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }

        // Reverse navigation
        public virtual ICollection<MonitoringPointParameter> MonitoringPointParameters { get; set; }

        #endregion
    }
}