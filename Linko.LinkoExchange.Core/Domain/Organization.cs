using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class Organization
    {
        public int OrgRegProgId { get; set; }
        public int RegProgId { get; set; }
        public int OrganizationId { get; set; }
        public int RegulatorOrgId { get; set; }
        public bool IsEnabled { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
    }

}
