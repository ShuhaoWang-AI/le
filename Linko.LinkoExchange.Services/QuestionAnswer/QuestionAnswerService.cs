using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Microsoft.AspNet.Identity;

namespace Linko.LinkoExchange.Services.QuestionAnswer
{
    public class QuestionAnswerService:IQuestionAnswerService
    {
        #region fields

        private readonly ICromerrAuditLogService _crommerAuditLogService;
        private readonly LinkoExchangeContext _dbContext;
        private readonly IEncryptionService _encryption;
        private readonly IDictionary<SystemSettingType, string> _globalSettings;
        private readonly IHttpContextService _httpContext;
        private readonly ILinkoExchangeEmailService _linkoExchangeEmailService;
        private readonly IOrganizationService _orgService;
        private readonly IPasswordHasher _passwordHasher;

        #endregion

        #region constructors and destructor

        public QuestionAnswerService(
            LinkoExchangeContext dbContext,
            IHttpContextService httpContext,
            IEncryptionService encryption,
            IPasswordHasher passwordHasher,
            ISettingService settingService,
            IOrganizationService orgService,
            ICromerrAuditLogService crommerAuditLogService,
            ILinkoExchangeEmailService linkoExchangeEmailService)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
            _encryption = encryption;
            _passwordHasher = passwordHasher;
            _globalSettings = settingService.GetGlobalSettings();
            _orgService = orgService;
            _crommerAuditLogService = crommerAuditLogService;
            _linkoExchangeEmailService = linkoExchangeEmailService;
        }

        #endregion

        #region interface implementations

        public void CreateOrUpdateUserQuestionAnswer(int userProfileId, AnswerDto answerDto)
        {
            var question = _dbContext.Questions.Include(q => q.QuestionType)
                                     .Single(q => q.QuestionId == answerDto.QuestionId);
            if (question.QuestionType.Name == QuestionTypeName.KBQ.ToString())
            {
                //Hash answer
                answerDto.Content = _passwordHasher.HashPassword(password:answerDto.Content.Trim().ToLower()); //CASE INSENSITIVE -- LOWER CASE ONLY
            }
            else if (question.QuestionType.Name == QuestionTypeName.SQ.ToString())
            {
                //Encrypt answer
                answerDto.Content = _encryption.EncryptString(readableString:answerDto.Content.Trim());
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
                _dbContext.UserQuestionAnswers.Add(entity:answer);
            }

            _dbContext.SaveChanges();
        }

