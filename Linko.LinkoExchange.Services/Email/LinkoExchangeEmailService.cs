using Linko.LinkoExchange.Data;
using System.Collections;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web.UI.WebControls;
using Linko.LinkoExchange.Core.Enum;
using System.Collections.Generic;
using System.Threading.Tasks;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Dto;
using System;
using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Settings;

namespace Linko.LinkoExchange.Services.Email
{
    public class LinkoExchangeEmailService : IEmailService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IAuditLogService _emailAuditLogService;
        private readonly IProgramService _programService;
        private readonly ISettingService _settingService;
        private readonly IRequestCache _requestCache;

        private string _emailServer = "";

        private readonly string _senderEmailAddres;
        private readonly string _senderFistName;
        private readonly string _senderLastName;

        public LinkoExchangeEmailService(
            LinkoExchangeContext linkoExchangeContext,
            IAuditLogService emailAuditLogService,
            IProgramService programService,
            ISettingService settingService,
            IRequestCache requestCache
            )
        {
            _dbContext = linkoExchangeContext;
            _emailAuditLogService = emailAuditLogService;
            _programService = programService;
            _settingService = settingService;
            _requestCache = requestCache;

            _senderEmailAddres = _settingService.GetGlobalSettings()[SystemSettingType.SystemEmailEmailAddress];
            _senderFistName = _settingService.GetGlobalSettings()[SystemSettingType.SystemEmailFirstName];
            _senderLastName = _settingService.GetGlobalSettings()[SystemSettingType.SystemEmailLastName];

            _emailServer = settingService.GetGlobalSettings()[SystemSettingType.EmailServer];
        }

        public async Task SendEmail(IEnumerable<string> recipients, EmailType emailType,
            IDictionary<string, string> contentReplacements, bool perRegulatoryProgram = true)
        {
            string sendTo = string.Join(separator: ",", values: recipients);

            var template = await GetTemplate(emailType);
            if (template == null)
            {
                return;
            }

            MailMessage msg = await GetMailMessage(sendTo, template, contentReplacements, _senderEmailAddres);
            if (string.IsNullOrWhiteSpace(_emailServer))
            {
                _emailServer = _settingService.GetGlobalSettings()[SystemSettingType.EmailServer];
            }

            if (string.IsNullOrWhiteSpace(_emailServer))
            {
                throw new ArgumentException("EmailServer");
            }

            using (var smtpClient = new SmtpClient(_emailServer))
            {
                smtpClient.Send(msg);
            }

            foreach (var receipientEmail in recipients)
            {
                var logEntries = GetEmailAuditLog(_senderEmailAddres, receipientEmail, emailType, msg.Subject, msg.Body, template.AuditLogTemplateId, perRegulatoryProgram);
                foreach (var log in logEntries)
                {
                    _emailAuditLogService.Log(log);
                }
            }
        }

        private Task<AuditLogTemplate> GetTemplate(EmailType emailType)
        {
            var emailTemplateName = string.Format("Email_{0}", emailType.ToString());
            return Task.FromResult(_dbContext.AuditLogTemplates.First(i => i.Name == emailTemplateName));
        }

        private Task<MailMessage> GetMailMessage(string sendTo, AuditLogTemplate emailTemplate,
            IDictionary<string, string> replacements, string senderEmail)
        {

            var keyValues = replacements.Select(i =>
            {
                return new KeyValuePair<string, string>("{" + i.Key + "}", i.Value);
            });

            replacements = keyValues.ToDictionary(i => i.Key, i => i.Value);

            var mailDefinition = new MailDefinition
            {
                IsBodyHtml = true,
                From = senderEmail,
                Subject = ReplaceUsingTemplates(emailTemplate.SubjectTemplate, keyValues)
            };

            var emailMessage = mailDefinition.CreateMailMessage(sendTo, (IDictionary)replacements,
                emailTemplate.MessageTemplate, new System.Web.UI.Control());
            return Task.FromResult(emailMessage);
        }

        private string ReplaceUsingTemplates(string originText, IEnumerable<KeyValuePair<string, string>> replacements)
        {
            foreach (var kv in replacements)
            {
                originText = originText.Replace(kv.Key, kv.Value);
            }

            return originText;
        }

        /// <summary>
        /// Return recipient log data.  If programFilters has value,  only return programIDs that exist in programFilters
        /// </summary>
        /// <param name="email">The email address of recipient.</param>
        /// <param name="programFilters">Filter collection of program IDs</param>
        /// <returns></returns>
        private IEnumerable<EmailAuditLogEntryDto> PopulateRecipientLogDataForAllPrograms(string email, ICollection<int> programFilters = null)
        {
            var emailAuditLogs = new List<EmailAuditLogEntryDto>();
            var oRpUs = _programService.GetUserRegulatoryPrograms(email);
            if (oRpUs != null && oRpUs.Any())
            {
                foreach (var user in oRpUs)
                {
                    if (user.OrganizationRegulatoryProgramDto != null)
                    {
                        var auditLog = new EmailAuditLogEntryDto
                        {
                            RecipientFirstName = user.UserProfileDto.FirstName,
                            RecipientLastName = user.UserProfileDto.LastName,
                            RecipientUserName = user.UserProfileDto.UserName,
                            RecipientUserProfileId = user.UserProfileId,

                            RecipientRegulatoryProgramId = user.OrganizationRegulatoryProgramId,
                            RecipientOrganizationId = user.OrganizationRegulatoryProgramDto.OrganizationId,
                            RecipientRegulatorOrganizationId = user.OrganizationRegulatoryProgramDto.RegulatorOrganizationId
                        };

                        emailAuditLogs.Add(auditLog);
                    }
                }
            }
            else
            {
                var auditLog = new EmailAuditLogEntryDto();
                emailAuditLogs.Add(auditLog);
            }

            if (programFilters != null && programFilters.Any())
            {
                return emailAuditLogs.Where(i => programFilters.Contains(i.RecipientRegulatoryProgramId.Value));
            }

            return emailAuditLogs;
        }

