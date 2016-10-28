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
using Linko.LinkoExchange.Services.Settings;

namespace Linko.LinkoExchange.Services.Invitation
{
    public class InvitationService : IInvitationService
    {
        private readonly LinkoExchangeContext _dbContext; 
        private readonly IMapper _mapper;
        private readonly ISettingService _settingService;

        public InvitationService(LinkoExchangeContext dbContext, IMapper mapper, ISettingService settingService) 
        {
            _dbContext = dbContext; 
            _mapper = mapper;
            _settingService = settingService;
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
                RegulatoryProgramId = 1000,
                Name = "Mock program name"
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
                    if (!_dbContext.Users.Any(u => u.Email == inviteDto.EmailAddress) &&
                        !_dbContext.Invitations.Any(i => i.EmailAddress == inviteDto.EmailAddress))
                    {
                        var newInvitation = _dbContext.Invitations.Create();
                        newInvitation = _mapper.Map<InvitationDto, Core.Domain.Invitation>(inviteDto);
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
                    throw new RuleViolationException("Validation errors", validationIssues);

                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    throw (ex);
                }
            }

        }

        public ICollection<InvitationDto> GetInvitationsForOrgRegProgram(int orgRegProgramId)
        {
            var dtos = new List<InvitationDto>();
            var invites = _dbContext.Invitations.Where(i => i.RecipientOrganizationRegulatoryProgramId == orgRegProgramId);
            if (invites != null)
            {
                foreach (var invite in invites)
                    dtos.Add(_mapper.Map<Core.Domain.Invitation, InvitationDto>(invite));
            }
            return dtos;
        }

        /// <summary>
        /// Get remaining users for either program or total users across all programs for the entire organization
        /// </summary>
        /// <param name="isForProgramOnly"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetRemainingUserLicenseCount(int orgRegProgramId, bool isForAuthority)
        {
            int maxCount;
            
            if (isForAuthority)
                maxCount = Convert.ToInt32(_settingService.GetOrgRegProgramSettingValue(orgRegProgramId, SettingType.AuthorityUserLicenseTotalCount));
            else
                maxCount = Convert.ToInt32(_settingService.GetOrgRegProgramSettingValue(orgRegProgramId, SettingType.IndustryUserLicenseTotalCount));

            var currentProgramUserCount = _dbContext.OrganizationRegulatoryProgramUsers.Count(u => u.OrganizationRegulatoryProgramId == orgRegProgramId);
            var remaining = maxCount - currentProgramUserCount;

            if (remaining < 0)
                throw new Exception(string.Format("ERROR: Remaining user license count is a negative number (={0}) for Org Reg Program={1}, IsForAuthority={2}", remaining, orgRegProgramId, isForAuthority));

            return remaining;
            
        }

        public void DeleteInvitation(string invitationId)
        {             
            var obj = _dbContext.Invitations.Find(invitationId);  
            _dbContext.Invitations.Remove(obj);
        }
    }
}