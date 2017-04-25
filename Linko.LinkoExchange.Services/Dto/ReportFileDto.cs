using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ReportFileDto
    {
        public int ReportFileId { get; set; }
        public int ReportPackageElementTypeId { get; set; }
        public int FileStoreId { get; set; }
        public FileStoreDto FileStore { get; set; }
    }
}
