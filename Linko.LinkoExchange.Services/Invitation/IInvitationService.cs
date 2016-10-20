using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services
{
    public interface IInvitationService
    {

        /// <summary>
        /// Get the invitation by ID
        /// </summary>
        /// <param name="invitationId">The invitation Id</param>
        /// <returns>The invitation</returns>
        InvitationDto GetInvitation(string invitationId);

        /// <summary>
        /// Get the program of the invitation
        /// </summary>
        /// <param name="invitationId">Invitation id</param>
        /// <returns>The program of the invitation</returns>
        ProgramDto GetInvitationProgram(string invitationId);

        /// <summary>
        /// Get the organization of the invitation
        /// </summary>
        /// <param name="invitationId">Invitation id</param>
        /// <returns>The organization of the invitation</returns>
        IEnumerable<OrganizationDto> GetInvitationrRecipientOrganization(string invitationId);

        /// <summary>
        /// Creates record in Invitation table
        /// </summary>
        /// <param name="invite"></param>
        void CreateInvitation(InvitationDto inviteDto);
    }
}
