using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Core.Domain;
using System;
using Linko.LinkoExchange.Core.Validation;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;

namespace Linko.LinkoExchange.Services
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

        public void CreateInvitation(InvitationDto inviteDto)
        {
            using (var dbContextTransaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead))
            {
                try
                {
                    //Final check for email in use
                    if (!_dbContext.UserProfiles.Any(u => u.Email == inviteDto.EmailAddress) &&
                        !_dbContext.Invitations.Any(i => i.EmailAddress == inviteDto.EmailAddress))
                    {
                        var newInvitation = _dbContext.Invitations.Create();
                        newInvitation = _mapper.Map<InvitationDto, Invitation>(inviteDto);
                        _dbContext.Invitations.Add(newInvitation);
                        _dbContext.SaveChanges();

                        dbContextTransaction.Commit();
                    }
                    else
                        throw new Exception("ERROR: Cannot create invitation due to duplicate email address in UserProfile and/or Invitiation tables.");

                }
                catch (DbEntityValidationException ex)
                {
                    dbContextTransaction.Rollback();

                    List<RuleViolation> validationIssues = new List<RuleViolation>();
                    foreach (DbEntityValidationResult item in ex.EntityValidationErrors)
                    {
                        DbEntityEntry entry = item.Entry;
                        string entityTypeName = entry.Entity.GetType().Name;

                        foreach (DbValidationError subItem in item.ValidationErrors)
                        {
                            string message = string.Format("Error '{0}' occurred in {1} at {2}", subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                            validationIssues.Add(new RuleViolation(string.Empty, null, message));

                        }
                    }
                    //_logger.Info("???");
                    throw new RuleViolationException("Validation errors", validationIssues);

                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    throw (ex);
                }
            }

        }
    }
}