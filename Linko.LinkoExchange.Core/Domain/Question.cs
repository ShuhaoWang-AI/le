using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Core.Domain
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }
        public string Content { get; set; }
        public QuestionType QuestionType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationDateTime { get; set; }
        public DateTime LastModificationDateTime { get; set; }
        public int LastModifierUserId { get; set; }
    }
}