        /// <summary>
        ///     Should be used when creating or updating the entire collection of user's questions.
        ///     Performs checks for duplicated questions and/or same answers used for more than 1 question.
        ///     Also sends email as per the use case(s).
        /// </summary>
        /// <param name="userProfileId"> User </param>
        /// <param name="questionAnswers"> Collection of Dtos </param>
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
                    || _dbContext.UserQuestionAnswers.Single(q => q.UserQuestionAnswerId == answerDto.UserQuestionAnswerId).Content != answerDto.Content)
                {
                    //dirty
                    if (updatedDirtyAnswerList.Contains(item:answerDto.Content))
                    {
                        return CreateOrUpdateAnswersResult.DuplicateAnswersInNew; //duplicate new un-hashed answers
                    }
                    else
                    {
                        updatedDirtyAnswerList.Add(item:answerDto.Content);
                        answerDtosToUpdate.Add(item:answerDto);
                    }
                }
                else
                {
                    //clean

                    //kbq or sq?
                    var question = _dbContext.Questions.Include(q => q.QuestionType).Single(q => q.QuestionId == answerDto.QuestionId);
                    if (question.QuestionType.Name == QuestionTypeName.KBQ.ToString())
                    {
                        //Hashed answer
                        cleanHashedAnswerList.Add(item:answerDto.Content);
                    }
                    else if (question.QuestionType.Name == QuestionTypeName.SQ.ToString())
                    {
                        //Encrypted answer
                        cleanEncryptedAnswerList.Add(item:answerDto.Content);
                    }
                }
            }

            //Check new answers don't already exist within old answers
            foreach (var newAnswer in updatedDirtyAnswerList)
            {
                foreach (var hashedAnswer in cleanHashedAnswerList)
                {
                    if (_passwordHasher.VerifyHashedPassword(hashedPassword:hashedAnswer, providedPassword:newAnswer.Trim().ToLower()) == PasswordVerificationResult.Success)
                    {
                        return CreateOrUpdateAnswersResult.DuplicateAnswersInNewAndExisting; //duplicate found
                    }
                }

                foreach (var encryptedAnswer in cleanEncryptedAnswerList)
                {
                    if (_encryption.EncryptString(readableString:newAnswer.Trim()) == encryptedAnswer)
                    {
                        return CreateOrUpdateAnswersResult.DuplicateAnswersInNewAndExisting; //duplicate found
                    }
                }
            }

            //Check for duplicate questions
            var duplicateQuestions = questionAnswers.GroupBy(x => new {x.QuestionId})
                                                    .Where(g => g.Count() > 1)
                                                    .Select(y => y.Key)
                                                    .ToList();

            if (duplicateQuestions.Any())
            {
                return CreateOrUpdateAnswersResult.DuplicateQuestionsInNewAndExisting;
            }

            var questionCountKbq = 0;
            var questionCountSq = 0;

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
                        var existingRow = _dbContext.UserQuestionAnswers.SingleOrDefault(uqa => uqa.UserProfileId == userProfileId && uqa.QuestionId == questionAnswer.QuestionId);

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
                                _dbContext.UserQuestionAnswers.Remove(entity:existingRow);
                                _dbContext.SaveChanges();

                                //Remove the Id to force "Create New" in method: "CreateOrUpdateUserQuestionAnswer"
                                questionAnswer.UserQuestionAnswerId = null;
                            }
                        }
                    }

                    //Now we can save to the database without risk of key violation error
                    foreach (var questionAnswer in answerDtosToUpdate)
                    {
                        var questionTypeName = _dbContext.Questions.Include(q => q.QuestionType).Single(q => q.QuestionId == questionAnswer.QuestionId).QuestionType.Name;
                        questionCountKbq += questionTypeName == QuestionTypeName.KBQ.ToString() ? 1 : 0;
                        questionCountSq += questionTypeName == QuestionTypeName.SQ.ToString() ? 1 : 0;

                        CreateOrUpdateUserQuestionAnswer(userProfileId:userProfileId, answerDto:questionAnswer);
                    }

                    var userProfile = _dbContext.Users.Single(u => u.UserProfileId == userProfileId);

                    // Cromerr audit log
                    var allOrgRegProgramUsers = _dbContext.OrganizationRegulatoryProgramUsers.Include(orpu => orpu.OrganizationRegulatoryProgram)
                                                          .Where(orpu => orpu.UserProfileId == userProfileId);

                    foreach (var orgRegProgramUser in allOrgRegProgramUsers)
                    {
                        var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                                      {
                                                          RegulatoryProgramId = orgRegProgramUser.OrganizationRegulatoryProgram.RegulatoryProgramId,
                                                          OrganizationId = orgRegProgramUser.OrganizationRegulatoryProgram.OrganizationId
                                                      };
                        cromerrAuditLogEntryDto.RegulatorOrganizationId = orgRegProgramUser.OrganizationRegulatoryProgram.RegulatorOrganizationId
                                                                          ?? cromerrAuditLogEntryDto.OrganizationId;
                        cromerrAuditLogEntryDto.UserProfileId = userProfile.UserProfileId;
                        cromerrAuditLogEntryDto.UserName = userProfile.UserName;
                        cromerrAuditLogEntryDto.UserFirstName = userProfile.FirstName;
                        cromerrAuditLogEntryDto.UserLastName = userProfile.LastName;
                        cromerrAuditLogEntryDto.UserEmailAddress = userProfile.Email;
                        cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                        cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
                        var cromerrContentReplacements = new Dictionary<string, string>
                                                         {
                                                             {"firstName", userProfile.FirstName},
                                                             {"lastName", userProfile.LastName},
                                                             {"userName", userProfile.UserName},
                                                             {"emailAddress", userProfile.Email}
                                                         };

                        if (questionCountKbq > 0)
                        {
                            _crommerAuditLogService.Log(eventType:CromerrEvent.Profile_KBQChanged, dto:cromerrAuditLogEntryDto, contentReplacements:cromerrContentReplacements);
                        }
                        if (questionCountSq > 0)
                        {
                            _crommerAuditLogService.Log(eventType:CromerrEvent.Profile_SQChanged, dto:cromerrAuditLogEntryDto, contentReplacements:cromerrContentReplacements);
                        }
                    }

                    //Send Emails
                    var contentReplacements = new Dictionary<string, string>();
                    var supportPhoneNumber = _globalSettings[key:SystemSettingType.SupportPhoneNumber];
                    var supportEmail = _globalSettings[key:SystemSettingType.SupportEmailAddress];

                    var authorityList = _orgService.GetUserAuthorityListForEmailContent(userProfileId:userProfileId);
                    contentReplacements.Add(key:"firstName", value:userProfile.FirstName);
                    contentReplacements.Add(key:"lastName", value:userProfile.LastName);

                    contentReplacements.Add(key:"authorityList", value:authorityList);
                    contentReplacements.Add(key:"supportPhoneNumber", value:supportPhoneNumber);
                    contentReplacements.Add(key:"supportEmail", value:supportEmail);

                    // Profile_KBQChanged, and Profile_SecurityQuestionsChanged emails are logged for all programs 
                    var emailEntries = new List<EmailEntry>();

                    if (questionCountKbq > 0)
                    {
                        emailEntries = _linkoExchangeEmailService.GetAllProgramEmailEntiresForUser(userProfile:userProfile, emailType:EmailType.Profile_KBQChanged,
                                                                                                   contentReplacements:contentReplacements);
                    }

                    if (questionCountSq > 0)
                    {
                        emailEntries.AddRange(collection:_linkoExchangeEmailService.GetAllProgramEmailEntiresForUser(userProfile:userProfile,
                                                                                                                     emailType:EmailType.Profile_SecurityQuestionsChanged,
                                                                                                                     contentReplacements:contentReplacements));
                    }

                    // Do email audit log.
                    _linkoExchangeEmailService.WriteEmailAuditLogs(emailEntries:emailEntries);

                    transaction.Commit();

                    // Send emails.
                    _linkoExchangeEmailService.SendEmails(emailEntries:emailEntries);

                    return CreateOrUpdateAnswersResult.Success;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdateAnswer(AnswerDto answer)
        {
            if (answer?.UserQuestionAnswerId != null && answer.UserQuestionAnswerId > 0)
            {
                var answerToUpdate = _dbContext.UserQuestionAnswers
                                               .Include(path:"Question")
                                               .Single(a => a.UserQuestionAnswerId == answer.UserQuestionAnswerId);

                if (answerToUpdate.Question.QuestionType.Name == QuestionTypeName.KBQ.ToString())
                {
                    //Hash answer
                    answer.Content = _passwordHasher.HashPassword(password:answer.Content.Trim().ToLower());
                }
                else if (answerToUpdate.Question.QuestionType.Name == QuestionTypeName.SQ.ToString())
                {
                    //Encrypt answer
                    answer.Content = _encryption.EncryptString(readableString:answer.Content.Trim());
                }

                answerToUpdate.QuestionId = answer.QuestionId;
                answerToUpdate.Content = answer.Content;
                answerToUpdate.LastModificationDateTimeUtc = DateTimeOffset.Now;

                _dbContext.SaveChanges();
            }
            else
            {
                var validationIssues = new List<RuleViolation> {new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:"Question update attempt failed.")};

                //_logger.Error("SubmitPOMDetails. Null question or missing QuestionId.");
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }
        }

        public ICollection<QuestionAnswerPairDto> GetUsersQuestionAnswers(int userProfileId, QuestionTypeName questionType)
        {
            var usersQaList = new List<QuestionAnswerPairDto>();
            var foundQAs = _dbContext.UserQuestionAnswers.Include(a => a.Question.QuestionType)
                                     .Where(a => a.UserProfileId == userProfileId && a.Question.QuestionType.Name == questionType.ToString()).ToList();

            if (foundQAs.Any())
            {
                foreach (var foundQa in foundQAs)
                {
                    var newQaDto = new QuestionAnswerPairDto {Answer = new AnswerDto(), Question = new QuestionDto()};

                    newQaDto.Answer.UserQuestionAnswerId = foundQa.UserQuestionAnswerId;

                    newQaDto.Answer.Content = questionType == QuestionTypeName.SQ ? _encryption.DecryptString(encryptedString:foundQa.Content) : foundQa.Content;

                    newQaDto.Question.QuestionId = foundQa.Question.QuestionId;
                    newQaDto.Question.IsActive = foundQa.Question.IsActive;
                    newQaDto.Question.QuestionType = (QuestionTypeName) Enum.Parse(enumType:typeof(QuestionTypeName), value:foundQa.Question.QuestionType.Name, ignoreCase:true);
                    newQaDto.Question.Content = foundQa.Question.Content;

                    usersQaList.Add(item:newQaDto);
                }
            }

            return usersQaList;
        }

        public QuestionAnswerPairDto GetRandomQuestionAnswerFromToken(string token, QuestionTypeName questionType)
        {
            //Find UserProfileId from EmailAuditLog.Recipient
            var emailAuditLog = _dbContext.EmailAuditLogs.FirstOrDefault(e => e.Token == token);
            if (emailAuditLog?.RecipientUserProfileId != null)
            {
                var userProfileId = emailAuditLog.RecipientUserProfileId;
                return GetRandomQuestionAnswerFromUserProfileId(userProfileId:userProfileId.Value, questionType:questionType);
            }
            else
            {
                throw new Exception(message:$"ERROR: Cannot find email audit log for token={token}");
            }
        }

        public QuestionAnswerPairDto GetRandomQuestionAnswerFromUserProfileId(int userProfileId, QuestionTypeName questionType)
        {
            var qAndAs = GetUsersQuestionAnswers(userProfileId:userProfileId, questionType:questionType);
            return qAndAs.OrderBy(qu => Guid.NewGuid()).First();
        }

        public void CreateUserQuestionAnswers(int userProfileId, IEnumerable<AnswerDto> questionAnswers)
        {
            var answerDtos = questionAnswers as IList<AnswerDto> ?? questionAnswers.ToList();
            if (answerDtos.Any())
            {
                foreach (var qa in answerDtos)
                {
                    CreateOrUpdateUserQuestionAnswer(userProfileId:userProfileId, answerDto:qa);
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
                    _dbContext.UserQuestionAnswers.Remove(entity:answer);
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
                              QuestionType = (QuestionTypeName) Enum.Parse(enumType:typeof(QuestionTypeName), value:question.QuestionType.Name, ignoreCase:true)
                          };

                list.Add(item:dto);
            }

            return list;
        }

        public bool ConfirmCorrectAnswer(int userQuestionAnswerId, string answer)
        {
            var questionAnswer = _dbContext.UserQuestionAnswers.Include(q => q.Question.QuestionType)
                                           .Single(u => u.UserQuestionAnswerId == userQuestionAnswerId);

            if (questionAnswer.Question.QuestionType.Name == QuestionTypeName.KBQ.ToString())
            {
                return _passwordHasher.VerifyHashedPassword(hashedPassword:questionAnswer.Content, providedPassword:answer.Trim().ToLower()) == PasswordVerificationResult.Success;
            }
            else
            {
                return _encryption.EncryptString(readableString:answer.Trim()) == questionAnswer.Content;
            }
        }

        /// <summary>
        /// Validates the user question answer.
        /// </summary>
        /// <param name="userProfileId">The user profile identifier.</param>
        /// <param name="questionAnswerDto">The question answer dto.</param>
        /// <returns></returns>
        public IEnumerable<RegistrationResult> ValidateUserQuestionAnswer(int userProfileId, AnswerDto questionAnswerDto)
        {
             if(questionAnswerDto.QuestionTypeName == QuestionTypeName.KBQ) 
                return ValidateKbq(userProfileId, questionAnswerDto); 
             else
                return ValidateSecurityQuestion(userProfileId, questionAnswerDto);
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

            // Test KBQ questions must have answer
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

        #endregion

        public void UpdateQuestion(QuestionDto question)
        {
            if (question?.QuestionId != null && question.QuestionId > 0)
            {
                var questionToUpdate = _dbContext.Questions.Single(q => q.QuestionId == question.QuestionId);
                questionToUpdate.Content = question.Content;
                questionToUpdate.QuestionTypeId = _dbContext.QuestionTypes.Single(q => q.Name == question.QuestionType.ToString()).QuestionTypeId;
                questionToUpdate.IsActive = question.IsActive;
                questionToUpdate.LastModificationDateTimeUtc = DateTimeOffset.Now;
                questionToUpdate.LastModifierUserId = Convert.ToInt32(value:_httpContext.CurrentUserProfileId());

                _dbContext.SaveChanges();
            }
            else
            {
                var validationIssues = new List<RuleViolation> {new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:"Question update attempt failed.")};

                //_logger.Error("SubmitPOMDetails. Null question or missing QuestionId.");
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }
        }

        private IEnumerable<RegistrationResult> ValidateKbq(int userProfileId, AnswerDto kbq)
        {
            var results = new List<RegistrationResult>(); 

            // check if the quetionId is duplicated 
            if (_dbContext.UserQuestionAnswers.Any(i => i.UserProfileId == userProfileId && i.QuestionId == kbq.QuestionId && i.UserQuestionAnswerId != kbq.UserQuestionAnswerId))
            {
                results.Add(RegistrationResult.DuplicatedKBQ);
            }

            if (string.IsNullOrWhiteSpace(value:kbq.Content))
            {
                if (kbq.QuestionTypeName == QuestionTypeName.KBQ)
                {
                    results.Add(RegistrationResult.MissingKBQAnswer);
                }
            }
            else
            {
                // check if the content is duplicated with others 
                if (kbq.QuestionTypeName == QuestionTypeName.KBQ)
                {
                    var userQuestionAnswers = _dbContext.UserQuestionAnswers.Where(i => i.UserProfileId == userProfileId && i.UserQuestionAnswerId != kbq.UserQuestionAnswerId
                                                                                        && i.Question.QuestionType.Name == QuestionTypeName.KBQ.ToString()).ToList();
                    foreach (var uqa in userQuestionAnswers)
                    {
                        var verifyKbqAnser = _passwordHasher.VerifyHashedPassword(hashedPassword:uqa.Content, providedPassword:kbq.Content);
                        if (verifyKbqAnser == PasswordVerificationResult.Success)
                        {
                            results.Add(RegistrationResult.DuplicatedKBQAnswer);
                            break;
                        }
                    }
                }
            }

            if(!results.Any())
            {
                results.Add(RegistrationResult.Success); 
            }

            return results;  
        }

        private IEnumerable<RegistrationResult> ValidateSecurityQuestion(int userProfileId, AnswerDto securityQuestion)
        {
             var results = new List<RegistrationResult>(); 
             
            // check if the quetionId is duplicated 
            if (_dbContext.UserQuestionAnswers.Any(i =>
                                                       i.UserProfileId == userProfileId
                                                       && i.QuestionId == securityQuestion.QuestionId
                                                       && i.UserQuestionAnswerId != securityQuestion.UserQuestionAnswerId))
            {
                results.Add(RegistrationResult.DuplicatedSecurityQuestion);
            }

            
            if (string.IsNullOrWhiteSpace(value:securityQuestion.Content))
            {
                results.Add(RegistrationResult.MissingSecurityQuestionAnswer);
            } 
            else
            {
                // check if the content is duplicated with others 
                var userQuestionAnswers = _dbContext.UserQuestionAnswers.Where(i => i.UserProfileId == userProfileId && i.UserQuestionAnswerId != securityQuestion.UserQuestionAnswerId
                                                                                    && i.Question.QuestionType.Name == QuestionTypeName.SQ.ToString()).ToList();
                foreach (var uqa in userQuestionAnswers)
                {
                    if (StringCipher.Decrypt(cipherText:uqa.Content).Equals(value:securityQuestion.Content))
                    {
                        results.Add(RegistrationResult.DuplicatedSecurityQuestionAnswer);
                        break;
                    }
                }
            }
            
            if(!results.Any())
            {
                results.Add(RegistrationResult.Success); 
            }

            return results;  
        }
    }
}