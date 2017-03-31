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
        public int MonitoringPointParameterId { get; set; }
        public virtual MonitoringPointParameter MonitoringPointParameter { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double? MinimumValue { get; set; }
        public double? MaximumValue { get; set; }
        public int BaseUnitId { get; set; }
        public virtual Unit BaseUnit { get; set; }
        public int CollectionMethodId { get; set; }
        public virtual CollectionMethod CollectionMethod { get; set; }
        public int LimitTypeId { get; set; }
        public virtual LimitType LimitType { get; set; }
        public int LimitBasisId { get; set; }
        public virtual LimitBasis LimitBasis { get; set; }
        public bool IsAlertsOnly { get; set; }
    }

}
