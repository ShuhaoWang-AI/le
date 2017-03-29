using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class SampleResultDto
    {
        public int SampleResultId { get; set; }
        public int SampleId { get; set; }
        public int ParameterId { get; set; }
        public string ParameterName { get; set; }
        public string Qualifier { get; set; }
        public double? Value { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string MethodDetectionLimit { get; set; }
        public string AnalysisMethod { get; set; }
        public DateTime? AnalysisDateTimeLocal { get; set; }
        public bool IsApprovedEPAMethod { get; set; }
        public bool IsMassLoadingCalculationRequired { get; set; }
        public bool IsFlowForMassLoadingCalculation { get; set; }
        public bool IsCalculated { get; set; }
        public int? LimitTypeId { get; set; }
        public int? LimitBasisId { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }
    }
}
