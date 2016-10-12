using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class QuestionType
    {
        [Key]
        public int QuestionTypeId { get; set; }
        public string Name { get; set; }
    }
}
