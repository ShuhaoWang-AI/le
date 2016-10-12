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
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public List<OrganizationSetting> OrganizationSettings { get; set; }
        public object AddressLine2 { get; set; }
        public object City { get; set; }
        public object ZipCode { get; set; }
    }

}
