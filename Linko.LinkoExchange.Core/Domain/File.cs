using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// The class definition for tFile
    /// </summary>
    public partial class LinkExchangeFile
    {
        public int FileId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OrigianName { get; set; }
        public long SizeByte { get; set; }
        public int OrgnizationRegulatoryProgramId { get; set; }
        public bool IsReported { get; set; }
        public DateTimeOffset UploadDateTimeUtc { get; set; }
        public int UploaderUserId { get; set; }
    }
}
