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
        public int RegulatoryProgramId { get; set; }
        [Required]
        public virtual RegulatoryProgram RegulatoryProgram { get; set; }
        public int OrganizationId { get; set; }
        [Required]
        public virtual Organization Organization { get; set; }
        public int? RegulatorOrganizationId { get; set; }
        [Required]
        public virtual Organization RegulatorOrganization { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsRemoved { get; set; }

        public string AssignedTo { get; set; }
    }
}
