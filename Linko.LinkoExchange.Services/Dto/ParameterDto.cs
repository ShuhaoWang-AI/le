using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class ParameterDto
    {
        public int ParameterId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public UnitDto DefaultUnit { get; set; }
        public double? TrcFactor { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public bool IsRemoved { get; set; }
        public DateTime LastModificationDateTimeLocal { get; set; }
        public string LastModifierFullName { get; set; }
    }
}
