using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.User
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQuestionAnswerService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <param name="question"></param>
        /// <param name="answer"></param>
        void AddQuestionAnswerPair(int userProfileId, QuestionDto question, AnswerDto answer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="question"></param>
        void UpdateQuestion(QuestionDto question);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="answer"></param>
        void UpdateAnswer(AnswerDto answer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userQuestionAnswerId"></param>
        void DeleteQuestionAnswerPair(int userQuestionAnswerId);
    }
}
