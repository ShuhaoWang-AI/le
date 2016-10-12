using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class OrganizationRegulatoryProgram
    {
        [Key]
        public int OrganizationRegulatoryProgramId { get; set; }
        public RegulatoryProgram RegulatoryProgram { get; set; }
        public Organization Organization { get; set; }
        public Organization RegulatorOrganization { get; set; }
        public bool IsEnabled { get; set; }
        public List<OrganizationRegulatoryProgramSetting> OrganizationRegulatoryProgramSettings  { get; set; }
    }
}
