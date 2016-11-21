using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Core.Enum;

namespace Linko.LinkoExchange.Services.Invitation
{
    public interface IInvitationService
    {
        /// <summary>
        /// Delete an invitation from db.
        /// </summary>
        /// <param name="invitation">The invitation id to be deleted.</param>
        void DeleteInvitation(string invitationId);

        /// <summary>
        /// Get the invitation by ID
        /// </summary>
        /// <param name="invitationId">The invitation Id</param>
        /// <returns>The invitation</returns>
        InvitationDto GetInvitation(string invitationId);

        /// <summary>
        /// Get the organization of the invitation
        /// </summary>
        /// <param name="invitationId">Invitation id</param>
        /// <returns>The organization of the invitation</returns>
        IEnumerable<OrganizationDto> GetInvitationrRecipientOrganization(string invitationId);

        /// <summary>
        /// Returns all records in the Invitation table 
        /// where recipient program id matches
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <returns></returns>
        ICollection<InvitationDto> GetInvitationsForOrgRegProgram(int orgRegProgramId);

        void CreateInvitation(InvitationDto dto);

        InvitationServiceResultDto SendUserInvite(int orgRegProgramId, string email, string firstName, string lastName, InvitationType invitationType);

        InvitationCheckEmailResultDto CheckEmailAddress(int orgRegProgramId, string email);

    }
}
