using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.QuestionAnswer
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
        void AddUserQuestionAnswer(int userProfileId, AnswerDto answer);

        /// <summary>
        /// Adds a question if no Id's are provided or updates if Id's are present
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <param name="questionAnswer"></param>
        void CreateOrUpdateUserQuestionAnswer(int userProfileId, AnswerDto questionAnswer);

        CreateOrUpdateAnswersResult CreateOrUpdateUserQuestionAnswers(int userProfileId, ICollection<AnswerDto> questionAnswers);

        /// <summary>
        /// Add a collection of question and answer pairs.
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <param name="questionAnswers"></param>
        void CreateUserQuestionAnswers(int userProfileId, IEnumerable<AnswerDto> questionAnswers);

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
        void DeleteUserQuestionAnswer(int userQuestionAnswerId);

        /// <summary>
        /// Delete all user's KBQ an Security Questions and answsers
        /// </summary>
        /// <param name="userProfileId"></param>
        void DeleteUserQuestionAndAnswers(int userProfileId);

        ICollection<QuestionAnswerPairDto> GetUsersQuestionAnswers(int userProfileId, QuestionTypeName questionType);

        QuestionAnswerPairDto GetRandomQuestionAnswerFromToken(string token, QuestionTypeName questionType);

        QuestionAnswerPairDto GetRandomQuestionAnswerFromUserProfileId(int userProfileId, QuestionTypeName questionType);

        ICollection<QuestionDto> GetQuestions();

        bool ConfirmCorrectAnswer(int userQuestionAnswerId, string answer);

        RegistrationResult ValidateUserKbqData(IEnumerable<AnswerDto> kbqQuestions);

        RegistrationResult ValidateUserSqData(IEnumerable<AnswerDto> securityQuestions); 
    }
}
