using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class Organization
    {
        public int OrganizationId { get; set; }
        public int OrganizationTypeId { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public List<OrganizationSetting> OrganizationSettings { get; set; }
}

}
