using System.Xml.Serialization;

namespace Linko.LinkoExchange.Services.Report.DataXML
{
    [XmlType(TypeName = "SampleResult")]
    public class SampleResultNode
    {
        #region public properties
        
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