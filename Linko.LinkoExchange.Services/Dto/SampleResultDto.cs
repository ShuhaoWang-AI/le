using System;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Core.Enum;

[assembly:InternalsVisibleTo(assemblyName:"Linko.LinkoExchange.Test")]

namespace Linko.LinkoExchange.Services.Dto
{
    public class SampleResultDto
    {
        #region public properties
        public int? ConcentrationSampleResultId { get; set; }
        public int ParameterId { get; set; }
        public string ParameterName { get; set; }
        public string Qualifier { get; set; }
        public string EnteredValue { get; set; }
        public double? Value { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string EnteredMethodDetectionLimit { get; set; }
        internal double? MethodDetectionLimit { get; set; }
        public string AnalysisMethod { get; set; }
        public DateTime? AnalysisDateTimeLocal { get; set; }
        public bool IsApprovedEPAMethod { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }
        public bool IsCalcMassLoading { get; set; }
        public int? MassLoadingSampleResultId { get; set; }
        public string MassLoadingQualifier { get; set; }
        public string MassLoadingValue { get; set; }
        public int MassLoadingUnitId { get; set; }
        public string MassLoadingUnitName { get; set; }

        public ResultComplianceType ConcentrationResultCompliance { get; set; }
        public string ConcentrationResultComplianceComment { get; set; }
        public ResultComplianceType MassResultCompliance { get; set; }
        public string MassResultComplianceComment { get; set; }

		public bool ExistingUnchanged { get; set; }
		
        #endregion
    }
}