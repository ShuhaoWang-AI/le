using System;
using System.ComponentModel.DataAnnotations;

namespace Linko.LinkoExchange.Core.Domain
{
    public class Invitation
    {
        [Key]
        public int InvitationId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public DateTime InvitationDateTimeUtc { get; set; }
        public int SenderOrganizationRegulatoryProgramId { get; set; }
        public int RecipientOrganizationRegulatoryProgramId { get; set; } 
    } 
}
