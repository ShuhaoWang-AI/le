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
    public class LimitBasis
    {
        public int LimitBasisId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }
        public virtual ICollection<MonitoringPointParameterLimit> MonitoringPointParameterLimits_LimitBasis { get; set; }
    }
}
