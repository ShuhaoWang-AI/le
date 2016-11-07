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
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Microsoft.AspNet.Identity;
using Moq;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Email;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class QuestionAnswerServiceTests
    {
        private QuestionAnswerService _questionAnswerService;
        private EncryptionService _encrypter;
        Mock<ISettingService> _settings = new Mock<ISettingService>();
        Mock<IOrganizationService> _orgService = new Mock<IOrganizationService>();
        Mock<IEmailService> _emailService = new Mock<IEmailService>();

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
            _questionAnswerService = new QuestionAnswerService(new LinkoExchangeContext(connectionString),
                                                               new EmailAuditLogEntryDto(),
                                                               new HttpContextService(),
                                                               new EncryptionService(),
                                                               new PasswordHasher(),
                                                               _settings.Object,
                                                               _orgService.Object,
                                                               _emailService.Object
                                                               );
            _encrypter = new EncryptionService();
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

        [TestMethod]
        public void GetUsersQuestionAnswers()
        {
            var results = _questionAnswerService.GetUsersQuestionAnswers(1, QuestionType.KnowledgeBased);
        }


        [TestMethod]
        public void EncryptDecryptTest()
        {
            var answer = "Test answer";
            var encryptedString = _encrypter.EncryptString(answer);
            var deccryptedString = _encrypter.DecryptString(encryptedString);

            Assert.AreEqual(answer, deccryptedString);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_Duplicate_Answers_Test()
        {
            var qAndAs = new List<QuestionAnswerPairDto>();
            var q1 = new QuestionDto() { QuestionType = QuestionType.KnowledgeBased, Content = "What color is your car?" };
            var a1 = new AnswerDto() { Content = "Red" };
            var q2 = new QuestionDto() { QuestionType = QuestionType.KnowledgeBased, Content = "What color is your bike?" };
            var a2 = new AnswerDto() { Content = "Red" };
            qAndAs.Add(new QuestionAnswerPairDto() { Question = q1, Answer = a1 });
            qAndAs.Add(new QuestionAnswerPairDto() { Question = q2, Answer = a2 });

            var result = _questionAnswerService.CreateOrUpdateQuestionAnswerPairs(1, qAndAs);

            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_Duplicate_Questions_Test()
        {
            var qAndAs = new List<QuestionAnswerPairDto>();
            var q1 = new QuestionDto() { QuestionType = QuestionType.KnowledgeBased, Content = "What color is your dog?" };
            var a1 = new AnswerDto() { Content = "Black" };
            var q2 = new QuestionDto() { QuestionType = QuestionType.KnowledgeBased, Content = "What color is your dog?" };
            var a2 = new AnswerDto() { Content = "Brown" };
            qAndAs.Add(new QuestionAnswerPairDto() { Question = q1, Answer = a1 });
            qAndAs.Add(new QuestionAnswerPairDto() { Question = q2, Answer = a2 });

            var result = _questionAnswerService.CreateOrUpdateQuestionAnswerPairs(1, qAndAs);

            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_Duplicate_Questions_Different_TypesTest()
        {
            var qAndAs = new List<QuestionAnswerPairDto>();
            var q1 = new QuestionDto() { QuestionType = QuestionType.KnowledgeBased, Content = "What color is your dog?" };
            var a1 = new AnswerDto() { Content = "Black" };
            var q2 = new QuestionDto() { QuestionType = QuestionType.Security, Content = "What color is your dog?" };
            var a2 = new AnswerDto() { Content = "Black" };
            qAndAs.Add(new QuestionAnswerPairDto() { Question = q1, Answer = a1 });
            qAndAs.Add(new QuestionAnswerPairDto() { Question = q2, Answer = a2 });

            var result = _questionAnswerService.CreateOrUpdateQuestionAnswerPairs(1, qAndAs);

            Assert.AreEqual(result, true);
        }
    }
}
