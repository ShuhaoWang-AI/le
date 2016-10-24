using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Settings;
using AutoMapper;
using Linko.LinkoExchange.Services.AutoMapperProfile;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;
using System.Configuration;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class QuestionAnswerServiceTests
    {
        private QuestionAnswerService _questionAnswerService;

        public QuestionAnswerServiceTests()
        {
            Mapper.Initialize(cfg =>
            {
                //cfg.AddProfile(new UserMapProfile());
                //cfg.AddProfile(new UserQuestionAnswerMapProfile());
            });

            Mapper.AssertConfigurationIsValid();
        }

        [TestInitialize]
        public void Initialize()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _questionAnswerService = new QuestionAnswerService(new LinkoExchangeContext(connectionString), new EmailAuditLogEntryDto(), new CurrentUser());
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPair()
        {
            var questionDto = new QuestionDto();
            questionDto.QuestionType = QuestionType.KnowledgeBased;
            questionDto.IsActive = true;
            questionDto.Content = "What color is your car?";
            //questionDto.QuestionId = 8;
            
            var answerDto = new AnswerDto();
            answerDto.Content = "Blue";
            //answerDto.UserQuestionAnswerId = 6;

            var questionAnswersPair = new QuestionAnswerPairDto() { Question = questionDto, Answer = answerDto };
            _questionAnswerService.CreateOrUpdateQuestionAnswerPair(1, questionAnswersPair);

        }

        [TestMethod]
        public void DeleteQuestionAnswerPair()
        {
            _questionAnswerService.DeleteQuestionAnswerPair(12);
        }
        [TestMethod]
        public void UpdateAnswer()
        {
            _questionAnswerService.UpdateAnswer(new AnswerDto() { UserQuestionAnswerId = 8, Content = "Purple" });
        }

        [TestMethod]
        public void UpdateQuestion()
        {
            _questionAnswerService.UpdateQuestion(
                new QuestionDto() { QuestionId = 10,
                    QuestionType = QuestionType.KnowledgeBased,
                    IsActive = true,                
                    Content = "Color of your first bike?" });
        }

    }
}
