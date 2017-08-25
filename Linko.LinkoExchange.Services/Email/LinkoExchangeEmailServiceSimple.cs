using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Settings;
using NLog;

namespace Linko.LinkoExchange.Services.Email
{
    public class LinkoExchangeEmailServiceSimple : ILinkoExchangeEmailService
    {
        #region fields

        private readonly LinkoExchangeContext _dbContext;
        private readonly IAuditLogService _emailAuditLogService;
        private readonly ILogger _logger;
        private readonly IProgramService _programService;
        private readonly string _senderEmailAddres;
        private readonly string _senderFistName;
        private readonly string _senderLastName;
        private readonly ISettingService _settingService;
        private string _emailServer;

        #endregion

        #region constructors and destructor

        public LinkoExchangeEmailServiceSimple(
            LinkoExchangeContext linkoExchangeContext,
            IAuditLogService emailAuditLogService,
            IProgramService programService,
            ISettingService settingService,
            ILogger logger
        )
        {
            _dbContext = linkoExchangeContext;
            _emailAuditLogService = emailAuditLogService;
            _programService = programService;
            _settingService = settingService;

            _senderEmailAddres = _settingService.GetGlobalSettings()[key:SystemSettingType.SystemEmailEmailAddress];
            _senderFistName = _settingService.GetGlobalSettings()[key:SystemSettingType.SystemEmailFirstName];
            _senderLastName = _settingService.GetGlobalSettings()[key:SystemSettingType.SystemEmailLastName];

            _emailServer = settingService.GetGlobalSettings()[key:SystemSettingType.EmailServer];
            _logger = logger;
        }

        #endregion

        #region interface implementations

        public void SendEmails(List<EmailEntry> emailEntries)
        {
            if (emailEntries != null && emailEntries.Any())
            {
                foreach (var entry in emailEntries)
                {
                    WriteEmailAuditLog(emailEntry:entry);
                }

                _dbContext.SaveChanges();

                foreach (var entry in emailEntries)
                {
                    SendEmailInner(entry:entry);
                }
            }
        }

        public EmailEntry GetEmailEntryForOrgRegProgramUser(OrganizationRegulatoryProgramUserDto user, EmailType emailType, Dictionary<string, string> contentReplacements)
        {
            return new EmailEntry
                   {
                       EmailType = emailType,
                       RecipientEmailAddress = user.UserProfileDto?.Email,
                       RecipientFirstName = user.UserProfileDto?.FirstName,
                       RecipientLastName = user.UserProfileDto?.LastName,
                       RecipientUserName = user.UserProfileDto?.UserName,
                       RecipientUserProfileId = user.UserProfileId,
                       RecipientOrganizationId = user.OrganizationRegulatoryProgramDto?.OrganizationId,
                       RecipientOrgulatoryProgramId = user.OrganizationRegulatoryProgramDto?.RegulatoryProgramId,
                       RecipientRegulatorOrganizationId = user.OrganizationRegulatoryProgramDto?.RegulatorOrganizationId,
                       ContentReplacements = contentReplacements
                   };
        }

        /// <inheritdoc />
        public List<EmailEntry> GetAllProgramEmailEntiresForUser(UserProfile userProfile, EmailType emailType, Dictionary<string, string> contentReplacements)
        {
            var orgRegProgramUsers = _programService.GetActiveRegulatoryProgramUsers(email:userProfile.Email);

            return orgRegProgramUsers.Select(i => new EmailEntry
                                                  {
                                                      EmailType = emailType,
                                                      ContentReplacements = contentReplacements,
                                                      RecipientEmailAddress = userProfile.Email,
                                                      RecipientUserName = userProfile.UserName,
                                                      RecipientUserProfileId = userProfile.UserProfileId,
                                                      RecipientFirstName = userProfile.FirstName,
                                                      RecipientLastName = userProfile.LastName,
                                                      RecipientOrganizationId = i.OrganizationRegulatoryProgramDto.OrganizationId,
                                                      RecipientOrgulatoryProgramId = i.OrganizationRegulatoryProgramId,
                                                      RecipientRegulatorOrganizationId = i.OrganizationRegulatoryProgramDto.RegulatorOrganizationId
                                                  }).ToList();
        }

