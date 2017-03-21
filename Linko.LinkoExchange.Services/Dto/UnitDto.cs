using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class UnitDto
    {
        public int UnitId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsFlowUnit { get; set; }
        public int OrganizationId { get; set; }
        public bool IsRemoved { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }
    }
}
