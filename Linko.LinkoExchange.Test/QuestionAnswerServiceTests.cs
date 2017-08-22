using System.Collections.Generic;
using System.Configuration;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.AspNet.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Linko.LinkoExchange.Test
{
    [TestClass]
    public class QuestionAnswerServiceTests
    {
        #region fields

        private Mock<ICromerrAuditLogService> _crommerAuditLogService;
        private EncryptionService _encrypter;
        private readonly Mock<ILinkoExchangeEmailService> _linkoExchangeEmailService = new Mock<ILinkoExchangeEmailService>();
        private Mock<IMapHelper> _mapHelper;
        private readonly Mock<IOrganizationService> _orgService = new Mock<IOrganizationService>();
        private IPasswordHasher _passwordHasher;
        private QuestionAnswerService _questionAnswerService;
        private Mock<ISessionCache> _sessionCache;
        private readonly Mock<ISettingService> _settings = new Mock<ISettingService>();

        #endregion

        #region constructors and destructor

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            var globalSettings = new Dictionary<SystemSettingType, string>();
            globalSettings.Add(key:SystemSettingType.SupportPhoneNumber, value:"555-555-5555");
            globalSettings.Add(key:SystemSettingType.SupportEmailAddress, value:"test@test.com");
            _settings.Setup(s => s.GetGlobalSettings()).Returns(value:globalSettings);

            _sessionCache = new Mock<ISessionCache>();
            _mapHelper = new Mock<IMapHelper>();
            _crommerAuditLogService = new Mock<ICromerrAuditLogService>();

            var connectionString = ConfigurationManager.ConnectionStrings[name:"LinkoExchangeContext"].ConnectionString;
            _questionAnswerService = new QuestionAnswerService(dbContext:new LinkoExchangeContext(nameOrConnectionString:connectionString),
                                                               httpContext:new HttpContextService(),
                                                               encryption:new EncryptionService(),
                                                               passwordHasher:new PasswordHasher(),
                                                               settingService:_settings.Object,
                                                               orgService:_orgService.Object,
                                                               crommerAuditLogService:_crommerAuditLogService.Object,
                                                               linkoExchangeEmailService:_linkoExchangeEmailService.Object
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

            _questionAnswerService.CreateOrUpdateUserQuestionAnswer(userProfileId:1, answerDto:answerDto);
        }

        [TestMethod]
        public void UpdateAnswer()
        {
            _questionAnswerService.UpdateAnswer(answer:new AnswerDto {UserQuestionAnswerId = 36, Content = "test answer"});
        }

        [TestMethod]
        public void GetUsersQuestionAnswers()
        {
            var results = _questionAnswerService.GetUsersQuestionAnswers(userProfileId:1, questionType:QuestionTypeName.KBQ);
        }

        [TestMethod]
        public void EncryptDecryptTest()
        {
            var answer = "Second test answer";
            answer = answer.Trim();
            var encryptedString = _encrypter.EncryptString(readableString:answer.Trim());
            var deccryptedString = _encrypter.DecryptString(encryptedString:encryptedString);

            Assert.AreEqual(expected:answer, actual:deccryptedString);
        }

        [TestMethod]
        public void StringHashTest()
        {
            var answer = "Hash tHiS answer";
            answer = answer.Trim().ToLower();
            var hashedString = _passwordHasher.HashPassword(password:answer);
            var result = _passwordHasher.VerifyHashedPassword(hashedPassword:hashedString, providedPassword:answer);

            Assert.AreEqual(expected:result, actual:PasswordVerificationResult.Success);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_Duplicate_Answers_Test()
        {
            var qAndAs = new List<AnswerDto>();
            var a1 = new AnswerDto {Content = "Red"};
            var a2 = new AnswerDto {Content = "Red"};
            qAndAs.Add(item:a1);
            qAndAs.Add(item:a2);

            var result = _questionAnswerService.CreateOrUpdateUserQuestionAnswers(userProfileId:1, questionAnswers:qAndAs);

            Assert.AreEqual(expected:result, actual:CreateOrUpdateAnswersResult.DuplicateAnswersInNew);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_KBQ_Only_SomeDirtySomeClean_NoDuplicates_Test()
        {
            var qAndAs = new List<AnswerDto>();
            var a1 = new AnswerDto {UserQuestionAnswerId = 49, QuestionId = 8, Content = "New answer 1"};
            var a2 = new AnswerDto {UserQuestionAnswerId = 50, QuestionId = 20, Content = "New answer 2"};
            var a3 = new AnswerDto
                     {
                         UserQuestionAnswerId = 51,
                         QuestionId = 21,
                         Content = "JcW2LFIrvNo7AIF13kWOG4Tbo3781pYr42DEsEX+8Hl9zx2zhGkE7goRceMe1+eNcmjZVuwznkiDgZLzyvGwgoF+4K0wTGbrvAVsV+6RB0EM+UkO0JnTY9TCh096SQ4h"
                     };
            var a4 = new AnswerDto {UserQuestionAnswerId = 52, QuestionId = 5, Content = "ABT8jI4kSpK7I5mSPBl1jzkKD+FM/Sj44JKmUSRibf+cKA5kAcYVByyGGTFbCQ5sJA=="};
            qAndAs.Add(item:a1);
            qAndAs.Add(item:a2);
            qAndAs.Add(item:a3);
            qAndAs.Add(item:a4);

            var result = _questionAnswerService.CreateOrUpdateUserQuestionAnswers(userProfileId:1, questionAnswers:qAndAs);

            Assert.AreEqual(expected:result, actual:CreateOrUpdateAnswersResult.Success);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_KBQ_Only_SomeDirtySomeClean_WithDuplicates_Test()
        {
            var qAndAs = new List<AnswerDto>();
            var a1 = new AnswerDto {UserQuestionAnswerId = 44, QuestionId = 8, Content = "Same answer"};
            var a2 = new AnswerDto {UserQuestionAnswerId = 45, QuestionId = 20, Content = "Same answer"};
            var a3 = new AnswerDto {UserQuestionAnswerId = 46, QuestionId = 21, Content = "AEFWE9YatSU7u5fxOhKxYmD5MPXU38Jzvx2fekf+4S0SM3rJg4hbYZa2/eE/hf2pyw=="};
            var a4 = new AnswerDto {UserQuestionAnswerId = 47, QuestionId = 5, Content = "AOXH+5TNf1zZIT8rJvbCqwVqDx3z2kbOssWb68035X/2eflFjxs3ceIR4ENGEoHCjw=="};
            qAndAs.Add(item:a1);
            qAndAs.Add(item:a2);
            qAndAs.Add(item:a3);
            qAndAs.Add(item:a4);

            var result = _questionAnswerService.CreateOrUpdateUserQuestionAnswers(userProfileId:1, questionAnswers:qAndAs);

            Assert.AreEqual(expected:result, actual:CreateOrUpdateAnswersResult.DuplicateAnswersInNew);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_AllNew_NoDuplicates_Test()
        {
            var qAndAs = new List<AnswerDto>();
            var a1 = new AnswerDto {QuestionId = 8, Content = "New answer 1"};
            var a2 = new AnswerDto {QuestionId = 20, Content = "New answer 2"};
            var a3 = new AnswerDto {QuestionId = 21, Content = "New answer 3"};
            var a4 = new AnswerDto {QuestionId = 5, Content = "New answer 4"};
            qAndAs.Add(item:a1);
            qAndAs.Add(item:a2);
            qAndAs.Add(item:a3);
            qAndAs.Add(item:a4);

            var result = _questionAnswerService.CreateOrUpdateUserQuestionAnswers(userProfileId:1, questionAnswers:qAndAs);

            Assert.AreEqual(expected:result, actual:CreateOrUpdateAnswersResult.Success);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_AllNew_WithDuplicates_Test()
        {
            var qAndAs = new List<AnswerDto>();
            var a1 = new AnswerDto {UserQuestionAnswerId = 44, QuestionId = 8, Content = "New answer 1"};
            var a2 = new AnswerDto {UserQuestionAnswerId = 45, QuestionId = 20, Content = "New answer 1"};
            var a3 = new AnswerDto {UserQuestionAnswerId = 46, QuestionId = 21, Content = "New answer 3"};
            var a4 = new AnswerDto {UserQuestionAnswerId = 47, QuestionId = 5, Content = "New answer 4"};
            qAndAs.Add(item:a1);
            qAndAs.Add(item:a2);
            qAndAs.Add(item:a3);
            qAndAs.Add(item:a4);

            var result = _questionAnswerService.CreateOrUpdateUserQuestionAnswers(userProfileId:1, questionAnswers:qAndAs);

            Assert.AreEqual(expected:result, actual:CreateOrUpdateAnswersResult.DuplicateAnswersInNew);
        }

        [TestMethod]
        public void ConfirmCorrectKBQAnswer_Test()
        {
            var phrase = "NeW aNSWer 1";
            _questionAnswerService.UpdateAnswer(answer:new AnswerDto {UserQuestionAnswerId = 44, Content = phrase});
            var result = _questionAnswerService.ConfirmCorrectAnswer(userQuestionAnswerId:44, answer:phrase);

            Assert.IsTrue(condition:result);
        }

        [TestMethod]
        public void ValidateUserSqData_Test()
        {
            var answerDtos = new List<AnswerDto>();
            answerDtos.Add(item:new AnswerDto {QuestionId = 1, Content = "test answer"});
            answerDtos.Add(item:new AnswerDto {QuestionId = 2, Content = "Test answer"});
            var result = _questionAnswerService.ValidateUserSqData(securityQuestions:answerDtos);

            Assert.AreEqual(expected:result, actual:RegistrationResult.DuplicatedSecurityQuestionAnswer);
        }

        [TestMethod]
        public void CreateOrUpdateQuestionAnswerPairs_Reordered_Question_Answers_Test()
        {
            var qAndAs = new List<AnswerDto>();

            //var sq1 = new AnswerDto() { UserQuestionAnswerId = 95, QuestionId = 21, Content = "yzdmYW32U1LNjiFKvB4d5qZVb4g15GqE9f6fhOjlcdOFjE87jgfVkgOH8L4hjpyHu8XetUtTtvckuEEgBgtbJLqqX6kSx3c8/Tk+DGh8WM1DaPvr+ZYUsdclnvW8ZE1I" };
            var sq2 = new AnswerDto
                      {
                          UserQuestionAnswerId = 96,
                          QuestionId = 21,
                          Content = "TVh8YtzPPVpnH7WqrW51FLZlzL/HcS38Nsr3MlHMU1yITkWB6KnSgFBea6zvUNTp7a83oe46jo08NnsC/q8n34AQ+GeocGbP1ZIf9VthGKmAV+sZTJHhov/yu25ybX0S"
                      };

            //qAndAs.Add(sq1);
            qAndAs.Add(item:sq2);

            var result = _questionAnswerService.CreateOrUpdateUserQuestionAnswers(userProfileId:1, questionAnswers:qAndAs);

            Assert.AreEqual(expected:result, actual:CreateOrUpdateAnswersResult.DuplicateAnswersInNew);
        }
    }
}