        public List<EmailEntry> GetAllProgramEmailEntiresForUser(UserDto user, EmailType emailType, Dictionary<string, string> contentReplacements)
        {
            var orgRegProgramUsers = _programService.GetActiveRegulatoryProgramUsers(email:user.Email);
            return orgRegProgramUsers.Select(i => new EmailEntry
                                                  {
                                                      EmailType = emailType,
                                                      ContentReplacements = contentReplacements,
                                                      RecipientEmailAddress = user.Email,
                                                      RecipientUserName = user.UserName,
                                                      RecipientUserProfileId = user.UserProfileId,
                                                      RecipientFirstName = user.FirstName,
                                                      RecipientLastName = user.LastName,
                                                      RecipientOrganizationId = i.OrganizationRegulatoryProgramDto.OrganizationId,
                                                      RecipientOrgulatoryProgramId = i.OrganizationRegulatoryProgramId,
                                                      RecipientRegulatorOrganizationId = i.OrganizationRegulatoryProgramDto.RegulatorOrganizationId
                                                  }).ToList();
        }

        public EmailEntry GetEmailEntryForUser(UserProfile user, EmailType emailType, Dictionary<string, string> contentReplacements, OrganizationRegulatoryProgram orgRegProg)
        {
            return new EmailEntry
                   {
                       EmailType = emailType,
                       RecipientUserName = user.UserName,
                       RecipientUserProfileId = user.UserProfileId,
                       RecipientEmailAddress = user.Email,
                       RecipientFirstName = user.FirstName,
                       RecipientLastName = user.LastName,
                       RecipientOrganizationId = orgRegProg.OrganizationId,
                       RecipientOrgulatoryProgramId = orgRegProg.RegulatoryProgramId,
                       RecipientRegulatorOrganizationId = orgRegProg.RegulatorOrganizationId,
                       ContentReplacements = contentReplacements
                   };
        }

        public EmailEntry GetEmailEntryForUser(UserDto user, EmailType emailType, Dictionary<string, string> contentReplacements, OrganizationRegulatoryProgramDto orgRegProg)
        {
            return new EmailEntry
                   {
                       EmailType = emailType,
                       RecipientUserName = user.UserName,
                       RecipientUserProfileId = user.UserProfileId,
                       RecipientEmailAddress = user.Email,
                       RecipientFirstName = user.FirstName,
                       RecipientLastName = user.LastName,
                       RecipientOrganizationId = orgRegProg.OrganizationId,
                       RecipientOrgulatoryProgramId = orgRegProg.RegulatoryProgramId,
                       RecipientRegulatorOrganizationId = orgRegProg.RegulatorOrganizationId,
                       ContentReplacements = contentReplacements
                   };
        }

        public EmailEntry GetEmailEntryForUser(UserDto user, EmailType emailType, Dictionary<string, string> contentReplacements, OrganizationRegulatoryProgram orgRegProg)
        {
            return new EmailEntry
                   {
                       EmailType = emailType,
                       RecipientUserName = user.UserName,
                       RecipientUserProfileId = user.UserProfileId,
                       RecipientEmailAddress = user.Email,
                       RecipientFirstName = user.FirstName,
                       RecipientLastName = user.LastName,
                       RecipientOrganizationId = orgRegProg.OrganizationId,
                       RecipientOrgulatoryProgramId = orgRegProg.RegulatoryProgramId,
                       RecipientRegulatorOrganizationId = orgRegProg.RegulatorOrganizationId,
                       ContentReplacements = contentReplacements
                   };
        }

        #endregion

        private void SendEmailInner(EmailEntry entry)
        {
            try
            {
                using (var smtpClient = new SmtpClient(host:_emailServer))
                {
                    smtpClient.Send(message:entry.MailMessage);
                    _logger.Info(message:$"#LogToEmailLogFile - LinkoExchangeEmailService. SendEmail successful. Email Audit Log ID: {entry.AuditLogId}, "
                                         + $"Recipient User name:{entry.RecipientUserName}, Recipient Email Address:{entry.RecipientEmailAddress}, "
                                         + $"Subject:{entry.MailMessage.Subject}");
                }
            }
            catch (Exception ex)
            {
                var errors = new List<string> {ex.Message};
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    errors.Add(item:ex.Message);
                }

                _logger.Info(message:$"#LogToEmailLogFile - LinkoExchangeEmailService. SendEmail fails. Email Audit Log ID: {entry.AuditLogId}, "
                                     + $"Recipient User name:{entry.RecipientUserName}, Recipient Email Address:{entry.RecipientEmailAddress}, "
                                     + $"Subject:{entry.MailMessage.Subject}");
                _logger.Error(message:$"#LogToEmailLogFile - LinkoExchangeEmailService. SendEmail fails", argument:string.Join(",", Environment.NewLine, errors));
            }
        }

