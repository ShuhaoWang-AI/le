using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
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
using NLog;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.AuditLog;

namespace Linko.LinkoExchange.Services.Invitation
{
    public class InvitationService : IInvitationService
    {
        private readonly LinkoExchangeContext _dbContext; 
        private readonly ISettingService _settingService;
        private readonly IUserService _userService;
        private readonly IRequestCache _requestCache;
        private readonly IEmailService _emailService;
        private readonly IOrganizationService _organizationService;
        private readonly IHttpContextService _httpContext;
        private readonly ITimeZoneService _timeZones;
        private readonly IProgramService _programService;
        private readonly ILogger _logger;
        private readonly ISessionCache _sessionCache;
        private readonly IMapHelper _mapHelper;
        private readonly ICromerrAuditLogService _crommerAuditLogService;

        public InvitationService(LinkoExchangeContext dbContext,
            ISettingService settingService, IUserService userService, IRequestCache requestCache,
            IEmailService emailService, IOrganizationService organizationService, IHttpContextService httpContext,
            ITimeZoneService timeZones, ILogger logger,
            IProgramService programService, ISessionCache sessionCache, IMapHelper mapHelper,
            ICromerrAuditLogService crommerAuditLogService) 
        {
            _dbContext = dbContext; 
            _settingService = settingService;
            _userService = userService;
            _requestCache = requestCache;
            _emailService = emailService;
            _organizationService = organizationService;
            _httpContext = httpContext;
            _timeZones = timeZones;
            _programService = programService;
            _logger = logger;
            _sessionCache = sessionCache;
            _mapHelper = mapHelper;
            _crommerAuditLogService = crommerAuditLogService;
        }

        public InvitationDto GetInvitation(string invitationId)
        {
            var invitation = _dbContext.Invitations.SingleOrDefault(i => i.InvitationId == invitationId);
            if (invitation == null)
            {
                return null;
            }

            var invitationDto = _mapHelper.GetInvitationDtoFromInvitation(invitation);

            var senderProgram = _programService.GetOrganizationRegulatoryProgram(invitation.SenderOrganizationRegulatoryProgramId);
            var recipientProgram = _programService.GetOrganizationRegulatoryProgram(invitation.RecipientOrganizationRegulatoryProgramId);

            if (senderProgram == null || recipientProgram == null)
            {
                throw new Exception("Invalid invitation data");
            }

            //If Recipient Org Reg Program is disabled, treat same as expired invitation (Bug 1865)
            if (!recipientProgram.IsEnabled)
            {
                return null;
            } 

            // Industry Invite Industry
            if (senderProgram.RegulatorOrganization != null)
            {
                invitationDto.AuthorityName = senderProgram.RegulatorOrganization.OrganizationName;
                if (senderProgram.OrganizationDto != null)
                {
                    invitationDto.IndustryName = senderProgram.OrganizationDto.OrganizationName;
                }
            } 

            if (recipientProgram != null && !recipientProgram.RegulatorOrganizationId.HasValue
                && recipientProgram.OrganizationDto != null)
            {
                // recipient is authority user 
                invitationDto.AuthorityName = recipientProgram.OrganizationDto.CityName;
            }

            if (recipientProgram != null && recipientProgram.RegulatoryProgramDto != null)
            {
                invitationDto.ProgramName = recipientProgram.RegulatoryProgramDto.Name;
            }

            return invitationDto;
        }

        public IEnumerable<OrganizationDto> GetInvitationrRecipientOrganization(string invitationId)
        {
            throw new System.NotImplementedException();
        }

        public ICollection<InvitationDto> GetInvitationsForOrgRegProgram(int senderOrgRegProgramId, int targetOrgRegProgramId)
        {
            var dtos = new List<InvitationDto>();
            var org = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == senderOrgRegProgramId);
            //var authority = _dbContext.OrganizationRegulatoryPrograms
            //    .Single(o => o.OrganizationId == org.RegulatorOrganizationId
            //    && o.RegulatoryProgramId == org.RegulatoryProgramId);

