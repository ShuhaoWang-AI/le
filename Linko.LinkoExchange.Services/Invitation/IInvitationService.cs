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
        /// /// <param name="registrationActorOrgRegProgUserId">When we are deleting an invitation after a user Registers</param>
        void DeleteInvitation(string invitationId, int? registrationActorOrgRegProgUserId = null);

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
        /// <param name="senderOrgRegProgramId">The Org Reg Program that sent the invitation</param>
        /// <param name="targetOrgRegProgramId">The Org Reg Program that the user was invited into (aka "recipient")</param>
        /// <returns></returns>
        ICollection<InvitationDto> GetInvitationsForOrgRegProgram(int senderOrgRegProgramId, int targetOrgRegProgramId);

        void CreateInvitation(InvitationDto dto);

        InvitationServiceResultDto SendUserInvite(int orgRegProgramId, string email, string firstName, string lastName, InvitationType invitationType, int? existingOrgRegProgramUserId = null);

        InvitationCheckEmailResultDto CheckEmailAddress(int orgRegProgramId, string email);

    }
}