        private void WriteEmailAuditLog(EmailEntry emailEntry)
        {
            var sendTo = emailEntry.RecipientEmailAddress;
            var template = GetTemplate(emailType:emailEntry.EmailType).Result;
            if (template == null)
            {
                throw new Exception(message:$"Email Audit Log template is missing for EmailType: {emailEntry.EmailType}");
            }

            var msg = GetMailMessage(sendTo:sendTo, emailTemplate:template, replacements:emailEntry.ContentReplacements, senderEmail:_senderEmailAddres).Result;
            emailEntry.MailMessage = msg;

            if (string.IsNullOrWhiteSpace(value:_emailServer))
            {
                _emailServer = _settingService.GetGlobalSettings()[key:SystemSettingType.EmailServer];
            }

            if (string.IsNullOrWhiteSpace(value:_emailServer))
            {
                _logger.Fatal(message:"EmailServer value in tSystemSetting table is null or blank.");
                throw new ArgumentException(message:"EmailServer value is missing.");
            }

            var logEntry = GetEmailAuditLog(emailEntry:emailEntry, emailTemplateId:template.AuditLogTemplateId);
            _emailAuditLogService.Log(logEntry:logEntry);
            emailEntry.AuditLogId = logEntry.EmailAuditLogId;
        }

        private EmailAuditLogEntryDto GetEmailAuditLog(EmailEntry emailEntry, int emailTemplateId)
        {
            var entry = new EmailAuditLogEntryDto
                        {
                            RecipientFirstName = emailEntry.RecipientFirstName,
                            RecipientLastName = emailEntry.RecipientLastName,
                            RecipientUserName = emailEntry.RecipientUserName,
                            RecipientUserProfileId = emailEntry.RecipientUserProfileId,
                            RecipientRegulatoryProgramId = emailEntry.RecipientOrgulatoryProgramId,
                            RecipientOrganizationId = emailEntry.RecipientOrganizationId,
                            RecipientRegulatorOrganizationId = emailEntry.RecipientRegulatorOrganizationId,
                            SenderEmailAddress = _senderEmailAddres,
                            SenderFirstName = _senderFistName,
                            SenderLastName = _senderLastName,
                            SenderUserProfileId = null,
                            Body = emailEntry.MailMessage.Body,
                            Subject = emailEntry.MailMessage.Subject,
                            RecipientEmailAddress = emailEntry.RecipientEmailAddress,
                            SentDateTimeUtc = DateTimeOffset.Now,
                            AuditLogTemplateId = emailTemplateId
                        };

            if (entry.RecipientRegulatorOrganizationId.HasValue == false)
            {
                entry.RecipientRegulatorOrganizationId = entry.RecipientOrganizationId;
            }

            if (entry.RecipientUserProfileId == 0)
            {
                entry.RecipientUserProfileId = null;
            }

            return entry;
        }

        private Task<AuditLogTemplate> GetTemplate(EmailType emailType)
        {
            var emailTemplateName = $"Email_{emailType}";
            return Task.FromResult(result:_dbContext.AuditLogTemplates.First(i => i.Name == emailTemplateName));
        }

        private Task<MailMessage> GetMailMessage(string sendTo, AuditLogTemplate emailTemplate, IDictionary<string, string> replacements, string senderEmail)
        {
            var keyValues = replacements.Select(i => new KeyValuePair<string, string>(key:"{" + i.Key + "}", value:i.Value ?? "")).ToList();

            replacements = keyValues.ToDictionary(i => i.Key, i => i.Value);

            var mailDefinition = new MailDefinition
                                 {
                                     IsBodyHtml = true,
                                     From = senderEmail,
                                     Subject = ReplaceUsingTemplates(originText:emailTemplate.SubjectTemplate, replacements:keyValues)
                                 };

            var emailMessage = mailDefinition.CreateMailMessage(recipients:sendTo, replacements:(IDictionary) replacements,
                                                                body:emailTemplate.MessageTemplate, owner:new Control());
            return Task.FromResult(result:emailMessage);
        }

        private string ReplaceUsingTemplates(string originText, IEnumerable<KeyValuePair<string, string>> replacements)
        {
            foreach (var kv in replacements)
            {
                originText = originText.Replace(oldValue:kv.Key, newValue:kv.Value);
            }

            return originText;
        }
    }
}