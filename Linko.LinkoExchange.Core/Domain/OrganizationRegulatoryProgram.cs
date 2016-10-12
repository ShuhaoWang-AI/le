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
        public virtual RegulatoryProgram RegulatoryProgram { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual Organization RegulatorOrganization { get; set; }
        public bool IsEnabled { get; set; }
        public virtual List<OrganizationRegulatoryProgramSetting> OrganizationRegulatoryProgramSettings  { get; set; }
    }
}
