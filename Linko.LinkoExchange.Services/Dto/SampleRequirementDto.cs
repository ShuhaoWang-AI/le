using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class SampleRequirementDto
    {
        public int MonitoringPointId { get; set; }
        public string MonitoringPointName { get; set; }
        public int ParameterId { get; set; }
        public string ParameterName { get; set; }
        public int TotalSamplesRequiredCount { get; set; }
    }
}
