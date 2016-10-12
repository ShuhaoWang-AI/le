using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class PermissionGroup
    {
        [Key]
        public int PermissionGroupId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime LastModificationDateTime { get; set; }

        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }

    }
}
