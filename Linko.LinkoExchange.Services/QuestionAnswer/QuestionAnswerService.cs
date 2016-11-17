using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Microsoft.AspNet.Identity;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Email;

namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    public class QuestionAnswerService : IQuestionAnswerService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IAuditLogEntry _logger;
        private readonly IHttpContextService _httpContext;
        private readonly IEncryptionService _encryption;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ISettingService _settingService;
        private readonly IDictionary<SystemSettingType, string> _globalSettings;
        private readonly IOrganizationService _orgService;
        private readonly IEmailService _emailService;

        public QuestionAnswerService(LinkoExchangeContext dbContext, IAuditLogEntry logger, IHttpContextService httpContext,
            IEncryptionService encryption, IPasswordHasher passwordHasher, ISettingService settingService,
            IOrganizationService orgService, IEmailService emailService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _httpContext = httpContext;
            _encryption = encryption;
            _passwordHasher = passwordHasher;
            _settingService = settingService;
            _globalSettings = _settingService.GetGlobalSettings();
            _orgService = orgService;
            _emailService = emailService;
        }

        public void AddUserQuestionAnswer(int userProfileId, AnswerDto answer)
        {
            try
            {
                var question = _dbContext.Questions.Include(q => q.QuestionType)
                    .Single(q => q.QuestionId == answer.QuestionId);

                if (question.QuestionType.Name == QuestionTypeName.KBQ.ToString())
                {
                    //Hash answer
                    answer.Content = _passwordHasher.HashPassword(answer.Content.Trim().ToLower());
                }
                else if (question.QuestionType.Name == QuestionTypeName.SQ.ToString())
                {
                    //Encrypt answer
                    answer.Content = _encryption.EncryptString(answer.Content.Trim());
                }


                UserQuestionAnswer newAnswer = _dbContext.UserQuestionAnswers.Create();
                newAnswer.Content = answer.Content;
                newAnswer.UserProfileId = userProfileId;
                newAnswer.CreationDateTimeUtc = DateTime.UtcNow;
                newAnswer.LastModificationDateTimeUtc = null;
                newAnswer.QuestionId = answer.QuestionId;
                newAnswer.UserProfileId = userProfileId;
                newAnswer.CreationDateTimeUtc = DateTime.UtcNow;
                newAnswer.LastModificationDateTimeUtc = DateTime.UtcNow;


                _dbContext.UserQuestionAnswers.Add(newAnswer);
                _dbContext.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                List<RuleViolation> validationIssues = new List<RuleViolation>();

                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;

                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        validationIssues.Add(new RuleViolation(string.Empty, null, message));

                    }
                }

                throw new RuleViolationException("Validation errors", validationIssues);
            }

        }

        public void CreateOrUpdateUserQuestionAnswer(int userProfileId, AnswerDto answerDto)
        {
            var question = _dbContext.Questions.Include(q => q.QuestionType)
                .Single(q => q.QuestionId == answerDto.QuestionId);
            if (question.QuestionType.Name == QuestionTypeName.KBQ.ToString())
            {
                //Hash answer
                answerDto.Content = _passwordHasher.HashPassword(answerDto.Content.Trim().ToLower()); //CASE INSENSITIVE -- LOWER CASE ONLY
            }
            else if (question.QuestionType.Name == QuestionTypeName.SQ.ToString())
            {
                //Encrypt answer
                answerDto.Content = _encryption.EncryptString(answerDto.Content.Trim());
            }

            UserQuestionAnswer answer;
            if (answerDto.UserQuestionAnswerId.HasValue)
            {
                answer = _dbContext.UserQuestionAnswers
                    .Include(a => a.Question)
                    .Single(a => a.UserQuestionAnswerId == answerDto.UserQuestionAnswerId.Value);

                answer.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                answer.Content = answerDto.Content;
                answer.QuestionId = answerDto.QuestionId;
            }
            else
            {
                answer = _dbContext.UserQuestionAnswers.Create();
                answer.Content = answerDto.Content;
                answer.CreationDateTimeUtc = DateTimeOffset.UtcNow;
                answer.UserProfileId = userProfileId;
                answer.QuestionId = answerDto.QuestionId;
                _dbContext.UserQuestionAnswers.Add(answer);
            }

            try
            {
                _dbContext.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                List<RuleViolation> validationIssues = new List<RuleViolation>();

                foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                {
                    DbEntityEntry entry = item.Entry;
                    string entityTypeName = entry.Entity.GetType().Name;

                    foreach (DbValidationError subItem in item.ValidationErrors)
                    {
                        string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                        validationIssues.Add(new RuleViolation(string.Empty, null, message));

                    }
                }

                throw new RuleViolationException("Validation errors", validationIssues);
            }

        }

        /// <summary>
        /// Should be used when creating or updating the entire collection of user's questions.
        /// Performs checks for duplicated questions and/or same answers used for more than 1 question.
        /// Also sends email as per the use case(s).
        /// </summary>
        /// <param name="userProfileId">User</param>
        /// <param name="questionAnswers">Collection of Dtos</param>
        public CreateOrUpdateAnswersResult CreateOrUpdateUserQuestionAnswers(int userProfileId, ICollection<AnswerDto> questionAnswers)
        {
            //First step: go through all answers and find dirty fields that are not hashed yet.
            //Check THESE only against the clean ones
            var updatedDirtyAnswerList = new List<string>();
            var cleanHashedAnswerList = new List<string>();
            var cleanEncryptedAnswerList = new List<string>();
            var answerDtosToUpdate = new List<AnswerDto>();
            foreach (var answerDto in questionAnswers)
            {
                if (!answerDto.UserQuestionAnswerId.HasValue 
                    || _dbContext.UserQuestionAnswers
                        .Single(q => q.UserQuestionAnswerId == answerDto.UserQuestionAnswerId).Content != answerDto.Content)
                {
                    //dirty
                    if (updatedDirtyAnswerList.Contains(answerDto.Content))
                        return CreateOrUpdateAnswersResult.DuplicateAnswersInNew; //duplicate new un-hashed answers
                    else
                    {
                        updatedDirtyAnswerList.Add(answerDto.Content);
                        answerDtosToUpdate.Add(answerDto);
                    }
                }
                else
                {
                    //clean

                    //kbq or sq?
                    var question = _dbContext.Questions.Include(q => q.QuestionType)
                        .Single(q => q.QuestionId == answerDto.QuestionId);
                    if (question.QuestionType.Name == QuestionTypeName.KBQ.ToString())
                    {
                        //Hashed answer
                        cleanHashedAnswerList.Add(answerDto.Content);
                    }
                    else if (question.QuestionType.Name == QuestionTypeName.SQ.ToString())
                    {
                        //Encrypted answer
                        cleanEncryptedAnswerList.Add(answerDto.Content);
                    }

                }
            }

            //Check new answers don't already exist within old answers
            foreach (var newAnswer in updatedDirtyAnswerList)
            {
                foreach (var hashedAnswer in cleanHashedAnswerList)
                {
                    if (_passwordHasher.VerifyHashedPassword(hashedAnswer, newAnswer.Trim().ToLower()) == PasswordVerificationResult.Success)
                        return CreateOrUpdateAnswersResult.DuplicateAnswersInNewAndExisting; //duplicate found
                }

                foreach (var encryptedAnswer in cleanEncryptedAnswerList)
                {
                    if (_encryption.EncryptString(newAnswer.Trim()) == encryptedAnswer)
                        return CreateOrUpdateAnswersResult.DuplicateAnswersInNewAndExisting; //duplicate found
                }
            }


            //Check for duplicate questions
            var duplicateQuestions = questionAnswers.GroupBy(x => new { x.QuestionId })
                    .Where(g => g.Count() > 1)
                    .Select(y => y.Key)
                    .ToList();

            if (duplicateQuestions.Count() > 0)
                return CreateOrUpdateAnswersResult.DuplicateQuestionsInNewAndExisting;

            //Persist to DB
            int questionCountKBQ = 0;
            int questionCountSQ = 0;
            foreach (var questionAnswer in answerDtosToUpdate)
            {
                var questionTypeName = _dbContext.Questions.Include(q => q.QuestionType)
                    .Single(q => q.QuestionId == questionAnswer.QuestionId).QuestionType.Name;
                questionCountKBQ += questionTypeName == QuestionTypeName.KBQ.ToString() ? 1 : 0;
                questionCountSQ += questionTypeName == QuestionTypeName.SQ.ToString() ? 1 : 0;

                CreateOrUpdateUserQuestionAnswer(userProfileId, questionAnswer);
            }

            var userProfile = _dbContext.Users.Single(u => u.UserProfileId == userProfileId);

            //Send Emails
            var contentReplacements = new Dictionary<string, string>();
            string supportPhoneNumber = _globalSettings[SystemSettingType.SupportPhoneNumber];
            string supportEmail = _globalSettings[SystemSettingType.SupportEmailAddress];

            var authorityList = _orgService.GetUserAuthorityListForEmailContent(userProfileId);
            contentReplacements.Add("userName", userProfile.UserName);
            contentReplacements.Add("authorityList", authorityList);
            contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
            contentReplacements.Add("supportEmail", supportEmail);

            if (questionCountKBQ > 0)
                _emailService.SendEmail(new[] { userProfile.Email }, EmailType.Profile_KBQChanged, contentReplacements);
            if (questionCountSQ > 0)
                _emailService.SendEmail(new[] { userProfile.Email }, EmailType.Profile_SecurityQuestionsChanged, contentReplacements);

            return CreateOrUpdateAnswersResult.Success;

        }

        public void UpdateQuestion(QuestionDto question)
        {
            if (question != null && question.QuestionId.HasValue && question.QuestionId > 0)
            {
                var questionToUpdate = _dbContext.Questions.Single(q => q.QuestionId == question.QuestionId);
                questionToUpdate.Content = question.Content;
                questionToUpdate.QuestionTypeId = _dbContext.QuestionTypes.Single(q => q.Name == question.QuestionType.ToString()).QuestionTypeId;
                questionToUpdate.IsActive = question.IsActive;
                questionToUpdate.LastModificationDateTimeUtc = DateTime.UtcNow;
                questionToUpdate.LastModifierUserId = Convert.ToInt32(_httpContext.CurrentUserProfileId());

                try
                {
                    _dbContext.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    List<RuleViolation> validationIssues = new List<RuleViolation>();

                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        DbEntityEntry entry = item.Entry;
                        string entityTypeName = entry.Entity.GetType().Name;

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                            validationIssues.Add(new RuleViolation(string.Empty, null, message));

                        }
                    }

                    throw new RuleViolationException("Validation errors", validationIssues);
                }
            }
            else
            {
                List<RuleViolation> validationIssues = new List<RuleViolation>();
                validationIssues.Add(new RuleViolation(string.Empty, null, "Question update attempt failed."));
                //_logger.Error("SubmitPOMDetails. Null question or missing QuestionId.");
                throw new RuleViolationException("Validation errors", validationIssues);
            }

        }

        public void UpdateAnswer(AnswerDto answer)
        {
            if (answer != null && answer.UserQuestionAnswerId.HasValue && answer.UserQuestionAnswerId > 0)
            {
                var answerToUpdate = _dbContext.UserQuestionAnswers
                    .Include("Question")
                    .Single(a => a.UserQuestionAnswerId == answer.UserQuestionAnswerId);

                if (answerToUpdate.Question.QuestionType.Name == QuestionTypeName.SQ.ToString())
                {
                    //Hash answer
                    answer.Content = _passwordHasher.HashPassword(answer.Content.Trim().ToLower());
                }
                else if (answerToUpdate.Question.QuestionType.Name == QuestionTypeName.SQ.ToString())
                {
                    //Encrypt answer
                    answer.Content = _encryption.EncryptString(answer.Content.Trim());
                }

                answerToUpdate.Content = answer.Content;
                answerToUpdate.LastModificationDateTimeUtc = DateTime.UtcNow;

                try
                {
                    _dbContext.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    List<RuleViolation> validationIssues = new List<RuleViolation>();

                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        DbEntityEntry entry = item.Entry;
                        string entityTypeName = entry.Entity.GetType().Name;

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                            validationIssues.Add(new RuleViolation(string.Empty, null, message));

                        }
                    }

                    throw new RuleViolationException("Validation errors", validationIssues);
                }
            }
            else
            {
                List<RuleViolation> validationIssues = new List<RuleViolation>();
                validationIssues.Add(new RuleViolation(string.Empty, null, "Question update attempt failed."));
                //_logger.Error("SubmitPOMDetails. Null question or missing QuestionId.");
                throw new RuleViolationException("Validation errors", validationIssues);
            }

        }

        public void DeleteUserQuestionAnswer(int userQuestionAnswerId)
        {
            var answerToDelete = _dbContext.UserQuestionAnswers
                .Include(a => a.Question)
                .Single(a => a.UserQuestionAnswerId == userQuestionAnswerId);
            if (answerToDelete != null)
            {
                _dbContext.UserQuestionAnswers.Remove(answerToDelete);

                try
                {
                    _dbContext.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    List<RuleViolation> validationIssues = new List<RuleViolation>();

                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        DbEntityEntry entry = item.Entry;
                        string entityTypeName = entry.Entity.GetType().Name;

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                            validationIssues.Add(new RuleViolation(string.Empty, null, message));

                        }
                    }

                    //_logger.Info("???");
                    throw new RuleViolationException("Validation errors", validationIssues);
                }

            }
            else
            {
                string errorMsg = string.Format("DeleteQuestionAnswerPair. Could not find UserQuestionAnswer associated with Id={0}", userQuestionAnswerId);
                List<RuleViolation> validationIssues = new List<RuleViolation>();
                validationIssues.Add(new RuleViolation(string.Empty, null, errorMsg));
                //_logger.Info("SubmitPOMDetails. Null question or missing QuestionId.");
                throw new RuleViolationException("Validation errors", validationIssues);
            }
        }

        public ICollection<QuestionAnswerPairDto> GetUsersQuestionAnswers(int userProfileId, QuestionTypeName questionType)
        {
            var usersQAList = new List<Dto.QuestionAnswerPairDto>();
            var foundQAs = _dbContext.UserQuestionAnswers.Include(a => a.Question.QuestionType)
                .Where(a => a.UserProfileId == userProfileId
                && a.Question.QuestionType.Name == questionType.ToString());

            if (foundQAs != null)
            {
                foreach (var foundQA in foundQAs)
                {
                    var newQADto = new QuestionAnswerPairDto() { Answer = new Dto.AnswerDto(), Question = new Dto.QuestionDto() };
                    newQADto.Answer.UserQuestionAnswerId = foundQA.UserQuestionAnswerId;

                    if (questionType == QuestionTypeName.SQ)
                    {
                        //Decrypt answer
                        newQADto.Answer.Content = _encryption.DecryptString(foundQA.Content);
                    }
                    else
                    {
                        newQADto.Answer.Content = foundQA.Content;
                    }

                    newQADto.Question.QuestionId = foundQA.Question.QuestionId;
                    newQADto.Question.IsActive = foundQA.Question.IsActive;
                    newQADto.Question.QuestionType = (QuestionTypeName)Enum.Parse(typeof(QuestionTypeName), foundQA.Question.QuestionType.Name, true);
                    newQADto.Question.Content = foundQA.Question.Content;

                    usersQAList.Add(newQADto);

                }
            }
            return usersQAList;
        }

        public QuestionAnswerPairDto GetRandomQuestionAnswerFromToken(string token, QuestionTypeName questionType)
        {
            //Find UserProfileId from EmailAuditLog.Recipient
            var emailAuditLog = _dbContext.EmailAuditLogs.FirstOrDefault(e => e.Token == token);
            if (emailAuditLog != null && emailAuditLog.RecipientUserProfileId.HasValue)
            {
                var userProfileId = emailAuditLog.RecipientUserProfileId;
                return GetRandomQuestionAnswerFromUserProfileId(userProfileId.Value, questionType);

            }
            else
                throw new Exception(string.Format("ERROR: Cannot find email audit log for token={0}", token));

        }

        public QuestionAnswerPairDto GetRandomQuestionAnswerFromUserProfileId(int userProfileId, QuestionTypeName questionType)
        {
            var qAndAs = GetUsersQuestionAnswers(userProfileId, questionType);
            return qAndAs.OrderBy(qu => Guid.NewGuid()).First();
        }

        public void CreateUserQuestionAnswers(int userProfileId, IEnumerable<AnswerDto> questionAnswers)
        { 
            if(questionAnswers != null && questionAnswers.Any())
            {
                 foreach(var qa in questionAnswers)
                {
                    CreateOrUpdateUserQuestionAnswer(userProfileId, qa);
                }
            }
        }

        public void DeleteUserQuestionAndAnswers(int userProfileId)
        {
            var userQuestionAndAnswers = _dbContext.UserQuestionAnswers.Where(i => i.UserProfileId == userProfileId); 

            if (userQuestionAndAnswers != null && userQuestionAndAnswers.Any())
            {
                foreach (var answer in userQuestionAndAnswers)
                {
                    _dbContext.UserQuestionAnswers.Remove(answer);
                }
            }
        }

        public ICollection<QuestionDto> GetQuestions()
        {
            var list = new List<QuestionDto>();

            var questions = _dbContext.Questions
                .Where(q => q.IsActive == true);


            foreach (var question in questions)
            {
                var dto = new Dto.QuestionDto()
                {
                    QuestionId = question.QuestionId,
                    Content = question.Content
                };
                dto.QuestionType = (QuestionTypeName)Enum.Parse(typeof(QuestionTypeName), question.QuestionType.Name, true);

                list.Add(dto);
            }

            return list;

        }

        public bool ConfirmCorrectAnswer(int userQuestionAnswerId, string answer)
        {
            var questionAnswer = _dbContext.UserQuestionAnswers.Include(q => q.Question.QuestionType)
                .Single(u => u.UserQuestionAnswerId == userQuestionAnswerId);

            if (questionAnswer.Question.QuestionType.Name == QuestionTypeName.KBQ.ToString())
            {
                return _passwordHasher.VerifyHashedPassword(questionAnswer.Content, answer.Trim().ToLower()) == PasswordVerificationResult.Success;
            }
            else
            {
                return _encryption.EncryptString(answer.Trim()) == questionAnswer.Content;
            }
        }


        public RegistrationResult ValidateUserKbqData(IEnumerable<AnswerDto> kbqQuestions)
        {
            if (kbqQuestions == null || kbqQuestions.Count() < 5)
            {
                return RegistrationResult.MissingKBQ;
            }

            // Test duplicated KBQ questions
            if (kbqQuestions.GroupBy(i => i.QuestionId).Any(i => i.Count() > 1))
            {
                return RegistrationResult.DuplicatedKBQ;
            }

            // Test duplicated KBQ question answers
            if (kbqQuestions.GroupBy(i => i.Content).Any(i => i.Count() > 1))
            {
                return RegistrationResult.DuplicatedKBQAnswer;
            }

            // Test KBQ questions mush have answer
            if (kbqQuestions.Any(i => i.Content == null))
            {
                return RegistrationResult.MissingKBQAnswer;
            }

            return RegistrationResult.Success;
        }

        public RegistrationResult ValidateUserSqData(IEnumerable<AnswerDto> securityQuestions)
        {
            if (securityQuestions == null || securityQuestions.Count() < 2)
            {
                return RegistrationResult.MissingSecurityQuestion;
            }

            // Test duplicated security questions
            if (securityQuestions.GroupBy(i => i.QuestionId).Any(i => i.Count() > 1))
            {
                return RegistrationResult.DuplicatedSecurityQuestion;
            }

            // Test duplicated security question answers
            if (securityQuestions.GroupBy(i => i.Content).Any(i => i.Count() > 1))
            {
                return RegistrationResult.DuplicatedSecurityQuestionAnswer;
            }

            // Test security questions mush have answer
            if (securityQuestions.Any(i => i.Content == null))
            {
                return RegistrationResult.MissingSecurityQuestionAnswer;
            }

            return RegistrationResult.Success;
        }
    }
}
