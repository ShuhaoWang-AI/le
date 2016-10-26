using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Services.Cache;

namespace Linko.LinkoExchange.Services
{
    public class QuestionAnswerService : IQuestionAnswerService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IAuditLogEntry _logger;
        private readonly ICurrentUser _currentUser;

        public QuestionAnswerService(LinkoExchangeContext dbContext, IAuditLogEntry logger, ICurrentUser currentUser)
        {
            _dbContext = dbContext;
            _logger = logger;
            _currentUser = currentUser;
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
            UserQuestionAnswer answer;
            Question question;
            if (questionAnswer.Answer.UserQuestionAnswerId.HasValue)
            {
                answer = _dbContext.UserQuestionAnswers
                    .Include(a => a.Question)
                    .Single(a => a.UserQuestionAnswerId == questionAnswer.Answer.UserQuestionAnswerId.Value);

                answer.Content = questionAnswer.Answer.Content;
                answer.LastModificationDateTimeUtc = DateTime.UtcNow;
                question = answer.Question;
                question.Content = questionAnswer.Question.Content;
                question.LastModificationDateTimeUtc = DateTime.UtcNow;
                question.LastModifierUserId = Convert.ToInt32(_currentUser.UserProfileId());
            }
            else
            {
                answer = _dbContext.UserQuestionAnswers.Create();
                answer.Content = questionAnswer.Answer.Content;
                answer.CreationDateTimeUtc = DateTime.UtcNow;
                answer.UserProfileId = userProfileId;

                question = _dbContext.Questions.Create();
                question.Content = questionAnswer.Question.Content;
                question.IsActive = questionAnswer.Question.IsActive;
                question.QuestionTypeId = (int)questionAnswer.Question.QuestionType;
                question.CreationDateTimeUtc = DateTime.UtcNow;
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
                questionToUpdate.LastModifierUserId = Convert.ToInt32(_currentUser.UserProfileId());

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
    }
}
