using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ReportSampleDto
    {
        public int ReportSampleId { get; set; }
        public int ReportPackageElementTypeId { get; set; }
        public int SampleId { get; set; }
        public SampleDto Sample { get; set; }
    }
}
