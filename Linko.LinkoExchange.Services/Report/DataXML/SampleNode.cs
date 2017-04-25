using System.Collections.Generic;
using System.Xml.Serialization;

namespace Linko.LinkoExchange.Services.Report.DataXML
{
    [XmlType(TypeName = "Sample")]
    public class SampleNode
    {
        public string SampleName { get; set; }
        public string MonitoringPointName { get; set; }
        public string CtsEventTypeCategoryName { get; set; }
        public string CtsEventTypeName { get; set; }
        public string CollectionMethodName { get; set; }
        public string LabSampleIdentifier { get; set; }
        public string StartDateTimeUtc { get; set; }
        public string EndDateTimeUtc { get; set; }
        public string SampleFlowForMassCalcs { get; set; }
        public string SampleFlowForMassCalcsUnitName { get; set; }
        public string MassLoadingsConversionFactorPounds { get; set; }
        public string MassLoadingCalculationDecimalPlaces { get; set; }
        public string IsMassLoadingResultToUseLessThanSign { get; set; }
        public string SampledBy { get; set; }
        public List<SampleResultNode> SampleResults { get; set; }
    }
}