﻿using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

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
        void AddQuestionAnswerPair(int userProfileId, QuestionDto question, AnswerDto answer);

        /// <summary>
        /// Adds a question if no Id's are provided or updates if Id's are present
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <param name="questionAnswer"></param>
        void CreateOrUpdateQuestionAnswerPair(int userProfileId, QuestionAnswerPairDto questionAnswer);

        bool CreateOrUpdateQuestionAnswerPairs(int userProfileId, ICollection<QuestionAnswerPairDto> questionAnswers);

        /// <summary>
        /// Add a collection of question and answer pairs.
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <param name="questionAnswers"></param>
        void CreateQuestionAnswerPairs(int userProfileId, IEnumerable<QuestionAnswerPairDto> questionAnswers);

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

        ICollection<QuestionAnswerPairDto> GetUsersQuestionAnswers(int userProfileId, Dto.QuestionType questionType);

        QuestionAnswerPairDto GetRandomQuestionAnswerFromToken(string token, Dto.QuestionType questionType);

        QuestionAnswerPairDto GetRandomQuestionAnswerFromUserProfileId(int userProfileId, Dto.QuestionType questionType);

    }
}
