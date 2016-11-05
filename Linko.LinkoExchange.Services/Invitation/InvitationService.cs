using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using AutoMapper;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.User;

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
        private readonly ITimeZoneService _timeZones;

        public InvitationService(LinkoExchangeContext dbContext, IMapper mapper, 
            ISettingService settingService, IUserService userService, IRequestCache requestCache,
            IEmailService emailService, IOrganizationService organizationService, IHttpContextService httpContext,
            ITimeZoneService timeZones) 
        {
            _dbContext = dbContext; 
            _mapper = mapper;
            _settingService = settingService;
            _userService = userService;
            _requestCache = requestCache;
            _emailService = emailService;
            _organizationService = organizationService;
            _httpContext = httpContext;
            _timeZones = timeZones;
        }
 
        public InvitationDto GetInvitation(string invitationId)
        {
            var invitation = _dbContext.Invitations.SingleOrDefault(i => i.InvitationId == invitationId);   
            if(invitation == null) return null;

            return _mapper.Map<InvitationDto>(invitation); 
        }

        public IEnumerable<OrganizationDto> GetInvitationrRecipientOrganization(string invitationId)
        {
            throw new System.NotImplementedException();
        }

        public void CreateInvitation_OLD(InvitationDto inviteDto)
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
            var org = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == orgRegProgramId);
            //var authority = _dbContext.OrganizationRegulatoryPrograms
            //    .Single(o => o.OrganizationId == org.RegulatorOrganizationId
            //    && o.RegulatoryProgramId == org.RegulatoryProgramId);

            Core.Domain.Organization authority;
            if (org.RegulatorOrganization != null)
                authority = org.RegulatorOrganization;
            else
                authority = org.Organization;

            var invites = _dbContext.Invitations.Where(i => i.RecipientOrganizationRegulatoryProgramId == orgRegProgramId);
            if (invites != null)
            {
                foreach (var invite in invites)
                {
                    var dto = _mapper.Map<Core.Domain.Invitation, InvitationDto>(invite);

                    //Get expiry
                    int addExpiryHours = Convert.ToInt32(_settingService.GetOrganizationSettingValue(authority.OrganizationId, SettingType.InvitationExpiredHours));
                    dto.ExpiryDateTimeUtc = dto.InvitationDateTimeUtc.AddHours(addExpiryHours);

                    //Need to modify datetime to local
                    int timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(authority.OrganizationId, SettingType.TimeZone));

                    TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(_timeZones.GetTimeZoneName(timeZoneId));
                    dto.InvitationDateTimeUtc = TimeZoneInfo.ConvertTimeFromUtc(dto.InvitationDateTimeUtc.UtcDateTime, authorityLocalZone);
                    dto.ExpiryDateTimeUtc = TimeZoneInfo.ConvertTimeFromUtc(dto.ExpiryDateTimeUtc.UtcDateTime, authorityLocalZone);

                    dtos.Add(dto);

                }
            }
            return dtos;
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
                    userList.Add(string.Format("{0}|{1}|{2}|{3}|{4}|{5}", user.OrganizationRegulatoryProgramId,
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
            int remaining;
            if (invitationType == InvitationType.AuthorityToIndustry)
                remaining = _organizationService.GetRemainingIndustryLicenseCount(orgRegProgramId);
            else
                remaining = _organizationService.GetRemainingUserLicenseCount(orgRegProgramId, invitationType == InvitationType.AuthorityToAuthority);
            
            if (remaining < 1)
            {
                return new InvitationServiceResultDto()
                {
                    Success = false,
                    ErrorType = Core.Enum.InvitationError.NoMoreRemainingUserLicenses
                };

            }

            var invitationId = Guid.NewGuid().ToString();

            _requestCache.SetValue(CacheKey.Token, invitationId);

            CreateInvitation(new Dto.InvitationDto()
            {
                InvitationId = invitationId,
                InvitationDateTimeUtc = DateTimeOffset.Now,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName,
                RecipientOrganizationRegulatoryProgramId = orgRegProgramId,
                SenderOrganizationRegulatoryProgramId = orgRegProgramId,
            });

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

        public void CreateInvitation(InvitationDto dto)
        {
            var newInvitation = _dbContext.Invitations.Create();
            newInvitation.InvitationId = dto.InvitationId;
            newInvitation.InvitationDateTimeUtc = DateTimeOffset.Now;
            newInvitation.EmailAddress = dto.EmailAddress;
            newInvitation.FirstName = dto.FirstName;
            newInvitation.LastName = dto.LastName;
            newInvitation.RecipientOrganizationRegulatoryProgramId = dto.RecipientOrganizationRegulatoryProgramId;
            newInvitation.SenderOrganizationRegulatoryProgramId = dto.SenderOrganizationRegulatoryProgramId;
            _dbContext.Invitations.Add(newInvitation);
            _dbContext.SaveChanges();

        }

        public void DeleteInvitation(string invitationId)
        {             
            var obj = _dbContext.Invitations.Find(invitationId);  
            _dbContext.Invitations.Remove(obj);
            _dbContext.SaveChanges();
        }
    }
}