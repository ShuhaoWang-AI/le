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
using Linko.LinkoExchange.Services.User;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services;

namespace Linko.LinkoExchange.Services.Invitation
{
    public class InvitationService : IInvitationService
    {
        private readonly LinkoExchangeContext _dbContext; 
        private readonly IMapper _mapper;
        private readonly ISettingService _settingService;
        private readonly IUserService _userService;
        private readonly IRequestCache _requestCache;
        private readonly IEmailService _emailService;
        private readonly IOrganizationService _organizationService;
        private readonly IHttpContextService _httpContext;

        public InvitationService(LinkoExchangeContext dbContext, IMapper mapper, 
            ISettingService settingService, IUserService userService, IRequestCache requestCache,
            IEmailService emailService, IOrganizationService organizationService, IHttpContextService httpContext) 
        {
            _dbContext = dbContext; 
            _mapper = mapper;
            _settingService = settingService;
            _userService = userService;
            _requestCache = requestCache;
            _emailService = emailService;
            _organizationService = organizationService;
            _httpContext = httpContext;
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
            {
                //Setting will be at the Authority of this Industry
                var thisIndustry = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
                var authority = _dbContext.OrganizationRegulatoryPrograms
                    .Single(o => o.OrganizationId == thisIndustry.RegulatorOrganizationId &&
                    o.RegulatoryProgramId == thisIndustry.RegulatoryProgramId);
                maxCount = Convert.ToInt32(_settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.UserPerIndustryMaxCount));
            }
            var currentProgramUserCount = _dbContext.OrganizationRegulatoryProgramUsers.Count(u => u.OrganizationRegulatoryProgramId == orgRegProgramId);
            var remaining = maxCount - currentProgramUserCount;

            if (remaining < 0)
                throw new Exception(string.Format("ERROR: Remaining user license count is a negative number (={0}) for Org Reg Program={1}, IsForAuthority={2}", remaining, orgRegProgramId, isForAuthority));

            return remaining;
            
        }

        public int GetRemainingIndustryLicenseCount(int orgRegProgramId)
        {
            var orgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            var currentChildIndustryCount = _dbContext.OrganizationRegulatoryPrograms
                .Count(u => u.RegulatorOrganizationId == orgRegProgram.OrganizationId 
                && u.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId);

            int maxCount = Convert.ToInt32(_settingService.GetOrgRegProgramSettingValue(orgRegProgramId, SettingType.IndustryUserLicenseTotalCount));
            var remaining = maxCount - currentChildIndustryCount;

            if (remaining < 0)
                throw new Exception(string.Format("ERROR: Remaining industry license count is a negative number (={0}) for Org Reg Program={1}", remaining, orgRegProgramId));

            return remaining;

        }


        public InvitationServiceResultDto SendUserInvite(int orgRegProgramId, string email, string firstName, string lastName, InvitationType invitationType)
        {
            //See if any existing users belong to this program
            var existingProgramUsers = _userService.GetProgramUsersByEmail(email);
            if (existingProgramUsers != null && existingProgramUsers.Count() > 0)
            {
                var existingUserForThisProgram = existingProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramId == orgRegProgramId);
                if (existingUserForThisProgram != null)
                {
                    return new InvitationServiceResultDto()
                    {
                        Success = false,
                        ErrorType = Core.Enum.InvitationError.Duplicated,
                        Errors = new string[] { existingUserForThisProgram.UserProfileDto.FirstName, existingUserForThisProgram.UserProfileDto.LastName }
                    };
                }

                //Found match(es) for user not yet part of this program (4.a)
                List<string> userList = new List<string>();
                foreach (var user in existingProgramUsers)
                {
                    userList.Add(string.Format("", user.OrganizationRegulatoryProgramId,
                        user.UserProfileDto.FirstName,
                        user.UserProfileDto.LastName,
                        user.UserProfileDto.PhoneNumber,
                        user.OrganizationRegulatoryProgramDto.RegulatoryProgramDto.Name,
                        user.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName)); //Add additional as needed
                }
                return new InvitationServiceResultDto()
                {
                    Success = false,
                    ErrorType = Core.Enum.InvitationError.MatchingUsersInOtherPrograms,
                    Errors = userList
                };

            }

