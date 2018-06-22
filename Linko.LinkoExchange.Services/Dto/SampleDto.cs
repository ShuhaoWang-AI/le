using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Core.Enum;

[assembly:InternalsVisibleTo(assemblyName:"Linko.LinkoExchange.Test")]

namespace Linko.LinkoExchange.Services.Dto
{
    public class SampleDto
    {
        #region public properties

        public int? SampleId { get; set; }
        public string Name { get; set; } //UI should only read this as it is automatically set based on settings
        public int MonitoringPointId { get; set; }
        public string MonitoringPointName { get; set; }
        public int CtsEventTypeId { get; set; }
        public string CtsEventTypeName { get; set; }
        public string CtsEventCategoryName { get; set; }
        public int CollectionMethodId { get; set; }
        public string CollectionMethodName { get; set; }
        public string LabSampleIdentifier { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset EndDateTime { get; set; }

        public DateTime StartDateTimeLocal { get; set; }
        public DateTime EndDateTimeLocal { get; set; }
        public bool IsReadyToReport { get; set; }
		public double FlowMethodDetectionLimit { get; set; }
		public string FlowAnalysisMethod { get; set; }
		public DateTime? FlowAnalysisDateTime { get; set; }
		
		public IEnumerable<UnitDto> FlowUnitValidValues { get; set; }
        public string ResultQualifierValidValues { get; set; }
        public double? MassLoadingConversionFactorPounds { get; set; }
        public int? MassLoadingCalculationDecimalPlaces { get; set; }
        public bool IsMassLoadingResultToUseLessThanSign { get; set; }

        public SampleStatusName SampleStatusName { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }
        public string FlowParameterName { get; set; }
        public string FlowEnteredValue { get; set; }
        public double? FlowValue { get; set; }
        public int? FlowUnitId { get; set; }
        public string FlowUnitName { get; set; }
        public IEnumerable<SampleResultDto> SampleResults { get; set; }
        public string ByOrganizationTypeName { get; internal set; }
        public bool IsAssociatedWithReportPackage { get; internal set; } // only to be used when displaying report package to show which samples are included

        public DateTime? LastSubmissionDateTimeLocal { get; internal set; }

	    public int GoodConcentrationComplianceCount
	    {
		    get { return SampleResults.Count(i => i.ConcentrationResultCompliance == ResultComplianceType.Good); }
	    }

		public int BadConcentrationComplianceCount
		{
			get { return SampleResults.Count(i => i.ConcentrationResultCompliance == ResultComplianceType.Bad); }
		}

        public int UnknowConcentrationComplianceCount
		{
			get { return SampleResults.Count(i => i.ConcentrationResultCompliance == ResultComplianceType.Unknown); }
		}

		public int GoodMassLoadingComplianceCount
		{
			get { return SampleResults.Count(i => i.MassResultCompliance == ResultComplianceType.Good && !string.IsNullOrWhiteSpace(i.MassLoadingValue)); }
		}

		public int BadMassLoadingComplianceCount
		{
			get { return SampleResults.Count(i => i.MassResultCompliance == ResultComplianceType.Bad && !string.IsNullOrWhiteSpace(i.MassLoadingValue)); }
		}
		public int UnknowMassLoadingComplianceCount
		{
			get { return SampleResults.Count(i => i.MassResultCompliance == ResultComplianceType.Unknown && !string.IsNullOrWhiteSpace(i.MassLoadingValue)); }
		}
		#endregion
	}
}