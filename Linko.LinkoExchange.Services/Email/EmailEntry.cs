using System.Collections.Generic;
using System.Net.Mail;
using Linko.LinkoExchange.Core.Enum;
using Newtonsoft.Json;

namespace Linko.LinkoExchange.Services.Email
{
    public class EmailEntry
    {
        #region public properties

        public int? AuditLogId { get; set; }
        public string RecipientEmailAddress { get; set; }
        public EmailType EmailType { get; set; }
        public Dictionary<string, string> ContentReplacements { get; set; }
        public int? RecipientOrgulatoryProgramId { get; set; }
        public int? RecipientOrganizationId { get; set; }
        public int? RecipientRegulatorOrganizationId { get; set; }
        public MailMessage MailMessage { get; set; }
        public string RecipientFirstName { get; set; }
        public string RecipientLastName { get; set; }
        public string RecipientUserName { get; set; }
        public int? RecipientUserProfileId { get; set; }

        #endregion

        public EmailEntry Clone(string overrideEmailAddress = null)
        {
            var temp = JsonConvert.SerializeObject(value:this);
            var obj = JsonConvert.DeserializeObject<EmailEntry>(value:temp);
            if (string.IsNullOrEmpty(value:overrideEmailAddress) == false)
            {
                obj.RecipientEmailAddress = overrideEmailAddress;
            }

            return obj;
        }
    }
}