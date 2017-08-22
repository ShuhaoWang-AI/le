using System.Xml.Serialization;

namespace Linko.LinkoExchange.Services.Report.DataXML
{
    [XmlType(TypeName = "SampleResult")]
    public class SampleResultNode
    {
        #region public properties

        public string SampleName { get; set; }
        public string MonitoringPointName { get; set; }
        public string CtsEventTypeCategoryName { get; set; }
        public string CtsEventTypeName { get; set; }
        public string CollectionMethodName { get; set; }
        public string LabSampleIdentifier { get; set; }
        public string StartDateTimeUtc { get; set; }
        public string EndDateTimeUtc { get; set; }
        public string MassLoadingsConversionFactorPounds { get; set; }
        public string MassLoadingCalculationDecimalPlaces { get; set; }
        public string IsMassLoadingResultToUseLessThanSign { get; set; }
        public string SampledBy { get; set; }
        public string ParameterName { get; set; }
        public string Qualifier { get; set; }
        public string EnteredValue { get; set; }
        public string Value { get; set; }
        public string UnitName { get; set; }
        public string EnteredMethodDetectionLimit { get; set; }
        public string MethodDetectionLimit { get; set; }
        public string AnalysisMethod { get; set; }
        public string AnalysisDateTimeUtc { get; set; }
        public string IsApprovedEPAMethod { get; set; }
        public string LimitBasis { get; set; }

        #endregion
    }
}