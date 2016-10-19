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
using System.Web;
using Linko.LinkoExchange.Core.Common;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Program;

namespace Linko.LinkoExchange.Services.Email
{
    public class LinkoExchangeEmailService : IEmailService
    {
        private readonly LinkoExchangeContext _dbContext;
        private readonly IAuditLogService _emailAuditLogService;
        private readonly IProgramService _programService;

        private readonly string _emailServer = ConfigurationManager.AppSettings["EmailServer"];
        private readonly string _fromEmailAddress = ConfigurationManager.AppSettings["EmailSenderFromEmail"]; 
        public LinkoExchangeEmailService(
            LinkoExchangeContext linkoExchangeContext,
            EmailAuditLogService emailAuditLogService,
            IProgramService programService)
        {
            _dbContext = linkoExchangeContext;
            _emailAuditLogService = emailAuditLogService;
            _programService = programService; 
        }

        public async void SendEmail(IEnumerable<string> recipients, EmailType emailType,
            IDictionary<string, string> contentReplacements, IAuditLogEntry logEntry, string senderEmail = null)
        {
            string sendTo = string.Join(separator: ",", values: recipients);

            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                senderEmail = _fromEmailAddress;
            }

            var template = await GetTemplate(emailType);
            if (template == null) return;

            MailMessage msg = await GetMailMessage(sendTo, template, contentReplacements, senderEmail);
            using (var smtpClient = new SmtpClient(_emailServer))
            {
                smtpClient.Send(msg);
            }

