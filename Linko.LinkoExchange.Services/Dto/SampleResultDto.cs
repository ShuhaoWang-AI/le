using Linko.LinkoExchange.Core.Enum;
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
        public string EnteredMethodDetectionLimit { get; set; }
        public string AnalysisMethod { get; set; }
        public DateTime? AnalysisDateTimeLocal { get; set; }
        public bool IsApprovedEPAMethod { get; set; }
        public bool IsCalculated { get; set; }
        public LimitTypeName LimitTypeName { get; set; }
        public LimitBasisName LimitBasisName { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }
        public bool IsCalcMassLoading { get; set; }
        public string MassLoadingQualifier { get; set; }
        public double? MassLoadingValue { get; set; }
        public int MassLoadingUnitId { get; set; }
        public string MassLoadingUnitName { get; set; }


    }
}
