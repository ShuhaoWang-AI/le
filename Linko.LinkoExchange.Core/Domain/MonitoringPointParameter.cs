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
    public class MonitoringPointParameter
    {
        public int MonitoringPointParameterId { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }
        public int MonitoringPointId { get; set; }
        public virtual MonitoringPoint MonitoringPoint { get; set; }
        public int ParameterId { get; set; }
        public virtual Parameter Parameter { get; set; }
        public int? DefaultUnitId { get; set; }
        public virtual Unit DefaultUnit { get; set; }
        public DateTimeOffset? EffectiveDateTimeUtc { get; set; }
        public DateTimeOffset? RetireDateTimeUtc { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }
        public virtual ICollection<MonitoringPointParameterLimit> MonitoringPointParameterLimits { get; set; }
    }
}
