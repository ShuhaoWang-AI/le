using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class SampleFrequency
    {
        public int SampleFrequencyId { get; set; }
        public int MonitoringPointParameterId { get; set; }
        public virtual MonitoringPointParameter MonitoringPointParameter { get; set; }
        public int CollectionMethodId { get; set; }
        public virtual CollectionMethod CollectionMethod { get; set; }
        public string IUSampleFrequency { get; set; }
        public string AuthoritySampleFrequency { get; set; }

    }
}
