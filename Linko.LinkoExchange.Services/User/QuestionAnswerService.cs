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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Linko.LinkoExchange.Services.User
{
    public class QuestionAnswerService : IQuestionAnswerService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IAuditLogEntry _logger;

        public QuestionAnswerService(LinkoExchangeContext dbContext, IAuditLogEntry logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public void AddQuestionAnswerPair(int userProfileId, QuestionDto question, AnswerDto answer)
        {
            try
            {
                Question newQuestion = _dbContext.Questions.Create();
                newQuestion.Content = question.Content;
                newQuestion.QuestionType = _dbContext.QuestionTypes.Single(q => q.QuestionTypeId == question.QuestionTypeId);
                newQuestion.IsActive = true;
                newQuestion.CreationDateTime = DateTime.UtcNow;
                newQuestion.LastModificationDateTime = DateTime.UtcNow;
                newQuestion.LastModifierUserId = HttpContext.Current.User.Identity.GetOrganizationRegulatoryProgramUserId();

                UserQuestionAnswer newAnswer = _dbContext.UserQuestionAnswers.Create();
                newAnswer.Content = answer.Content;
                newAnswer.Question = newQuestion;
                newAnswer.UserProfile = _dbContext.UserProfiles.Single(u => u.UserProfileId == userProfileId);
                newAnswer.CreationDateTime = DateTime.UtcNow;
                newAnswer.LastModificationDateTime = DateTime.UtcNow;

                //_dbContext.Questions.Add(newQuestion); //Needed?
                _dbContext.UserQuestionAnswers.Add(newAnswer);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                //_logger.Log("ERROR")
                throw ex; //new Exception();
            }
            
        }

        public void UpdateQuestion(QuestionDto question)
        {
            if (question != null && question.QuestionId.HasValue && question.QuestionId > 0)
            {
                var questionToUpdate = _dbContext.Questions.Single(q => q.QuestionId == question.QuestionId);
                questionToUpdate.Content = question.Content;
                questionToUpdate.QuestionType = _dbContext.QuestionTypes.Single(q => q.QuestionTypeId == question.QuestionTypeId);
                questionToUpdate.IsActive = question.IsActive;
                questionToUpdate.LastModificationDateTime = DateTime.UtcNow;
                questionToUpdate.LastModifierUserId = HttpContext.Current.User.Identity.GetOrganizationRegulatoryProgramUserId();

                try
                {
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    //_logger.Log("ERROR")
                    throw ex; //new Exception();
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
            //TODO
        }

        public void DeleteQuestionAnswerPair(int userQuestionAnswerId)
        {
            var answerToDelete = _dbContext.UserQuestionAnswers.Single(a => a.UserQuestionAnswerId == userQuestionAnswerId);
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
    }
}
