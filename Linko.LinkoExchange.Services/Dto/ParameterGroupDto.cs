using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ParameterGroupDto
    {
        public int? ParameterGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public OrganizationRegulatoryProgramDto OrganizationRegulatoryProgram { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }
        public virtual ICollection<ParameterDto> Parameters { get; set; }
    }
}
