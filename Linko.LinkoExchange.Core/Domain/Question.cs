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
        public int QuestionTypeId { get; set; }
        public virtual QuestionType QuestionType { get; set; }
        public bool IsActive { get; set; }
        public DateTimeOffset CreationDateTimeUtc { get; set; }
        public DateTimeOffset? LastModificationDateTimeUtc { get; set; }
        public int? LastModifierUserId { get; set; }
    }
}
