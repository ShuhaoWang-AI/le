using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Linq;
using Linko.LinkoExchange.Services.QuestionAnswer;
using Microsoft.AspNet.Identity;

namespace Linko.LinkoExchange.Services
{
    public class QuestionAnswerService : IQuestionAnswerService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IAuditLogEntry _logger;
        private readonly IHttpContextService _httpContext;
        private readonly IEncryptionService _encryption;
        private readonly IPasswordHasher _passwordHasher;

        public QuestionAnswerService(LinkoExchangeContext dbContext, IAuditLogEntry logger, IHttpContextService httpContext,
            IEncryptionService encryption, IPasswordHasher passwordHasher)
        {
            _dbContext = dbContext;
            _logger = logger;
            _httpContext = httpContext;
            _encryption = encryption;
            _passwordHasher = passwordHasher;
        }

        public void AddQuestionAnswerPair(int userProfileId, QuestionDto question, AnswerDto answer)
        {
            try
            {
                Question newQuestion = _dbContext.Questions.Create();
                newQuestion.Content = question.Content;
                newQuestion.QuestionTypeId = (int)question.QuestionType;
                newQuestion.IsActive = question.IsActive;
                newQuestion.CreationDateTimeUtc = DateTime.UtcNow;
                newQuestion.LastModificationDateTimeUtc = null;
                newQuestion.LastModifierUserId = null;

                if (question.QuestionType == Dto.QuestionType.KnowledgeBased)
                {
                    //Hash answer
                    answer.Content = _passwordHasher.HashPassword(answer.Content);
                }
                else if (question.QuestionType == Dto.QuestionType.Security)
                {
                    //Encrypt answer
                    answer.Content = _encryption.EncryptString(answer.Content);
                }


                UserQuestionAnswer newAnswer = _dbContext.UserQuestionAnswers.Create();
                newAnswer.Content = answer.Content;
                newAnswer.UserProfileId = userProfileId;
                newAnswer.CreationDateTimeUtc = DateTime.UtcNow;
                newAnswer.LastModificationDateTimeUtc = null;
                newAnswer.Question = newQuestion;
                newAnswer.UserProfileId = userProfileId;
                newAnswer.CreationDateTimeUtc = DateTime.UtcNow;
                newAnswer.LastModificationDateTimeUtc = DateTime.UtcNow;


                _dbContext.Questions.Add(newQuestion);
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

        public void CreateOrUpdateQuestionAnswerPair(int userProfileId, QuestionAnswerPairDto questionAnswer)
        {
            if (questionAnswer.Question.QuestionType == Dto.QuestionType.KnowledgeBased)
            {
                //Hash answer
                questionAnswer.Answer.Content = _passwordHasher.HashPassword(questionAnswer.Answer.Content);
            }
            else if (questionAnswer.Question.QuestionType == Dto.QuestionType.Security)
            {
                //Encrypt answer
                questionAnswer.Answer.Content = _encryption.EncryptString(questionAnswer.Answer.Content);
            }

            UserQuestionAnswer answer;
            Question question;
            if (questionAnswer.Answer.UserQuestionAnswerId.HasValue)
            {
                answer = _dbContext.UserQuestionAnswers
                    .Include(a => a.Question)
                    .Single(a => a.UserQuestionAnswerId == questionAnswer.Answer.UserQuestionAnswerId.Value);

                answer.Content = questionAnswer.Answer.Content;
                answer.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;  // DateTime.UtcNow;
                question = answer.Question;
                question.Content = questionAnswer.Question.Content;
                question.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
                question.LastModifierUserId = Convert.ToInt32(_httpContext.Current().User.Identity.UserProfileId());
            }
            else
            {
                answer = _dbContext.UserQuestionAnswers.Create();
                answer.Content = questionAnswer.Answer.Content;
                answer.CreationDateTimeUtc = DateTimeOffset.UtcNow;
                answer.UserProfileId = userProfileId;

                question = _dbContext.Questions.Create();
                question.Content = questionAnswer.Question.Content;
                question.IsActive = questionAnswer.Question.IsActive;
                question.QuestionTypeId = (int)questionAnswer.Question.QuestionType;
                question.CreationDateTimeUtc = DateTimeOffset.UtcNow;
                question.LastModificationDateTimeUtc = null;
                question.LastModifierUserId = null;
                answer.Question = question;

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

        public void UpdateQuestion(QuestionDto question)
        {
            if (question != null && question.QuestionId.HasValue && question.QuestionId > 0)
            {
                var questionToUpdate = _dbContext.Questions.Single(q => q.QuestionId == question.QuestionId);
                questionToUpdate.Content = question.Content;
                questionToUpdate.QuestionTypeId = (int)question.QuestionType;
                questionToUpdate.IsActive = question.IsActive;
                questionToUpdate.LastModificationDateTimeUtc = DateTime.UtcNow;
                questionToUpdate.LastModifierUserId = Convert.ToInt32(_httpContext.Current().User.Identity.UserProfileId());

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
                var answerToUpdate = _dbContext.UserQuestionAnswers.Single(a => a.UserQuestionAnswerId == answer.UserQuestionAnswerId);

                if (answerToUpdate.Question.QuestionTypeId == (int)Dto.QuestionType.KnowledgeBased)
                {
                    //Hash answer
                    answer.Content = _passwordHasher.HashPassword(answer.Content);
                }
                else if (answerToUpdate.Question.QuestionTypeId == (int)Dto.QuestionType.Security)
                {
                    //Encrypt answer
                    answer.Content = _encryption.EncryptString(answer.Content);
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

        public void DeleteQuestionAnswerPair(int userQuestionAnswerId)
        {
            var answerToDelete = _dbContext.UserQuestionAnswers
                .Include(a => a.Question)
                .Single(a => a.UserQuestionAnswerId == userQuestionAnswerId);
            if (answerToDelete != null)
            {
                if (answerToDelete.Question != null)
                {
                    _dbContext.Questions.Remove(answerToDelete.Question);
                }
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

        public ICollection<QuestionAnswerPairDto> GetUsersQuestionAnswers(int userProfileId, Dto.QuestionType questionType)
        {
            var usersQAList = new List<Dto.QuestionAnswerPairDto>();
            var foundQAs = _dbContext.UserQuestionAnswers.Include(a => a.Question)
                .Where(a => a.UserProfileId == userProfileId
                && a.Question.QuestionTypeId == (int)questionType);

            if (foundQAs != null)
            {
                foreach (var foundQA in foundQAs)
                {
                    var newQADto = new QuestionAnswerPairDto() { Answer = new Dto.AnswerDto(), Question = new Dto.QuestionDto() };
                    newQADto.Answer.UserQuestionAnswerId = foundQA.UserQuestionAnswerId;

                    if (questionType == Dto.QuestionType.Security)
                    {
                        //Encrypt answer
                        foundQA.Content = _encryption.DecryptString(foundQA.Content);
                    }

                    newQADto.Answer.Content = foundQA.Content;
                    newQADto.Question.QuestionId = foundQA.Question.QuestionId;
                    newQADto.Question.IsActive = foundQA.Question.IsActive;
                    newQADto.Question.QuestionType = (Dto.QuestionType)foundQA.Question.QuestionTypeId;
                    newQADto.Question.Content = foundQA.Question.Content;

                    usersQAList.Add(newQADto);

                }
            }
            return usersQAList;
        }

        public QuestionAnswerPairDto GetRandomQuestionAnswerFromToken(string token, Dto.QuestionType questionType)
        {
            //Find UserProfileId from EmailAuditLog.Recipient
            var emailAuditLog = _dbContext.EmailAuditLog.SingleOrDefault(e => e.Token == token);
            if (emailAuditLog != null && emailAuditLog.RecipientUserProfileId.HasValue)
            {
                var userProfileId = emailAuditLog.RecipientUserProfileId;
                return GetRandomQuestionAnswerFromUserProfileId(userProfileId.Value, questionType);

            }
            else
                throw new Exception(string.Format("ERROR: Cannot find email audit log for token={0}", token));

        }

        public QuestionAnswerPairDto GetRandomQuestionAnswerFromUserProfileId(int userProfileId, Dto.QuestionType questionType)
        {
            var qAndAs = GetUsersQuestionAnswers(userProfileId, questionType);
            return qAndAs.OrderBy(qu => Guid.NewGuid()).First();
        }

        public void CreateQuestionAnswerPairs(int userProfileId, IEnumerable<QuestionAnswerPairDto> questionAnswers)
        { 
            if(questionAnswers != null && questionAnswers.Any())
            {
                 foreach(var qa in questionAnswers)
                {
                    CreateOrUpdateQuestionAnswerPair(userProfileId, qa);
                }
            }
        }
    }
}
