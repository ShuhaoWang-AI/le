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
        /// <param name="isSystemAction">True when system is automatically deleting an Invitation after Registration occurs</param>
        void DeleteInvitation(string invitationId, bool isSystemAction = false);

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

        /// <summary>
        /// Adds a new entry to the tInvitation table. No validation is performed.
        /// </summary>
        /// <param name="dto"></param>
        void CreateInvitation(InvitationDto dto);

        /// <summary>
        /// Sends one of three types of invitation emails:
        ///     1) Inviting the initial authority user
        ///     2) An authority user inviting an industry user
        ///     3) An industry user inviting another industry user
        ///     
        /// Various validation items are performed and logging occurs upon successful execution.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <param name="email"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="invitationType"></param>
        /// <param name="existingOrgRegProgramUserId"></param>
        /// <param name="isAuthorizationRequired"></param>
        /// <returns></returns>
        InvitationServiceResultDto SendUserInvite(int orgRegProgramId, string email, string firstName, string lastName, InvitationType invitationType, int? existingOrgRegProgramUserId = null, bool isAuthorizationRequired = false);

        /// <summary>
        /// This method is used to determine if an email address is 
        ///     1) already associated with the specified org reg program
        ///     2) associated with different org reg program(s)
        ///     3) not associated with any users in the system.
        /// 
        /// Finds all org reg program users associated with the passed in email address.
        /// If such user(s) exists, the list is iterated through to see if one of the users belongs to the
        /// passed in org reg program or other org reg programs. All found org reg program users are returned
        /// in the dto object.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        InvitationCheckEmailResultDto CheckEmailAddress(int orgRegProgramId, string email);

    }
}