            foreach (var receipientEmail in recipients)
            {
                var logEntries = GetEmailAuditLog(senderEmail, receipientEmail, emailType, msg.Subject, msg.Body);
                foreach (var log in logEntries)
                {
                    _emailAuditLogService.Log(logEntry);
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

            var emailMessage = mailDefinition.CreateMailMessage(sendTo, (IDictionary) replacements,
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
        /// Return receiptian log data.  If programFilters has value,  only return programIDs that exist in programFilters
        /// </summary>
        /// <param name="email">The email address of recipient.</param>
        /// <param name="programFilters">Filter collection of program IDs</param>
        /// <returns></returns>
        private IEnumerable<EmailAuditLog> GetRecipientLogData(string email, IEnumerable<int> programFilters = null)
        {
            var emailAuditLogs = new List<EmailAuditLog>();
            var oRpUs = _programService.GetUserPrograms(email);
            if (oRpUs.Any())
            {
                foreach (var user in oRpUs)
                {
                    if (user.OrganizationRegulatoryProgramDtos != null &&
                        user.OrganizationRegulatoryProgramDtos.Count > 0)
                    {
                        foreach (var program in user.OrganizationRegulatoryProgramDtos)
                        {
                            var auditLog = new EmailAuditLog
                            {
                                RecipientFirstName = user.UserProfileDto.FirstName,
                                RecipientLastName = user.UserProfileDto.LastName,
                                RecipientUserName = user.UserProfileDto.UserName,
                                RecipientRegulatoryProgramId = user.OragnizationRegulatoryProgramId,
                                RecipientRegulateeId = program.OrganizationId,
                                RecipientRegulatorId = program.RegulatorOrganizationId,
                            };

                            emailAuditLogs.Add(auditLog); 
                        }
                    }
                }
            }

            if (programFilters != null && programFilters.Any())
            {
                return emailAuditLogs.Where(i => programFilters.Contains(i.RecipientRegulatoryProgramId));
            }

            return emailAuditLogs;
        }

        private void UpdateSenderLogData(List<EmailAuditLog> logs)
        {
            if (logs == null || !logs.Any()) return;

            var currentRegulatorOrganziationId =
                ValueParser.TryParseInt(HttpContext.Current.Session["currentRegulatorOrganziationId"] as string, 0);
            var currentOrganizationId =
                ValueParser.TryParseInt(HttpContext.Current.Session["currentOrganziationId"] as string, 0);
            var currentUserRegulatoryProgramId = ValueParser.TryParseInt(HttpContext.Current.Session["currentUserRegulatoryProgramId"] as string, 0);

            var currentUserFirstName = HttpContext.Current.Session["currentUserFirstName"] as string;
            var currentUserLastName = HttpContext.Current.Session["currentUserLastName"] as string;
            var currentUserName = HttpContext.Current.Session["currentUserName"] as string;
            var currentUserEmail = HttpContext.Current.Session["currentUserName"] as string;

            foreach (var log in logs)
            {
                log.SenderEmailAddress = currentUserEmail;
                log.SenderRegulateeId = currentRegulatorOrganziationId;
                log.SenderRegulateeId = currentOrganizationId;
                log.SenderRegulatoryProgramId = currentUserRegulatoryProgramId;
                log.SenderFirstName = currentUserFirstName;
                log.SenderLastName = currentUserLastName;
                log.SenderUserName = currentUserName; 
            }

        }

        private void UpdateSenderLogDataToSystem(List<EmailAuditLog> logs)
        {
            if (logs == null || !logs.Any()) return;
            
            foreach (var log in logs)
            {
                log.SenderEmailAddress = _fromEmailAddress;
            }
        }

        private IEnumerable<EmailAuditLog> GetEmailAuditLog(string senderEmail, string receipientEmail, EmailType emailType, string subject, string body)
        { 

            var emailAuditLogs = new List<EmailAuditLog>();

            var logs = new List<EmailAuditLog>();
 
            switch (emailType)
            {
                case EmailType.Registration_AuthorityRegistrationDenied:   
                case EmailType.Registration_IndustryRegistrationDenied: 
                case EmailType.Registration_IndustryRegistrationApproved:
                case EmailType.Registration_AuthorityRegistrationApproved:
                case EmailType.UserAccess_AccountLockOut:
                    // Manual lock
                case EmailType.UserAccess_LockOutToSysAdmins:
                    // Manu Lock to system admin  
                case EmailType.Registration_RegistratioinResetRequired:
                case EmailType.Registration_InviteAuthorityUser:
                case EmailType.Registration_InviteIndustryUser:
                    logs.AddRange(GetRecipientLogData(receipientEmail));
                    UpdateSenderLogData(logs);
                    break;

                case EmailType.Signature_SignatoryGranted:  
                case EmailType.Signature_SignatoryRevoked:
                    var signatoryRequestOrgpUserId =
                        ValueParser.TryParseInt(
                            HttpContext.Current.Items["organizationRegulatoryProgramUserId"] as string, 0);

                    logs.AddRange(GetRecipientLogData(receipientEmail, new []{signatoryRequestOrgpUserId}));
                    UpdateSenderLogData(logs);
                    break;

                case EmailType.Profile_KBQFailedLockOut: 
                case EmailType.Profile_KBQChanged:  
                case EmailType.Profile_ProfileEmailChanged:  
                case EmailType.Profile_ProfileChanged:  
                case EmailType.Profile_SecurityQuestionsChanged:  
                case EmailType.Profile_ProfilePasswordChanged: 
                case EmailType.ForgotPassword_ForgotPassword:
                    logs.AddRange(GetRecipientLogData(receipientEmail));
                    UpdateSenderLogDataToSystem(logs);
                    break;

                case EmailType.Profile_ResetProfileRequired:
                    logs.AddRange(GetRecipientLogData(receipientEmail));
                    UpdateSenderLogData(logs);
                    break;

                case EmailType.Registration_AuthorityUserRegistrationPendingToApprovers:  
                case EmailType.Registration_IndustryUserRegistrationPendingToApprovers:
                    logs.AddRange(GetRecipientLogData(receipientEmail));
                    UpdateSenderLogDataToSystem(logs); 
                    break;

                case EmailType.ForgotUserName_ForgotUserName: 
                    var auditLog = new EmailAuditLog
                    {
                        AuditLogTemplateId = (int) EmailType.ForgotUserName_ForgotUserName,
                        SenderEmailAddress = _fromEmailAddress
                    };

                    emailAuditLogs.Add(auditLog);
                    break;

                case EmailType.Registration_RegistrationResetPending:
                    logs.AddRange(GetRecipientLogData(receipientEmail));
                    UpdateSenderLogDataToSystem(logs); 
                    break;
                default:
                    throw new Exception("Not valid EmailType");


            }

            foreach (var log in emailAuditLogs)
            {
                log.RecipientRegulatoryProgramId = (int) emailType;
                log.Body = body;
                log.Subject = subject;
                log.RecipientEmailAddress = receipientEmail;
                log.SentDateTimeUtc = DateTime.UtcNow;
            }

            return emailAuditLogs;
        }
    }
}