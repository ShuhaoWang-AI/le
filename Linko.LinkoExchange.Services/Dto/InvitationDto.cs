using System;

namespace Linko.LinkoExchange.Services.Dto
{
    public class InvitationDto
    {
        #region public properties

        public string InvitationId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public DateTimeOffset InvitationDateTimeUtc { get; set; }
        public DateTimeOffset ExpiryDateTimeUtc { get; set; }
        public int SenderOrganizationRegulatoryProgramId { get; set; }
        public int RecipientOrganizationRegulatoryProgramId { get; set; }

        public string ProgramName { get; set; }
        public string AuthorityName { get; set; }
        public string IndustryName { get; set; }
        public bool IsResetInvitation { get; set; }

        #endregion
    }
}