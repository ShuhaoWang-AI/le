using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Dto
{
    public class QuestionDto
    {
        public int? QuestionId { get; set; }
        public int QuestionTypeId { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
    }

    public class AnswerDto
    {
        public int? UserQuestionAnswerId { get; set; }
        public string Content { get; set; }

    }

}
