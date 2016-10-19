using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Invitation
{
    public class InvitationService : IInvitationService
    {
        private readonly LinkoExchangeContext _dbContext; 
        private readonly IMapper _mapper;

        public InvitationService(LinkoExchangeContext dbContext, IMapper mapper) 
        {
            _dbContext = dbContext; 
            _mapper = mapper;
        }
 

        public InvitationDto GetInvitation(string invitationId)
        {
            var invitation = _dbContext.Invitations.SingleOrDefault(i => i.InvitationId == invitationId);   
            if(invitation == null) return null;

            return _mapper.Map<InvitationDto>(invitation); 
        }

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

        public IEnumerable<OrganizationDto> GetInvitationrRecipientOrganization(string invitationId)
        {
            throw new System.NotImplementedException();
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