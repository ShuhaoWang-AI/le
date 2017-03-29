using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class MonitoringPointDto
    {
        public int MonitoringPointId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsRemoved { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }
        public virtual ICollection<ParameterDto> Parameters { get; set; }
    }
}