        private IEnumerable<EmailAuditLogEntryDto> PopulateSingleRecipientProgramData(string email)
        {
            var emailAuditLogs = new List<EmailAuditLogEntryDto>();
            var oRpUs = _programService.GetUserRegulatoryPrograms(email);
            if (oRpUs != null && oRpUs.Any())
            {
                var user = oRpUs.ToArray()[0]; // get the first one to get user first name, last, and user name; 
                var firstName = user.UserProfileDto.FirstName;
                var lastName = user.UserProfileDto.LastName;
                var userName = user.UserProfileDto.UserName;

                var auditLog = new EmailAuditLogEntryDto
                {
                    RecipientFirstName = firstName,
                    RecipientLastName = lastName,
                    RecipientUserName = userName,

                    RecipientRegulatoryProgramId =
                        ValueParser.TryParseInt(_requestCache.GetValue(CacheKey.EmailRecipientRegulatoryProgramId) as string, 0),
                    RecipientOrganizationId =
                        ValueParser.TryParseInt(_requestCache.GetValue(CacheKey.EmailRecipientOrganizationId) as string, 0),
                    RecipientRegulatorOrganizationId =
                        ValueParser.TryParseInt(_requestCache.GetValue(CacheKey.EmailRecipientRegulatoryOrganizationId) as string, 0)
                };

                emailAuditLogs.Add(auditLog);
            }
            else
            {
                emailAuditLogs.Add(new EmailAuditLogEntryDto());
            }

            return emailAuditLogs;
        }

        private IEnumerable<EmailAuditLogEntryDto> GetEmailAuditLog(string senderEmail, string receipientEmail, EmailType emailType, string subject, string body, int emailTemplateId, bool perRegulatoryPrrogram)
        {
            var emailAuditLogs = new List<EmailAuditLogEntryDto>();

            switch (emailType)
            {
                // Below type only needs to log one program or the recipient  
                case EmailType.Registration_AuthorityRegistrationDenied:
                case EmailType.Registration_IndustryRegistrationDenied:
                case EmailType.Registration_IndustryRegistrationApproved:
                case EmailType.Registration_AuthorityRegistrationApproved:
                case EmailType.Registration_InviteAuthorityUser:
                case EmailType.Registration_AuthorityInviteIndustryUser:
                case EmailType.Registration_IndustryInviteIndustryUser:
                case EmailType.Signature_SignatoryGranted:
                case EmailType.Signature_SignatoryRevoked:
                case EmailType.Registration_AuthorityUserRegistrationPendingToApprovers:
                case EmailType.Registration_IndustryUserRegistrationPendingToApprovers:
                case EmailType.Report_Submission_IU:
                case EmailType.Report_Submission_AU:
                    emailAuditLogs.AddRange(PopulateSingleRecipientProgramData(receipientEmail));
                    break;

                // Below type needs to log for all programs 
                case EmailType.UserAccess_AccountLockout:
                    if (perRegulatoryPrrogram)
                    {
                        emailAuditLogs.AddRange(PopulateRecipientLogDataForAllPrograms(receipientEmail));
                    }
                    else
                    {
                        emailAuditLogs.AddRange(PopulateSingleRecipientProgramData(receipientEmail));
                    }
                    break;
                case EmailType.UserAccess_LockoutToSysAdmins:
                case EmailType.Registration_ResetRequired:
                case EmailType.Profile_KBQFailedLockout:
                case EmailType.Profile_KBQChanged:
                case EmailType.Profile_EmailChanged:
                case EmailType.Profile_ProfileChanged:
                case EmailType.Profile_SecurityQuestionsChanged:
                case EmailType.Profile_PasswordChanged:
                case EmailType.ForgotPassword_ForgotPassword:
                case EmailType.Profile_ResetProfileRequired:
                case EmailType.ForgotUserName_ForgotUserName:
                case EmailType.Registration_RegistrationResetPending:
                    emailAuditLogs.AddRange(PopulateRecipientLogDataForAllPrograms(receipientEmail));
                    break;
                default:
                    throw new Exception("Not valid EmailType");
            }

            foreach (var log in emailAuditLogs)
            {
                log.SenderEmailAddress = _senderEmailAddres;
                log.SenderFirstName = _senderFistName;
                log.SenderLastName = _senderLastName;

                log.AuditLogTemplateId = emailTemplateId;
                log.Body = body;
                log.Subject = subject;
                log.RecipientEmailAddress = receipientEmail;
                log.SentDateTimeUtc = DateTimeOffset.UtcNow;
            }

            return emailAuditLogs;
        }
    }
}