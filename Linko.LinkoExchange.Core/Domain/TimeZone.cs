using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{ 
    public class TimeZone
    {
        [Key]
        public int TimeZoneId { get; set; }
        public string Abbreviation { get; set; }
        public string Name { get; set; }
    }
}
