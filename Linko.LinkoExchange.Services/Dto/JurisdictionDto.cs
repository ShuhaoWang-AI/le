using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class JurisdictionDto
    {
        public int JurisdictionId { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
    }
}
