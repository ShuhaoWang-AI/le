using System;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ParameterDto
    {
        #region public properties

        public int ParameterId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
		 
		//public DateTime EffectiveDateTimeLocal { get; set; }
		//public DateTime RetireDateTimeLocal { get; set; } 
		//public ICollection<MonitoringPointParameterLimitDto> MonitoringPointParameterLimits { get; set; }

        //UC-15-3.1.2: If the parameter has a concentration limit at the Monitoring Point it also adds unit
        //
        //Either DefaultUnit at the Parameter level OR overridden by the 
        //DefaultUnit at the MonitoringPoint
        public UnitDto DefaultUnit { get; set; }

        public double? TrcFactor { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public bool IsRemoved { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }

        //UC-15-3.1.3: If the parameter has a mass limit at the Monitoring Point it also checks the "Calc Mass Loadings" box
        public bool IsCalcMassLoading { get; set; }

        //Limits at a particular monitoring point may or may not exist
        public double? ConcentrationMinValue { get; set; }
        public double? ConcentrationMaxValue { get; set; }
        public double? MassLoadingMinValue { get; set; }
        public double? MassLoadingMaxValue { get; set; }

        #endregion
    }
}