using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class RepudiationReasonDto
    {
        public int RepudiationReasonId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationLocalDateTime { get; set; }
        public DateTime? LastModificationLocalDateTime { get; set; }
    }
}
