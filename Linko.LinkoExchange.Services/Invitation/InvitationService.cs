using System.Collections.Generic;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Invitation
{
    public class InvitationService : IInvitationService
    {
        /// <summary>
        /// Get the program of the invitation
        /// </summary>
        /// <param name="invitationId">Invitation id</param>
        /// <returns>The program of the invitation</returns>
        public ProgramDto GetInvitationProgram(string invitationId)
        {
            // TODO
            return new ProgramDto
            {
                ProgramId = 1000,
                ProgramName = "Mock program name"
            };
        }

        /// <summary>
        /// Get the organization of the invitation
        /// </summary>
        /// <param name="invitationId">Invitation id</param>
        /// <returns>The organization of the invitation</returns>
        public IEnumerable<OrganizationDto> GetInvitationrOrganizations(string invitationId)
        {
            // TODO
            var list = new List<OrganizationDto>
            {
                new OrganizationDto
                {
                    OrganizationId = 1000,
                    OrganizationName = "Mock organization name"
                }
            };

            return list;
        }
    }
}