using System.Collections.Generic;
using System.Xml.Serialization;

namespace Linko.LinkoExchange.Services.Report.DataXML
{
    public class FileManifest
    {
        #region public properties

        [XmlElement(elementName:"File")]
        public List<FileInfo> Files { get; set; } = new List<FileInfo>();

        #endregion
    }
}