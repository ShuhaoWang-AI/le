﻿using System;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// Represents a sample result.
    /// </summary>
    public partial class SampleResult
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int SampleResultId { get; set; }

        public int SampleId { get; set; }
        public virtual Sample Sample { get; set; }

        public int ParameterId { get; set; }

        /// <summary>
        /// Denormalized data. Unique within a Sample.
        /// </summary>
        public string ParameterName { get; set; }

        public string Qualifier { get; set; }

        public double? Value { get; set; }

        public int DecimalPlaces { get; set; }

        public int UnitId { get; set; }

        /// <summary>
        /// Denormalized data.
        /// </summary>
        public string UnitName { get; set; }

        public string MethodDetectionLimit { get; set; }

        public string AnalysisMethod { get; set; }

        public DateTimeOffset? AnalysisDateTimeUtc { get; set; }

        public bool IsApprovedEPAMethod { get; set; }

        public bool IsMassLoadingCalculationRequired { get; set; }

        public bool IsFlowForMassLoadingCalculation { get; set; }

        public bool IsCalculated { get; set; }

        public int? LimitTypeId { get; set; }
        public virtual LimitType LimitType { get; set; }

        public int? LimitBasisId { get; set; }
        public virtual LimitBasis LimitBasis { get; set; }

        public DateTimeOffset CreationDateTimeUtc { get; set; }

        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }

        public int? LastModifierUserId { get; set; }
    }
}

