using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class UserQuestionAnswer
    {
        [Key]
        public int UserQuestionAnswerId { get; set; }
        public string Content { get; set; }
        public int UserProfileId { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime LastModificationDateTime { get; set; }
        public virtual Question Question { get; set; }

    }
}
