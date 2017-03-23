using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    /// <summary>
    /// TEMPORARY PLACEHOLDER FOR ACTUAL POCO CLASS -- PLEASE REPLACE WITH ACTUAL POCO WHEN AVAILABLE
    /// </summary>
    public class MonitoringPointParameterLimit
    {
        public int MonitoringPointParameterLimitId { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public int MonitoringPointId { get; set; }
        public virtual MonitoringPoint MonitoringPoint { get; set; }

        public int ParameterId { get; set; }
        public virtual Parameter Parameter { get; set; }
        public int CollectionMethodId { get; set; }
        public virtual CollectionMethod CollectionMethod { get; set; }
        public string IUSampleFrequency { get; set; }
        public bool IsRemoved { get; set; }
    }
}
