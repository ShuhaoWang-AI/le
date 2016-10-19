using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class Organization
    {
        [Key]
        public int OrganizationId { get; set; }
        public int OrganizationTypeId { get; set; }
        public virtual OrganizationType OrganizationType { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public virtual List<OrganizationSetting> OrganizationSettings { get; set; }

    }

}
