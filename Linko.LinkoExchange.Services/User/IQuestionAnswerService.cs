using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.User
{
    public interface IQuestionAnswerService
    {
        void AddQuestionAnswerPair(int userProfileId, QuestionDto question, AnswerDto answer);

        void UpdateQuestion(QuestionDto question);

        void UpdateAnswer(AnswerDto answer);

        void DeleteQuestionAnswerPair(int userQuestionAnswerId);
    }
}
