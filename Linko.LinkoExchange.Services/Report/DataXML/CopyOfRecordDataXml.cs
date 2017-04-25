using System.Collections.Generic;
using System.Xml.Serialization;

namespace Linko.LinkoExchange.Services.Report.DataXML
{
    [XmlRoot("Root")]
    public class CopyOfRecordDataXml
    {
        public XmlFileVersion XmlFileVersion { get; set; }
        public ReportHeader ReportHeader { get; set; }
        public SubmittedTo SubmittedTo { get; set; }
        public SubmittedOnBehalfOf SubmittedOnBehalfOf { get; set; }
        public SubmittedBy SubmittedBy { get; set; }
        public FileManifest FileManifest { get; set; }
        public List<Certification> Certifications { get; set; }
        public string Comment { get; set; }
        public List<SampleNode> Samples { get; set; }
    }
}
