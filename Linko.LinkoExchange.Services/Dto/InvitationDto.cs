using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class InvitationDto
    {
        public string InvitationId { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public DateTime InvitationDateTimeUtc { get; set; }
        public int SenderOrganizationRegulatoryProgramId { get; set; }
        public int RecipientOrganizationRegulatoryProgramId { get; set; }
    }
}
