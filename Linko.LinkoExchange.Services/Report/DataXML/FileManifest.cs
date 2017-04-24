using System.Collections.Generic;
using System.Xml.Serialization;

namespace Linko.LinkoExchange.Services.Report.DataXML
{
    public class FileManifest
    {
        [XmlElement("File")]
        public List<FileInfo> Files { get; set; }
    }
}