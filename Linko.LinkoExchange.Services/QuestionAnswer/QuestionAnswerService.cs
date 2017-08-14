﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Microsoft.AspNet.Identity;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Email;
using NLog;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.HttpContext;

namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    public class QuestionAnswerService : IQuestionAnswerService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly ILogger _logger;
        private readonly IHttpContextService _httpContext;
        private readonly IEncryptionService _encryption;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IDictionary<SystemSettingType, string> _globalSettings;
        private readonly IOrganizationService _orgService;  
        private readonly ICromerrAuditLogService _crommerAuditLogService; 
        private readonly ILinkoExchangeEmailService  _linkoExchangeEmailService;
        public QuestionAnswerService(
            LinkoExchangeContext dbContext, 
            ILogger logger, 
            IHttpContextService httpContext,
            IEncryptionService encryption, 
            IPasswordHasher passwordHasher, 
            ISettingService settingService,
            IOrganizationService orgService,
            ICromerrAuditLogService crommerAuditLogService,
            ILinkoExchangeEmailService linkoExchangeEmailService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _httpContext = httpContext;
            _encryption = encryption;
            _passwordHasher = passwordHasher;
            _globalSettings = settingService.GetGlobalSettings();
            _orgService = orgService;  
            _crommerAuditLogService = crommerAuditLogService; 
            _linkoExchangeEmailService = linkoExchangeEmailService;
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

                answer.LastModificationDateTimeUtc = DateTimeOffset.Now;
                answer.Content = answerDto.Content;
                answer.QuestionId = answerDto.QuestionId;
            }
            else
            {
                answer = _dbContext.UserQuestionAnswers.Create();
                answer.Content = answerDto.Content;
                answer.CreationDateTimeUtc = DateTimeOffset.Now;
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


            int questionCountKbq = 0;
            int questionCountSq = 0;
            //Persist to DB
            //
            using (var transaction = _dbContext.BeginTransaction())
            {
                try
                {
                    //Prevent failure scenario where the user is swapping 2 existing questions (causing a duplicate error) = BUG 1856
                    //  1. For items in the collection where Id is specified, attempt to find another existing row with matching QuestionId (key violation). 
                    //  2. If found:
                    //          - Delete from the table "tUserQuestionAnswer"
                    //          - Remove the Id from item in the collection to force "Create New" in method: "CreateOrUpdateUserQuestionAnswer"
                    foreach (var questionAnswer in answerDtosToUpdate)
                    {
                        var existingRow = _dbContext.UserQuestionAnswers
                                .SingleOrDefault(uqa => uqa.UserProfileId == userProfileId
                                    && uqa.QuestionId == questionAnswer.QuestionId);

                        if (existingRow != null)
                        {
                            if (questionAnswer.UserQuestionAnswerId.HasValue && existingRow.UserQuestionAnswerId == questionAnswer.UserQuestionAnswerId.Value)
                            {
                                //same row -- no problem
                            }
                            else
                            {
                                //Key violation (User Profile Id / Question Id)

                                //Delete key violating row from the table "tUserQuestionAnswer"
                                _dbContext.UserQuestionAnswers.Remove(existingRow);
                                _dbContext.SaveChanges();

                                //Remove the Id to force "Create New" in method: "CreateOrUpdateUserQuestionAnswer"
                                questionAnswer.UserQuestionAnswerId = null;
                            }
                        }
                    }

                    //Now we can save to the database without risk of key violation error
                    foreach (var questionAnswer in answerDtosToUpdate)
                    {
                        var questionTypeName = _dbContext.Questions.Include(q => q.QuestionType)
                            .Single(q => q.QuestionId == questionAnswer.QuestionId).QuestionType.Name;
                        questionCountKbq += questionTypeName == QuestionTypeName.KBQ.ToString() ? 1 : 0;
                        questionCountSq += questionTypeName == QuestionTypeName.SQ.ToString() ? 1 : 0;

                        CreateOrUpdateUserQuestionAnswer(userProfileId, questionAnswer);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    var errors = new List<string>() { ex.Message };

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        errors.Add(ex.Message);
                    }

                    _logger.Error("Error happens {0} ", String.Join("," + Environment.NewLine, errors));

                    throw;
                }

            }

            var userProfile = _dbContext.Users.Single(u => u.UserProfileId == userProfileId); 
              
            // Cromerr audit log
            var allOrgRegProgramUsers = _dbContext.OrganizationRegulatoryProgramUsers
                .Include(orpu => orpu.OrganizationRegulatoryProgram)
                .Where(orpu => orpu.UserProfileId == userProfileId);

            foreach (var orgRegProgramUser in allOrgRegProgramUsers)
            {
                var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
                cromerrAuditLogEntryDto.RegulatoryProgramId = orgRegProgramUser.OrganizationRegulatoryProgram.RegulatoryProgramId;
                cromerrAuditLogEntryDto.OrganizationId = orgRegProgramUser.OrganizationRegulatoryProgram.OrganizationId;
                cromerrAuditLogEntryDto.RegulatorOrganizationId = orgRegProgramUser.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                cromerrAuditLogEntryDto.UserProfileId = userProfile.UserProfileId;
                cromerrAuditLogEntryDto.UserName = userProfile.UserName;
                cromerrAuditLogEntryDto.UserFirstName = userProfile.FirstName;
                cromerrAuditLogEntryDto.UserLastName = userProfile.LastName;
                cromerrAuditLogEntryDto.UserEmailAddress = userProfile.Email;
                cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
                var cromerrContentReplacements = new Dictionary<string, string>();
                cromerrContentReplacements.Add("firstName", userProfile.FirstName);
                cromerrContentReplacements.Add("lastName", userProfile.LastName);
                cromerrContentReplacements.Add("userName", userProfile.UserName);
                cromerrContentReplacements.Add("emailAddress", userProfile.Email);

                if (questionCountKbq > 0)
                    _crommerAuditLogService.Log(CromerrEvent.Profile_KBQChanged, cromerrAuditLogEntryDto, cromerrContentReplacements);
                if (questionCountSq > 0)
                    _crommerAuditLogService.Log(CromerrEvent.Profile_SQChanged, cromerrAuditLogEntryDto, cromerrContentReplacements);
            }

            //Send Emails
            var contentReplacements = new Dictionary<string, string>();
            string supportPhoneNumber = _globalSettings[SystemSettingType.SupportPhoneNumber];
            string supportEmail = _globalSettings[SystemSettingType.SupportEmailAddress];

            var authorityList = _orgService.GetUserAuthorityListForEmailContent(userProfileId);
            contentReplacements.Add("firstName", userProfile.FirstName);
            contentReplacements.Add("lastName", userProfile.LastName);

            contentReplacements.Add("authorityList", authorityList);
            contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
            contentReplacements.Add("supportEmail", supportEmail);

            // Profile_KBQChanged, and Profile_SecurityQuestionsChanged emails are logged for all programs 
            var emailEntries = new List<EmailEntry>(); 
            
            if (questionCountKbq > 0) {
                emailEntries = _linkoExchangeEmailService.GetAllProgramEmailEntiresForUser(userProfile, EmailType.Profile_KBQChanged, contentReplacements); 
            } 

            if(questionCountSq > 0)
            {
                emailEntries.AddRange(_linkoExchangeEmailService.GetAllProgramEmailEntiresForUser(userProfile, EmailType.Profile_SecurityQuestionsChanged, contentReplacements));
            }
             
            _linkoExchangeEmailService.SendEmails(emailEntries);

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
                questionToUpdate.LastModificationDateTimeUtc = DateTimeOffset.Now;
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

                if (answerToUpdate.Question.QuestionType.Name == QuestionTypeName.KBQ.ToString())
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
                answerToUpdate.LastModificationDateTimeUtc = DateTimeOffset.Now;

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
                            string message = $"Error '{subItem.ErrorMessage}' occurred in {entityTypeName} at {subItem.PropertyName}";
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
            var usersQaList = new List<QuestionAnswerPairDto>();
            var foundQAs = _dbContext.UserQuestionAnswers.Include(a => a.Question.QuestionType)
                .Where(a => a.UserProfileId == userProfileId
                && a.Question.QuestionType.Name == questionType.ToString()).ToList();

            if (foundQAs.Any())
            {
                foreach (var foundQa in foundQAs)
                {
                    var newQaDto = new QuestionAnswerPairDto() { Answer = new AnswerDto(), Question = new QuestionDto() }; 

                    newQaDto.Answer.UserQuestionAnswerId = foundQa.UserQuestionAnswerId;

                    newQaDto.Answer.Content = questionType == QuestionTypeName.SQ ? _encryption.DecryptString(foundQa.Content) : foundQa.Content;

                    newQaDto.Question.QuestionId = foundQa.Question.QuestionId;
                    newQaDto.Question.IsActive = foundQa.Question.IsActive;
                    newQaDto.Question.QuestionType = (QuestionTypeName) Enum.Parse(typeof(QuestionTypeName), foundQa.Question.QuestionType.Name, true);
                    newQaDto.Question.Content = foundQa.Question.Content;

                    usersQaList.Add(newQaDto);
                }
            }

            return usersQaList;
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
            {
                throw new Exception($"ERROR: Cannot find email audit log for token={token}");
            }
        }

        public QuestionAnswerPairDto GetRandomQuestionAnswerFromUserProfileId(int userProfileId, QuestionTypeName questionType)
        {
            var qAndAs = GetUsersQuestionAnswers(userProfileId, questionType);
            return qAndAs.OrderBy(qu => Guid.NewGuid()).First();
        }

        public void CreateUserQuestionAnswers(int userProfileId, IEnumerable<AnswerDto> questionAnswers)
        {
            var answerDtos = questionAnswers as IList<AnswerDto> ?? questionAnswers.ToList();
            if (answerDtos.Any())
            {
                foreach (var qa in answerDtos)
                {
                    CreateOrUpdateUserQuestionAnswer(userProfileId, qa);
                }
            }
        }

        public void DeleteUserQuestionAndAnswers(int userProfileId)
        {
            var userQuestionAndAnswers = _dbContext.UserQuestionAnswers.Where(i => i.UserProfileId == userProfileId).ToList();

            if (userQuestionAndAnswers.Any())
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
            var questions = _dbContext.Questions.Where(q => q.IsActive);
            foreach (var question in questions)
            {
                var dto = new QuestionDto
                          {
                              QuestionId = question.QuestionId,
                              Content = question.Content,
                              QuestionType = (QuestionTypeName) Enum.Parse(typeof(QuestionTypeName), question.QuestionType.Name, true)
                          };

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
            var answerDtos = kbqQuestions as AnswerDto[] ?? kbqQuestions.ToArray();
            if (answerDtos.Count() < 5)
            {
                return RegistrationResult.MissingKBQ;
            }

            // Test duplicated KBQ questions
            if (answerDtos.GroupBy(i => i.QuestionId).Any(i => i.Count() > 1))
            {
                return RegistrationResult.DuplicatedKBQ;
            }

            // Test duplicated KBQ question answers
            if (answerDtos.GroupBy(i => i.Content).Any(i => i.Count() > 1))
            {
                return RegistrationResult.DuplicatedKBQAnswer;
            }

            // Test KBQ questions mush have answer
            if (answerDtos.Any(i => i.Content == null))
            {
                return RegistrationResult.MissingKBQAnswer;
            }

            return RegistrationResult.Success;
        }

        public RegistrationResult ValidateUserSqData(IEnumerable<AnswerDto> securityQuestions)
        {
            var answerDtos = securityQuestions as AnswerDto[] ?? securityQuestions.ToArray();
            if (answerDtos.Count() < 2)
            {
                return RegistrationResult.MissingSecurityQuestion;
            }

            // Test duplicated security questions
            if (answerDtos.GroupBy(i => i.QuestionId).Any(i => i.Count() > 1))
            {
                return RegistrationResult.DuplicatedSecurityQuestion;
            }

            // Test duplicated security question answers
            if (answerDtos.GroupBy(i => i.Content.ToLower()).Any(i => i.Count() > 1))
            {
                return RegistrationResult.DuplicatedSecurityQuestionAnswer;
            }

            // Test security questions mush have answer
            if (answerDtos.Any(i => i.Content == null))
            {
                return RegistrationResult.MissingSecurityQuestionAnswer;
            }

            return RegistrationResult.Success;
        }
    }
}
