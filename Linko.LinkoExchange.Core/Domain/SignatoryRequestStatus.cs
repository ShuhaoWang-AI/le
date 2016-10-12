using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class SignatoryRequestStatus
    {
        [Key]
        public int SignatoryRequestStatusId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