            Core.Domain.Organization authority;
            if (org.RegulatorOrganization != null)
                authority = org.RegulatorOrganization;
            else
                authority = org.Organization;

            var invites = _dbContext.Invitations
                .Where(i => i.SenderOrganizationRegulatoryProgramId == senderOrgRegProgramId
                && i.RecipientOrganizationRegulatoryProgramId == targetOrgRegProgramId);

            if (invites != null)
            {
                foreach (var invite in invites)
                {
                    var dto = _mapHelper.GetInvitationDtoFromInvitation(invite);

                    //Get expiry
                    int addExpiryHours = Convert.ToInt32(_settingService.GetOrganizationSettingValue(authority.OrganizationId, org.RegulatoryProgramId, SettingType.InvitationExpiredHours));

                    //Need to modify datetime to local
                    dto.InvitationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(
                        dto.InvitationDateTimeUtc.DateTime, authority.OrganizationId, org.RegulatoryProgramId);
                    dto.ExpiryDateTimeUtc = dto.InvitationDateTimeUtc.AddHours(addExpiryHours);

                    dtos.Add(dto);

                }
            }
            return dtos;
        }

        public InvitationServiceResultDto SendUserInvite(int targetOrgRegProgramId, string email, string firstName, string lastName, InvitationType invitationType, int? existingOrgRegProgramUserId = null)
        {
            string existingUserUserName = "[unknown]";
            if (existingOrgRegProgramUserId.HasValue) //Existing user in a different program -- lookup required invitation fields
            {
                var existingUser = _userService.GetOrganizationRegulatoryProgramUser(existingOrgRegProgramUserId.Value);
                email = existingUser.UserProfileDto.Email;
                firstName = existingUser.UserProfileDto.FirstName;
                lastName = existingUser.UserProfileDto.LastName;
                existingUserUserName = existingUser.UserProfileDto.UserName;
            }
            else
            {
                //See if any existing users belong to this program
                var existingProgramUsers = _userService.GetProgramUsersByEmail(email);
                if (existingProgramUsers != null && existingProgramUsers.Count() > 0)
                {
                    var existingUserForThisProgram = existingProgramUsers
                        .SingleOrDefault(u => u.OrganizationRegulatoryProgramId == targetOrgRegProgramId 
                        && u.UserProfileDto.IsAccountResetRequired == false
                        && u.IsRegistrationDenied != true
                        && u.IsRemoved != true);

                    if (existingUserForThisProgram != null)
                    {
                        _logger.Info(string.Format("SendInvitation Failed. User with email={0} already exists within OrgRegProgramId={0}", email, targetOrgRegProgramId));
                        return new InvitationServiceResultDto()
                        {
                            Success = false,
                            ErrorType = InvitationError.Duplicated,
                            Errors = new string[] { existingUserForThisProgram.UserProfileDto.FirstName, existingUserForThisProgram.UserProfileDto.LastName }
                        };
                    }
                }
            }

            //For Authority inviting Industry, check if an active 
            //Admin user does not already exist within that IU
            if (invitationType == InvitationType.AuthorityToIndustry)
            {
                var orgRegProgram = _organizationService.GetOrganizationRegulatoryProgram(targetOrgRegProgramId);
                if (orgRegProgram.HasAdmin) //already
                {
                    _logger.Info(string.Format("SendInvitation Failed. Industry already has Admin User. OrgRegProgramId={0}", targetOrgRegProgramId));
                    string errorMsg = "This organization already has an Administrator.";
                    return new InvitationServiceResultDto()
                    {
                        Success = false,
                        ErrorType = InvitationError.IndustryAlreadyHasAdminUser,
                        Errors = new string[] { errorMsg }
                    };
                }
            }


            //Check available license count
            int remaining = _organizationService.GetRemainingUserLicenseCount(targetOrgRegProgramId);
            
            if (remaining < 1)
            {
                _logger.Info(string.Format("SendInvitation Failed. No more remaining user licenses. OrgRegProgramId={0}, InviteType={0}", targetOrgRegProgramId, invitationType.ToString()));
                return new InvitationServiceResultDto()
                {
                    Success = false,
                    ErrorType = Core.Enum.InvitationError.NoMoreRemainingUserLicenses,
                    Errors = new string[] { "No more User Licenses are available for this organization.  Disable another User and try again." }
                };
            }

            var invitationId = Guid.NewGuid().ToString();

            _requestCache.SetValue(CacheKey.Token, invitationId);

            int senderOrgRegProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));

            CreateInvitation(new Dto.InvitationDto()
            {
                InvitationId = invitationId,
                InvitationDateTimeUtc = DateTime.UtcNow,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName,
                RecipientOrganizationRegulatoryProgramId = targetOrgRegProgramId, 
                SenderOrganizationRegulatoryProgramId = senderOrgRegProgramId
            });

            //Send invite with link
            var contentReplacements = new Dictionary<string, string>();
            OrganizationRegulatoryProgram authority;
            var targetOrgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == targetOrgRegProgramId);
            var senderOrgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == senderOrgRegProgramId);
            contentReplacements.Add("firstName", firstName);
            contentReplacements.Add("lastName", lastName);

            if (invitationType != InvitationType.IndustryToIndustry)    //Sender is the authority
                authority = senderOrgRegProgram;
            else
            {
                //Industry to industry (sender and target are the same) -- lookup the authority
                authority = _dbContext.OrganizationRegulatoryPrograms
                    .Single(o => o.OrganizationId == targetOrgRegProgram.RegulatorOrganizationId &&
                    o.RegulatoryProgramId == targetOrgRegProgram.RegulatoryProgramId);
            }

            var authorityName = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName);
            var authorityEmail = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress);
            var authorityPhone = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone);

            contentReplacements.Add("authorityName", authorityName);
            contentReplacements.Add("authorityOrganizationName", authority.Organization.Name);
            contentReplacements.Add("emailAddress", authorityEmail);
            contentReplacements.Add("phoneNumber", authorityPhone);
            contentReplacements.Add("supportEmail", authorityEmail);
            contentReplacements.Add("supportPhoneNumber", authorityPhone);

            if (invitationType == InvitationType.AuthorityToIndustry
                || invitationType == InvitationType.IndustryToIndustry)
            {
                contentReplacements.Add("organizationName", targetOrgRegProgram.Organization.Name);
                contentReplacements.Add("addressLine1", targetOrgRegProgram.Organization.AddressLine1);
                contentReplacements.Add("cityName", targetOrgRegProgram.Organization.CityName);
                contentReplacements.Add("stateName", _dbContext.Jurisdictions.Single(j => j.JurisdictionId == targetOrgRegProgram.Organization.JurisdictionId).Code);
            }

            string baseUrl = _httpContext.GetRequestBaseUrl();
            string url = baseUrl + "Account/Register?token=" + invitationId;
            contentReplacements.Add("link", url);

            EmailType emailType;
            switch (invitationType)
            {
                case InvitationType.AuthorityToAuthority:
                    emailType = EmailType.Registration_InviteAuthorityUser;
                    break;
                case InvitationType.AuthorityToIndustry:
                    emailType = EmailType.Registration_AuthorityInviteIndustryUser;
                    break;
                case InvitationType.IndustryToIndustry:
                    emailType = EmailType.Registration_IndustryInviteIndustryUser;
                    break;
                default:

                    throw new Exception("ERROR: unknown EmailType");
            }

            _emailService.SendEmail(new[] { email }, emailType, contentReplacements);

            int thisUserOrgRegProgUserId = Convert.ToInt32(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(actorProgramUser);
            var actorUser = actorProgramUserDto.UserProfileDto;

            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
            cromerrAuditLogEntryDto.RegulatoryProgramId = actorProgramUser.OrganizationRegulatoryProgram.RegulatoryProgramId;
            cromerrAuditLogEntryDto.OrganizationId = actorProgramUser.OrganizationRegulatoryProgram.OrganizationId;
            cromerrAuditLogEntryDto.RegulatorOrganizationId = actorProgramUser.OrganizationRegulatoryProgram.RegulatorOrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = actorProgramUser.UserProfileId;
            cromerrAuditLogEntryDto.UserName = actorUser.UserName;
            cromerrAuditLogEntryDto.UserFirstName = actorUser.FirstName;
            cromerrAuditLogEntryDto.UserLastName = actorUser.LastName;
            cromerrAuditLogEntryDto.UserEmailAddress = actorUser.Email;
            cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
            contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("authorityName", authorityName);
            contentReplacements.Add("businessName", authority.Organization.Name);
            contentReplacements.Add("regulatoryProgram", authority.RegulatoryProgram.Name);
            contentReplacements.Add("firstName", firstName);
            contentReplacements.Add("lastName", lastName);
            contentReplacements.Add("userName", existingUserUserName);
            contentReplacements.Add("emailAddress", email);
            contentReplacements.Add("inviterFirstName", actorUser.FirstName);
            contentReplacements.Add("inviterLastName", actorUser.LastName);
            contentReplacements.Add("inviterUserName", actorUser.UserName);
            contentReplacements.Add("inviterEmailAddress", actorUser.Email);

            _crommerAuditLogService.Log(CromerrEvent.Registration_InviteSent, cromerrAuditLogEntryDto, contentReplacements);

            return new InvitationServiceResultDto()
            {
                Success = true
            };

        }

        public void CreateInvitation(InvitationDto dto)
        {
            try
            {
                var newInvitation = _dbContext.Invitations.Create();
                newInvitation.InvitationId = dto.InvitationId;
                newInvitation.InvitationDateTimeUtc = DateTimeOffset.UtcNow;
                newInvitation.EmailAddress = dto.EmailAddress;
                newInvitation.FirstName = dto.FirstName;
                newInvitation.LastName = dto.LastName;
                newInvitation.RecipientOrganizationRegulatoryProgramId = dto.RecipientOrganizationRegulatoryProgramId;
                newInvitation.SenderOrganizationRegulatoryProgramId = dto.SenderOrganizationRegulatoryProgramId;
                _dbContext.Invitations.Add(newInvitation);
                _dbContext.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
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
                _logger.Info(string.Format("CreateInvitation. DbEntityValidationException. Email={0}", dto.EmailAddress));
                throw new RuleViolationException("Validation errors", validationIssues);

            }
            catch (Exception ex)
            {
                _logger.Info(string.Format("CreateInvitation. Error creating invitation. Email={0}", dto.EmailAddress));
                throw (ex);
            }

        }

        public void DeleteInvitation(string invitationId)
        {             
            var obj = _dbContext.Invitations.Find(invitationId);  
            _dbContext.Invitations.Remove(obj);
            _dbContext.SaveChanges();
        }

        public InvitationCheckEmailResultDto CheckEmailAddress(int orgRegProgramId, string emailAddress)
        {
            var existingUsers = _userService.GetProgramUsersByEmail(emailAddress);
            var existingUsersDifferentPrograms = new List<OrganizationRegulatoryProgramUserDto>();

            if (existingUsers != null && existingUsers.Count() > 0)
            {
                foreach (var existingUser in existingUsers)
                {
                    if (existingUser.OrganizationRegulatoryProgramId == orgRegProgramId 
                        && existingUser.UserProfileDto.IsAccountResetRequired == false
                        && existingUser.IsRegistrationDenied != true
                        && existingUser.IsRemoved != true)
                    {
                        return new InvitationCheckEmailResultDto()
                        {
                            ExistingUsersDifferentPrograms = null,
                            ExistingUserSameProgram = existingUser
                        };
                    }
                    else
                    {
                        existingUsersDifferentPrograms.Add(existingUser);
                    }

                }

                //Found match(es) for user not yet part of this program (4.a)
                return new InvitationCheckEmailResultDto()
                {
                    ExistingUserSameProgram = null,
                    ExistingUsersDifferentPrograms = existingUsersDifferentPrograms
                };

            }

            return new InvitationCheckEmailResultDto()
            {
                ExistingUsersDifferentPrograms = null,
                ExistingUserSameProgram = null
            };

        }
    }
}