using System.Collections.Generic;
using System.Xml.Serialization;

namespace Linko.LinkoExchange.Services.Report.DataXML
{
    public class FileManifest
    {
        [XmlElement("Files")]
        public List<FileInfo> Files { get; set; }
    }
}