using System;
using System.Collections.Generic;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class EmailTemplate
    {
        public int Id { get; set; }
        public string EmailType { get; set; } 
        public string Template { get; set; }
    }
}
