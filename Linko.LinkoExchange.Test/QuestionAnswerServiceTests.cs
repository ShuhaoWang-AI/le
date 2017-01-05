using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Linko.LinkoExchange.Services.Settings;
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
using Linko.LinkoExchange.Core.Enum;
using NLog;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.AuditLog;

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
        private IPasswordHasher _passwordHasher;
        Mock<ISessionCache> _sessionCache;
        Mock<IMapHelper> _mapHelper;
        Mock<ICromerrAuditLogService> _crommerAuditLogService;

        public QuestionAnswerServiceTests()
        {
        }

        [TestInitialize]
        public void Initialize()
        {

            Dictionary<SystemSettingType, string> globalSettings = new Dictionary<SystemSettingType, string>();
            globalSettings.Add(SystemSettingType.SupportPhoneNumber, "555-555-5555");
            globalSettings.Add(SystemSettingType.SupportEmailAddress, "test@test.com");
            _settings.Setup(s => s.GetGlobalSettings()).Returns(globalSettings);

            _sessionCache = new Mock<ISessionCache>();
            _mapHelper = new Mock<IMapHelper>();
            _crommerAuditLogService = new Mock<ICromerrAuditLogService>();

            var connectionString = ConfigurationManager.ConnectionStrings["LinkoExchangeContext"].ConnectionString;
            _questionAnswerService = new QuestionAnswerService(new LinkoExchangeContext(connectionString),
                                                               new Mock<ILogger>().Object,
                                                               new HttpContextService(),
                                                               new EncryptionService(),
                                                               new PasswordHasher(),
                                                               _settings.Object,
                                                               _orgService.Object,
                                                               _emailService.Object,
                                                               _sessionCache.Object,
                                                               _mapHelper.Object,
                                                               _crommerAuditLogService.Object
                                                               );
            _encrypter = new EncryptionService();
            _passwordHasher = new PasswordHasher();
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPair()
        {
            
            var answerDto = new AnswerDto();
            answerDto.Content = "Blue";
            answerDto.QuestionId = 8;

            _questionAnswerService.CreateOrUpdateUserQuestionAnswer(1, answerDto);

        }

        [TestMethod]
        public void DeleteQuestionAnswerPair()
        {
            _questionAnswerService.DeleteUserQuestionAnswer(4);
        }
        [TestMethod]
        public void UpdateAnswer()
        {
            _questionAnswerService.UpdateAnswer(new AnswerDto() { UserQuestionAnswerId = 36, Content = "test answer" });
        }

        [TestMethod]
        public void UpdateQuestion()
        {
            _questionAnswerService.UpdateQuestion(
                new QuestionDto() { QuestionId = 10,
                    QuestionType = QuestionTypeName.KBQ,
                    IsActive = true,                
                    Content = "Color of your first bike?" });
        }

        [TestMethod]
        public void GetUsersQuestionAnswers()
        {
            var results = _questionAnswerService.GetUsersQuestionAnswers(1, QuestionTypeName.KBQ);
        }


        [TestMethod]
        public void EncryptDecryptTest()
        {
            var answer = "Second test answer";
            answer = answer.Trim();
            var encryptedString = _encrypter.EncryptString(answer.Trim());
            var deccryptedString = _encrypter.DecryptString(encryptedString);

            Assert.AreEqual(answer, deccryptedString);
        }

        [TestMethod]
        public void StringHashTest()
        {
            var answer = "Hash tHiS answer";
            answer = answer.Trim().ToLower();
            var hashedString = _passwordHasher.HashPassword(answer);
            var result = _passwordHasher.VerifyHashedPassword(hashedString, answer);

            Assert.AreEqual(result, PasswordVerificationResult.Success);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_Duplicate_Answers_Test()
        {
            var qAndAs = new List<AnswerDto>();
            var a1 = new AnswerDto() { Content = "Red" };
            var a2 = new AnswerDto() { Content = "Red" };
            qAndAs.Add(a1);
            qAndAs.Add(a2);

            var result = _questionAnswerService.CreateOrUpdateUserQuestionAnswers(1, qAndAs);

            Assert.AreEqual(result, CreateOrUpdateAnswersResult.DuplicateAnswersInNew);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_KBQ_Only_SomeDirtySomeClean_NoDuplicates_Test()
        {
            var qAndAs = new List<AnswerDto>();
            var a1 = new AnswerDto() { UserQuestionAnswerId = 49, QuestionId = 8, Content = "New answer 1" };
            var a2 = new AnswerDto() { UserQuestionAnswerId = 50, QuestionId = 20, Content = "New answer 2" };
            var a3 = new AnswerDto() { UserQuestionAnswerId = 51, QuestionId = 21, Content = "JcW2LFIrvNo7AIF13kWOG4Tbo3781pYr42DEsEX+8Hl9zx2zhGkE7goRceMe1+eNcmjZVuwznkiDgZLzyvGwgoF+4K0wTGbrvAVsV+6RB0EM+UkO0JnTY9TCh096SQ4h" };
            var a4 = new AnswerDto() { UserQuestionAnswerId = 52, QuestionId = 5, Content = "ABT8jI4kSpK7I5mSPBl1jzkKD+FM/Sj44JKmUSRibf+cKA5kAcYVByyGGTFbCQ5sJA==" };
            qAndAs.Add(a1);
            qAndAs.Add(a2);
            qAndAs.Add(a3);
            qAndAs.Add(a4);

            var result = _questionAnswerService.CreateOrUpdateUserQuestionAnswers(1, qAndAs);

            Assert.AreEqual(result, CreateOrUpdateAnswersResult.Success);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_KBQ_Only_SomeDirtySomeClean_WithDuplicates_Test()
        {
            var qAndAs = new List<AnswerDto>();
            var a1 = new AnswerDto() { UserQuestionAnswerId = 44, QuestionId = 8, Content = "Same answer" };
            var a2 = new AnswerDto() { UserQuestionAnswerId = 45, QuestionId = 20, Content = "Same answer" };
            var a3 = new AnswerDto() { UserQuestionAnswerId = 46, QuestionId = 21, Content = "AEFWE9YatSU7u5fxOhKxYmD5MPXU38Jzvx2fekf+4S0SM3rJg4hbYZa2/eE/hf2pyw==" };
            var a4 = new AnswerDto() { UserQuestionAnswerId = 47, QuestionId = 5, Content = "AOXH+5TNf1zZIT8rJvbCqwVqDx3z2kbOssWb68035X/2eflFjxs3ceIR4ENGEoHCjw==" };
            qAndAs.Add(a1);
            qAndAs.Add(a2);
            qAndAs.Add(a3);
            qAndAs.Add(a4);

            var result = _questionAnswerService.CreateOrUpdateUserQuestionAnswers(1, qAndAs);

            Assert.AreEqual(result, CreateOrUpdateAnswersResult.DuplicateAnswersInNew);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_AllNew_NoDuplicates_Test()
        {
            var qAndAs = new List<AnswerDto>();
            var a1 = new AnswerDto() { QuestionId = 8, Content = "New answer 1" };
            var a2 = new AnswerDto() { QuestionId = 20, Content = "New answer 2" };
            var a3 = new AnswerDto() { QuestionId = 21, Content = "New answer 3" };
            var a4 = new AnswerDto() { QuestionId = 5, Content = "New answer 4" };
            qAndAs.Add(a1);
            qAndAs.Add(a2);
            qAndAs.Add(a3);
            qAndAs.Add(a4);

            var result = _questionAnswerService.CreateOrUpdateUserQuestionAnswers(1, qAndAs);

            Assert.AreEqual(result, CreateOrUpdateAnswersResult.Success);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_AllNew_WithDuplicates_Test()
        {
            var qAndAs = new List<AnswerDto>();
            var a1 = new AnswerDto() { UserQuestionAnswerId = 44, QuestionId = 8, Content = "New answer 1" };
            var a2 = new AnswerDto() { UserQuestionAnswerId = 45, QuestionId = 20, Content = "New answer 1" };
            var a3 = new AnswerDto() { UserQuestionAnswerId = 46, QuestionId = 21, Content = "New answer 3" };
            var a4 = new AnswerDto() { UserQuestionAnswerId = 47, QuestionId = 5, Content = "New answer 4" };
            qAndAs.Add(a1);
            qAndAs.Add(a2);
            qAndAs.Add(a3);
            qAndAs.Add(a4);

            var result = _questionAnswerService.CreateOrUpdateUserQuestionAnswers(1, qAndAs);

            Assert.AreEqual(result, CreateOrUpdateAnswersResult.DuplicateAnswersInNew);
        }


        [TestMethod]
        public void ConfirmCorrectKBQAnswer_Test()
        {
            string phrase = "NeW aNSWer 1";
            _questionAnswerService.UpdateAnswer(new AnswerDto() { UserQuestionAnswerId = 44, Content = phrase });
            var result = _questionAnswerService.ConfirmCorrectAnswer(44, phrase);

            Assert.IsTrue(result);
        }

    }
}
