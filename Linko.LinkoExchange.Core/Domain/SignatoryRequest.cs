using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class SignatoryRequest
    {
        [Key]
        public int SignatoryRequestId { get; set; }
        public DateTime RequestDateTime { get; set; }
        public DateTime GrantDenyDateTime { get; set; }
        public DateTime RevokeDateTime { get; set; }
        public virtual OrganizationRegulatoryProgramUser OrganizationRegulatoryProgramUser { get; set; }
        public virtual SignatoryRequestStatus SignatoryRequestStatus { get; set; }
    }
}
