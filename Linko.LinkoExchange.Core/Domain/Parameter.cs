﻿using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a Parameter.
    /// </summary>
    public partial class Parameter
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int ParameterId { get; set; }

        /// <summary>
        /// Unique within a particular OrganizationRegulatoryProgramId.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        public int? DefaultUnitId { get; set; }
        public virtual Unit DefaultUnit { get; set; }

        public double? TrcFactor { get; set; }

        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

        public bool IsRemoved { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}
