using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class OrganizationRegulatoryProgramUser
    {
        [Key]
        public int OrganizationRegulatoryProgramUserId { get; set; }
        public int OrganizationRegulatoryProgramId { get; set; }
        public bool IsRegistrationApproved { get; set; }
        public bool IsRegistrationDenied { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsRemoved { get; set; }
        public bool IsSignatory { get; set; }
        public DateTime LastModificationDateTime { get; set; }
        public int LastModificationUserId { get; set; }
        public virtual UserProfile UserProfile { get; set; }
        public virtual OrganizationRegulatoryProgram OrganizationRegulatoryProgram { get; set; }
        public virtual PermissionGroup PermissionGroup { get; set; }
    }
}
