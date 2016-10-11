using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class Question
    {
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public int QuestionTypeId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime LastModificationDateTime { get; set; }
        public int LastModifierUserId { get; set; }
    }
}
