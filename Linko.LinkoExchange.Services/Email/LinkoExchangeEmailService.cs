using Linko.LinkoExchange.Data;
using System.Collections;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Linko.LinkoExchange.Core.Enum;
using System.Collections.Generic;
using System;

namespace Linko.LinkoExchange.Services.Email
{
    public interface IEmailService
    {
        void SendEmail(string sendTo, string subject, EmailType emailType, IDictionary replacements);  
        void SendEmail(IList<string> receiveers, EmailType emailType, IDictionary subjectReplacements, IDictionary contentReplacements); 
    }

    public class LinkoExchangeEmailService2 : IEmailService
    {
        private readonly ApplicationDbContext _linkoExchangeDbContext = new ApplicationDbContext();

        private readonly string EmailServer = ConfigurationManager.AppSettings["EmailServer"];

        public void SendEmail(string sendTo, string subject, EmailType emailType, IDictionary replacements)
        {
            throw new NotImplementedException();
        }

        public void SendEmail(IList<string> receiveers, EmailType emailType, IDictionary subjectReplacements, IDictionary contentReplacements)
        {
            EventCategory eventCategory;
            EventType eventType;

            switch (emailType)
            {
                case EmailType.AuthorityRegistrationDenied:
                    eventCategory = EventCategory.Registration;
                    eventType = EventType.AuthorityRegistrationDenied;
                    break;
                case EmailType.IndustryRegistrationDenied:
                    eventCategory = EventCategory.Registration;
                    eventType = EventType.IndustryRegistrationDenied;
                    break;
                case EmailType.IndustryRegistrationApproved:
                    eventCategory = EventCategory.Registration;
                    eventType = EventType.IndustryRegistrationApproved;
                    break;
                case EmailType.AuthorityRegistrationApproved:
                    eventCategory = EventCategory.Registration;
                    eventType = EventType.AuthorityRegistrationApproved;
                    break;
                case EmailType.AccountLockOut:
                    eventCategory = EventCategory.UserAccess;
                    eventType = EventType.AccountLockOut;
                    break;
                case EmailType.LockOutToSysAdmins:
                    eventCategory = EventCategory.UserAccess;
                    eventType = EventType.LockOutToSysAdmins;
                    break;
                case EmailType.RegistratioinResetRequired:
                    eventCategory = EventCategory.Registration;
                    eventType = EventType.ResetRequired;
                    break; 
                case EmailType.InviteAuthorityUser:
                    eventCategory = EventCategory.Registration;
                    eventType = EventType.InviteAuthorityUser;
                    break;

                case EmailType.InviteIndustryUser:
                    eventCategory = EventCategory.Registration;
                    eventType = EventType.InviteIndustryUser;
                    break;

                case EmailType.SignatoryGranted:
                    eventCategory = EventCategory.Signature;
                    eventType = EventType.SignatoryGranted;
                    break;

                case EmailType.SignatoryRevoked:
                    eventCategory = EventCategory.Signature;
                    eventType = EventType.SignatoryRevoked;
                    break;

                case EmailType.KBQFailedLockOut:
                    eventCategory = EventCategory.Profile;
                    eventType = EventType.KBQFailedLockOut;
                    break;

                case EmailType.ProfileChanged:
                    eventCategory = EventCategory.Profile;
                    eventType = EventType.ProfileChanged;
                    break;

                case EmailType.ProfileEmailChanged:
                    eventCategory = EventCategory.Profile;
                    eventType = EventType.EmailChanged;
                    break;

                case EmailType.KBQChanged:
                    eventCategory = EventCategory.Profile;
                    eventType = EventType.KBQChanged;
                    break;

                case EmailType.SecurityQuestionsChanged:
                    eventCategory = EventCategory.Profile;
                    eventType = EventType.SecurityQuestionsChanged;
                    break;

                case EmailType.ProfilePasswordChanged:
                    eventCategory = EventCategory.Profile;
                    eventType = EventType.PasswordChanged;
                    break;

                case EmailType.ForgotPassword:
                    eventCategory = EventCategory.ForgotPassword;
                    eventType = EventType.ForgotPassword;
                    break;

                case EmailType.ResetProfileRequired:
                    eventCategory = EventCategory.Profile;
                    eventType = EventType.ResetProfileRequired;
                    break;
                    
                case EmailType.IndustryUserRegistrationPendingToApprovers:
                    eventCategory = EventCategory.Registration;
                    eventType = EventType.IndustryUserRegistrationPendingToApprovers;
                    break; 

                case EmailType.AuthorityUserRegistrationPendingToApprovers:
                    eventCategory = EventCategory.Registration;
                    eventType = EventType.AuthorityUserRegistrationPendingToApprovers;
                    break;

                case EmailType.ForgotUserName:
                    eventCategory = EventCategory.ForgotUserName;
                    eventType = EventType.ForgotUserName;
                    break;

                case EmailType.RegistrationResetPending:
                    eventCategory = EventCategory.Registration;
                    eventType = EventType.RegistrationResetPending;
                    break;
                default:
                    throw new Exception("Not valid EmailType");
            }


            var template = _linkoExchangeDbContext.AuditLogTemplates.FirstOrDefault(i => i.TemplateType == "Email"
                   && i.EventCategory == eventCategory.ToString()
                   && i.EventType == eventType.ToString()
               ); 


        }
    }

    public class LinkoExchangeEmailService
    { 
        private static readonly ApplicationDbContext LinkoExchangeDbContext = new ApplicationDbContext(); 

        private static readonly string EmailServer = ConfigurationManager.AppSettings["EmailServer"];  

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="sendTo">The target email address</param>
        /// <param name="subject">The subject of the email to send</param>
        /// <param name="emailType">The email type to send</param>
        /// <param name="replacements">The values to replace the place holders in the email templates</param>
        public static void SendEmail(string sendTo, string subject, EmailType emailType, IDictionary replacements)
        {
            var msg = GetMailMessage(sendTo, subject, emailType, replacements).Result;
            using (var smtpClient = new SmtpClient(EmailServer))
            {
                smtpClient.Send(msg);
            }
        }

        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="sendTo">The target email address</param>
        /// <param name="subject">The subject of the email to send</param>
        /// <param name="emailType">The email type to send</param>
        /// <param name="replacements">The values to replace the place holders in the email templates</param>
        public static async Task<MailMessage> GenerateMailMessage(string sendTo, string subject, EmailType emailType, IDictionary replacements)
        {
            var msg = await GetMailMessage(sendTo, subject, emailType, replacements); 
            return msg;  
        }

        private static async Task<MailMessage> GetMailMessage(string sendTo, string subject, EmailType emailType, IDictionary replacements)
        {
            // TODO replace with the real email address
            sendTo = "shuhao.wang@watertrax.com";

            //LinkoExchangeEntities entities = new LinkoExchangeEntities();
            //var emailTemplate = entities.EmailTemplates.First(i => i.EmailType == emailType.ToString()).Template;
            var emailTemplate = LinkoExchangeDbContext.EmailTemplates.FirstOrDefaultAsync(i => i.EmailType == emailType.ToString()).Result; 
            
            if(emailTemplate == null || emailTemplate.Template == null) 
                 return null; 

            MailDefinition md = new MailDefinition
            {
                IsBodyHtml = true,
                From = sendTo,
                Subject = subject
            };
            
            MailMessage msg = md.CreateMailMessage(sendTo, replacements, emailTemplate.Template, new System.Web.UI.Control());
            return msg;
        }
    } 
}