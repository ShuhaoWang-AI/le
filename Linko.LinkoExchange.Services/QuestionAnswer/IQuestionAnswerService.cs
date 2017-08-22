using System.Collections.Generic;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    /// <summary>
    /// 
    /// </summary>
    public interface IQuestionAnswerService
    {
        /// <summary>
        ///     Adds a question if no Id's are provided or updates if Id's are present
        /// </summary>
        /// <param name="userProfileId"> </param>
        /// <param name="questionAnswer"> </param>
        void CreateOrUpdateUserQuestionAnswer(int userProfileId, AnswerDto questionAnswer);

        /// <summary>
        ///     Takes a collections of a user's answers to questions and creates new objects in the database
        ///     if answers do not already exist or updates existing answers if they exist.
        ///     Email communications are sent to stakeholders and the activity is logged for Cromerr purposes.
        /// </summary>
        /// <param name="userProfileId"> </param>
        /// <param name="questionAnswers"> </param>
        /// <returns> </returns>
        CreateOrUpdateAnswersResult CreateOrUpdateUserQuestionAnswers(int userProfileId, ICollection<AnswerDto> questionAnswers);

        /// <summary>
        ///     Add a collection of question and answer pairs.
        /// </summary>
        /// <param name="userProfileId"> </param>
        /// <param name="questionAnswers"> </param>
        void CreateUserQuestionAnswers(int userProfileId, IEnumerable<AnswerDto> questionAnswers);

        /// <summary>
        ///     Overwrites a saved answer to a question. Answers for security questions are encrypted and
        ///     answers for knowledge based questions are hashed.
        /// </summary>
        /// <param name="answer"> Must have UserQuestionAnswerId property set. </param>
        void UpdateAnswer(AnswerDto answer);

        /// <summary>
        ///     Delete all user's KBQ an Security Questions and answers
        /// </summary>
        /// <param name="userProfileId"> </param>
        void DeleteUserQuestionAndAnswers(int userProfileId);

        /// <summary>
        ///     Gets all a user's saved question/answers of a specified type. Security question answers are decrypted.
        /// </summary>
        /// <param name="userProfileId"> </param>
        /// <param name="questionType"> </param>
        /// <returns> </returns>
        ICollection<QuestionAnswerPairDto> GetUsersQuestionAnswers(int userProfileId, QuestionTypeName questionType);

        /// <summary>
        ///     Finds a user profile associated with the passed in token (from tEmailAuditLog).
        ///     Returns a random question/answer pair selection from amongst the user's saved question/answers.
        /// </summary>
        /// <param name="token"> </param>
        /// <param name="questionType"> </param>
        /// <returns> </returns>
        QuestionAnswerPairDto GetRandomQuestionAnswerFromToken(string token, QuestionTypeName questionType);

        /// <summary>
        ///     Returns a random question/answer pair selection from amongst a user's saved question/answers.
        /// </summary>
        /// <param name="userProfileId"> </param>
        /// <param name="questionType"> Security question or knowledge-based question. </param>
        /// <returns> </returns>
        QuestionAnswerPairDto GetRandomQuestionAnswerFromUserProfileId(int userProfileId, QuestionTypeName questionType);

        /// <summary>
        ///     Returns all active questions in the database.
        /// </summary>
        /// <returns> </returns>
        ICollection<QuestionDto> GetQuestions();

        /// <summary>
        ///     Compares the passed in string answer with the stored answer in the database.
        /// </summary>
        /// <param name="userQuestionAnswerId"> tUserQuestionAnswer.UserQuestionAnswerId </param>
        /// <param name="answer"> Readable string answer being tested against the actual hashed (KBQ) or encrypted (SQ) answer in the database. </param>
        /// <returns> True is answer is correct. False otherwise. </returns>
        bool ConfirmCorrectAnswer(int userQuestionAnswerId, string answer);

        /// <summary>
        ///     - Check for duplicated knowledge-based questions
        ///     - Check for duplicated knowledge-based question answers
        ///     - Check that knowledge-based questions have answers
        /// </summary>
        /// <param name="kbqQuestions"> </param>
        /// <returns> </returns>
        RegistrationResult ValidateUserKbqData(IEnumerable<AnswerDto> kbqQuestions);

        /// <summary>
        ///     - Check for duplicated security questions
        ///     - Check for duplicated security question answers
        ///     - Check that security questions have answers
        /// </summary>
        /// <param name="securityQuestions"> </param>
        /// <returns> </returns>
        RegistrationResult ValidateUserSqData(IEnumerable<AnswerDto> securityQuestions);
    }
}