            //Check available license count
            bool isNoMoreRemaining = false;
            if (invitationType == InvitationType.AuthorityToIndustry)
                isNoMoreRemaining = GetRemainingIndustryLicenseCount(orgRegProgramId) < 1;
            else
                isNoMoreRemaining = GetRemainingUserLicenseCount(orgRegProgramId, invitationType == InvitationType.AuthorityToAuthority) < 1;
            
            if (isNoMoreRemaining)
            {
                return new InvitationServiceResultDto()
                {
                    Success = false,
                    ErrorType = Core.Enum.InvitationError.NoMoreRemainingUserLicenses
                };

            }

            var invitationId = Guid.NewGuid().ToString();

            _requestCache.SetValue(CacheKey.Token, invitationId);

            var newInvitation = _dbContext.Invitations.Create();
            newInvitation.InvitationId = invitationId;
            newInvitation.InvitationDateTimeUtc = DateTimeOffset.Now;
            newInvitation.EmailAddress = email;
            newInvitation.FirstName = firstName;
            newInvitation.LastName = lastName;
            newInvitation.RecipientOrganizationRegulatoryProgramId = orgRegProgramId;
            newInvitation.SenderOrganizationRegulatoryProgramId = orgRegProgramId;
            _dbContext.Invitations.Add(newInvitation);
            _dbContext.SaveChanges();

            //Send invite with link
            var contentReplacements = new Dictionary<string, string>();
            OrganizationRegulatoryProgram authority;
            var thisOrgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            contentReplacements.Add("userName", firstName + " " + lastName);

            if (invitationType == InvitationType.AuthorityToAuthority)
                authority = thisOrgRegProgram;
            else
            {
                //Sending to an Industry therefore need to lookup Authority
                authority = _dbContext.OrganizationRegulatoryPrograms
                    .Single(o => o.OrganizationId == thisOrgRegProgram.RegulatorOrganizationId &&
                    o.RegulatoryProgramId == thisOrgRegProgram.RegulatoryProgramId);
            }

            var authorityName = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName);
            var authorityEmail = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress);
            var authorityPhone = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone);

            contentReplacements.Add("authorityName", authorityName);
            contentReplacements.Add("emailAddress", authorityEmail);
            contentReplacements.Add("phoneNumber", authorityPhone);

            if (invitationType == InvitationType.AuthorityToIndustry
                || invitationType == InvitationType.IndustryToIndustry)
            {
                contentReplacements.Add("organizationName", thisOrgRegProgram.Organization.Name);
                contentReplacements.Add("addressLine1", thisOrgRegProgram.Organization.AddressLine1);
                contentReplacements.Add("cityName", thisOrgRegProgram.Organization.CityName);
                contentReplacements.Add("stateName", _dbContext.Jurisdictions.Single(j => j.JurisdictionId == thisOrgRegProgram.Organization.JurisdictionId).Name);
            }

            string baseUrl = _httpContext.Current().Request.Url.Scheme + "://" + _httpContext.Current().Request.Url.Authority + _httpContext.Current().Request.ApplicationPath.TrimEnd('/') + "/";
            string url = baseUrl + "?token=" + invitationId;
            contentReplacements.Add("link", url);

            _emailService.SendEmail(new[] { email }, EmailType.Registration_AuthorityInviteIndustryUser, contentReplacements);

            return new InvitationServiceResultDto()
            {
                Success = true
            };

        }

        public void DeleteInvitation(string invitationId)
        {             
            var obj = _dbContext.Invitations.Find(invitationId);  
            _dbContext.Invitations.Remove(obj);
        }
    }
}