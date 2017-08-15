using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Jurisdiction;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Program;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Linko.LinkoExchange.Services.User;
using NLog;

namespace Linko.LinkoExchange.Services.Invitation
{
    public class InvitationService:IInvitationService
    {
        #region fields

        private readonly ICromerrAuditLogService _crommerAuditLogService;
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        private readonly ILinkoExchangeEmailService _linkoExchangeEmailService;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _organizationService;
        private readonly IProgramService _programService;
        private readonly IRequestCache _requestCache;
        private readonly ISettingService _settingService;
        private readonly ITimeZoneService _timeZones;
        private readonly IUserService _userService;
        private readonly IJurisdictionService _jurisdictionService;
        #endregion

        #region constructors and destructor

        public InvitationService(LinkoExchangeContext dbContext,
                                 ISettingService settingService, IUserService userService, IRequestCache requestCache,
                                 IOrganizationService organizationService, IHttpContextService httpContext,
                                 ITimeZoneService timeZones, ILogger logger,
                                 IProgramService programService, IMapHelper mapHelper,
                                 ICromerrAuditLogService crommerAuditLogService,
                                 ILinkoExchangeEmailService linkoExchangeEmailService,
                                 IJurisdictionService jurisdictionService
        )
        {
            _dbContext = dbContext;
            _settingService = settingService;
            _userService = userService;
            _requestCache = requestCache;
            _organizationService = organizationService;
            _httpContextService = httpContext;
            _timeZones = timeZones;
            _programService = programService;
            _logger = logger;
            _mapHelper = mapHelper;
            _crommerAuditLogService = crommerAuditLogService;
            _linkoExchangeEmailService = linkoExchangeEmailService;
            _jurisdictionService = jurisdictionService;
        }

        #endregion

        #region interface implementations

        public InvitationDto GetInvitation(string invitationId)
        {
            var invitation = _dbContext.Invitations.SingleOrDefault(i => i.InvitationId == invitationId);
            if (invitation == null)
            {
                //Cannot find invitation using token provided
                _logger.Info(message:$"GetInvitation. Cannot find invitation using token (='{invitationId}') provided.");
                return null;
            }

            var invitationDto = _mapHelper.GetInvitationDtoFromInvitation(invitation:invitation);
            var senderProgram =
                _programService.GetOrganizationRegulatoryProgram(organizationRegulatoryProgramId:invitation.SenderOrganizationRegulatoryProgramId);
            var recipientProgram =
                _programService.GetOrganizationRegulatoryProgram(organizationRegulatoryProgramId:invitation.RecipientOrganizationRegulatoryProgramId);

            if (senderProgram == null || recipientProgram == null)
            {
                throw new Exception(message:"Invalid invitation data");
            }

            //Check expiration here
            var addExpiryHours =
                Convert.ToInt32(value:_settingService.GetOrganizationSettingValue(orgRegProgramId:invitation.SenderOrganizationRegulatoryProgramId,
                                                                                  settingType:SettingType.InvitationExpiredHours));
            var expiryDateTimeUtc = invitation.InvitationDateTimeUtc.AddHours(hours:addExpiryHours);
            if (DateTime.UtcNow > expiryDateTimeUtc)
            {
                //expired invitation

                //check if this is a reset request or invitation
                var user = _userService.GetUserProfileByEmail(emailAddress:invitation.EmailAddress);
                if (user != null && user.IsAccountResetRequired)
                {
                    //Reset/Re-reg scenario
                    var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
                    cromerrAuditLogEntryDto.RegulatoryProgramId = recipientProgram.RegulatoryProgramId;
                    cromerrAuditLogEntryDto.OrganizationId = recipientProgram.OrganizationId;
                    cromerrAuditLogEntryDto.RegulatorOrganizationId = recipientProgram.RegulatorOrganizationId ??
                                                                      cromerrAuditLogEntryDto.OrganizationId;
                    cromerrAuditLogEntryDto.UserProfileId = user.UserProfileId;
                    cromerrAuditLogEntryDto.UserName = user.UserName;
                    cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
                    cromerrAuditLogEntryDto.UserLastName = user.LastName;
                    cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
                    cromerrAuditLogEntryDto.IPAddress = _httpContextService.CurrentUserIPAddress();
                    cromerrAuditLogEntryDto.HostName = _httpContextService.CurrentUserHostName();
                    var contentReplacements = new Dictionary<string, string>();
                    contentReplacements.Add(key:"firstName", value:user.FirstName);
                    contentReplacements.Add(key:"lastName", value:user.LastName);
                    contentReplacements.Add(key:"userName", value:user.UserName);
                    contentReplacements.Add(key:"emailAddress", value:user.Email);

                    _crommerAuditLogService.Log(eventType:CromerrEvent.UserAccess_AccountResetExpired, dto:cromerrAuditLogEntryDto,
                                                contentReplacements:contentReplacements);
                }
                else
                {
                    //Invitation scenario
                }

                _logger.Info(message:$"GetInvitation. Expired invitation. Token ='{invitationId}'");
                return null;
            }

            //If Recipient Org Reg Program is disabled, treat same as expired invitation (Bug 1865)
            if (!recipientProgram.IsEnabled)
            {
                return null;
            }

            if (recipientProgram.OrganizationDto != null)
            {
                if (recipientProgram.RegulatorOrganizationId.HasValue)
                {
                    // recipient is IndustryName user
                    invitationDto.IndustryName = recipientProgram.OrganizationDto.OrganizationName;
                    invitationDto.AuthorityName = recipientProgram.RegulatorOrganization.OrganizationName; // get authority
                }
                else
                {
                    // recipient is authority user 
                    invitationDto.AuthorityName = recipientProgram.OrganizationDto.OrganizationName;
                }
            }

            if (recipientProgram.RegulatoryProgramDto != null)
            {
                invitationDto.ProgramName = recipientProgram.RegulatoryProgramDto.Description;
            }

            return invitationDto;
        }

        public IEnumerable<OrganizationDto> GetInvitationrRecipientOrganization(string invitationId)
        {
            throw new NotImplementedException();
        }

        public ICollection<InvitationDto> GetInvitationsForOrgRegProgram(int senderOrgRegProgramId, int targetOrgRegProgramId)
        {
            var dtos = new List<InvitationDto>();
            var org = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == senderOrgRegProgramId);

            //var authority = _dbContext.OrganizationRegulatoryPrograms
            //    .Single(o => o.OrganizationId == org.RegulatorOrganizationId
            //    && o.RegulatoryProgramId == org.RegulatoryProgramId);

            var authority = org.RegulatorOrganization ?? org.Organization;

            var invites = _dbContext.Invitations
                                    .Where(i => i.SenderOrganizationRegulatoryProgramId == senderOrgRegProgramId
                                                && i.RecipientOrganizationRegulatoryProgramId == targetOrgRegProgramId);

   
                foreach (var invite in invites)
                {
                    var dto = _mapHelper.GetInvitationDtoFromInvitation(invitation:invite);

                    //Get expiry
                    var addExpiryHours =
                        Convert.ToInt32(value:
                                        _settingService.GetOrganizationSettingValue(organizationId:authority.OrganizationId, regProgramId:org.RegulatoryProgramId,
                                                                                    settingType:SettingType.InvitationExpiredHours));

                    //Need to modify date time to local
                    dto.InvitationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(
                                                                                                      utcDateTime:dto.InvitationDateTimeUtc.UtcDateTime,
                                                                                                      orgId:authority.OrganizationId, regProgramId:org.RegulatoryProgramId);
                    dto.ExpiryDateTimeUtc = dto.InvitationDateTimeUtc.AddHours(hours:addExpiryHours);

                    dtos.Add(item:dto);
                }
      

            return dtos;
        }

        public InvitationServiceResultDto SendUserInvite(int targetOrgRegProgramId, string email, string firstName, string lastName, InvitationType invitationType,
                                                         int? existingOrgRegProgramUserId = null, bool isAuthorizationRequired = false)
        {
            _logger.Info(message:$"Enter InvitationService.SendUserInvite. targetOrgRegProgramId={targetOrgRegProgramId}, email={email}, invitationType={invitationType}");

            OrganizationRegulatoryProgramUserDto existingUser = null;
            if (existingOrgRegProgramUserId.HasValue) //Existing user in a different program -- lookup required invitation fields
            {
                existingUser = _userService.GetOrganizationRegulatoryProgramUser(orgRegProgUserId:existingOrgRegProgramUserId.Value);
                email = existingUser.UserProfileDto.Email;
                firstName = existingUser.UserProfileDto.FirstName;
                lastName = existingUser.UserProfileDto.LastName;
            }
            else
            {
                //See if any existing users belong to this program
                var existingProgramUsers = _userService.GetProgramUsersByEmail(emailAddress:email);
                if (existingProgramUsers != null && existingProgramUsers.Any())
                {
                    var existingUserForThisProgram = existingProgramUsers
                        .SingleOrDefault(u => u.OrganizationRegulatoryProgramId == targetOrgRegProgramId
                                              && u.UserProfileDto.IsAccountResetRequired == false
                                              && u.IsRegistrationDenied != true
                                              && u.IsRemoved != true);

                    if (existingUserForThisProgram != null)
                    {
                        _logger.Info(message:$"SendInvitation Failed. User with email={email} already exists within OrgRegProgramId={targetOrgRegProgramId}");
                        return new InvitationServiceResultDto
                               {
                                   Success = false,
                                   ErrorType = InvitationError.Duplicated,
                                   Errors = new[] {existingUserForThisProgram.UserProfileDto.FirstName, existingUserForThisProgram.UserProfileDto.LastName}
                               };
                    }
                }
            }

            //For Authority inviting Industry, check if an active 
            //Admin user does not already exist within that IU
            if (invitationType == InvitationType.AuthorityToIndustry)
            {
                var orgRegProgram = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:targetOrgRegProgramId);
                if (orgRegProgram.HasAdmin) //already
                {
                    _logger.Info(message:$"SendInvitation Failed. Industry already has Admin User. OrgRegProgramId={targetOrgRegProgramId}");
                    var errorMsg = "This organization already has an Administrator.";
                    return new InvitationServiceResultDto
                           {
                               Success = false,
                               ErrorType = InvitationError.IndustryAlreadyHasAdminUser,
                               Errors = new[] {errorMsg}
                           };
                }
            }

            //Check available license count
            var remaining = _organizationService.GetRemainingUserLicenseCount(orgRegProgramId:targetOrgRegProgramId);

            if (remaining < 1)
            {
                _logger.Info(message:$"SendInvitation Failed. No more remaining user licenses. OrgRegProgramId={targetOrgRegProgramId}, InviteType={invitationType}");
                return new InvitationServiceResultDto
                       {
                           Success = false,
                           ErrorType = InvitationError.NoMoreRemainingUserLicenses,
                           Errors = new[] {"No more User Licenses are available for this organization.  Disable another User and try again."}
                       };
            }

            var invitationId = Guid.NewGuid().ToString();

            _requestCache.SetValue(key:CacheKey.Token, value:invitationId);

            var senderOrgRegProgramId = int.Parse(s:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));

            var invitationDto = new InvitationDto
                                {
                                    InvitationId = invitationId,
                                    InvitationDateTimeUtc = DateTime.UtcNow,
                                    EmailAddress = email,
                                    FirstName = firstName,
                                    LastName = lastName,
                                    RecipientOrganizationRegulatoryProgramId = targetOrgRegProgramId,
                                    SenderOrganizationRegulatoryProgramId = senderOrgRegProgramId
                                };

            CreateInvitation(dto:invitationDto);

            // Call SaveChanges to make sure data is good before sending emails.
            _dbContext.SaveChanges();

            //Send invite with link
            var contentReplacements = new Dictionary<string, string>();
            OrganizationRegulatoryProgram authority;
            var targetOrgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == targetOrgRegProgramId);
            var senderOrgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == senderOrgRegProgramId);
            contentReplacements.Add(key:"firstName", value:firstName);
            contentReplacements.Add(key:"lastName", value:lastName);

            if (invitationType != InvitationType.IndustryToIndustry) //Sender is the authority
            {
                authority = senderOrgRegProgram;
            }
            else
            {
                //Industry to industry (sender and target are the same) -- lookup the authority
                authority = _dbContext.OrganizationRegulatoryPrograms
                                      .Single(o => o.OrganizationId == targetOrgRegProgram.RegulatorOrganizationId &&
                                                   o.RegulatoryProgramId == targetOrgRegProgram.RegulatoryProgramId);
            }

            var authorityName = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId, settingType:SettingType.EmailContactInfoName);
            var authorityEmail = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                              settingType:SettingType.EmailContactInfoEmailAddress);
            var authorityPhone = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                              settingType:SettingType.EmailContactInfoPhone);

            contentReplacements.Add(key:"authorityName", value:authorityName);
            contentReplacements.Add(key:"authorityOrganizationName", value:authority.Organization.Name);
            contentReplacements.Add(key:"emailAddress", value:authorityEmail);
            contentReplacements.Add(key:"phoneNumber", value:authorityPhone);
            contentReplacements.Add(key:"supportEmail", value:authorityEmail);
            contentReplacements.Add(key:"supportPhoneNumber", value:authorityPhone);

            if (invitationType == InvitationType.AuthorityToIndustry
                || invitationType == InvitationType.IndustryToIndustry)
            {
                contentReplacements.Add(key:"organizationName", value:targetOrgRegProgram.Organization.Name);
                contentReplacements.Add(key:"addressLine1", value:targetOrgRegProgram.Organization.AddressLine1);
                contentReplacements.Add(key:"cityName", value:targetOrgRegProgram.Organization.CityName); 
                contentReplacements.Add(key:"stateName", value: _jurisdictionService.GetJurisdictionById(targetOrgRegProgram.Organization.JurisdictionId)?.Code ?? "");
            }

            var baseUrl = _httpContextService.GetRequestBaseUrl();
            var url = baseUrl + "Account/Register?token=" + invitationId;
            contentReplacements.Add(key:"link", value:url);

            var thisUserOrgRegProgUserId = Convert.ToInt32(value:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                             .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
            var actorUser = _userService.GetUserProfileById(userProfileId:actorProgramUser.UserProfileId);

            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
            var cromerrContentReplacements = new Dictionary<string, string>();
            if (existingUser != null)
            {
                cromerrAuditLogEntryDto.UserProfileId = existingUser.UserProfileId;
                cromerrAuditLogEntryDto.UserName = existingUser.UserProfileDto.UserName;
                cromerrContentReplacements.Add(key:"userName", value:existingUser.UserProfileDto.UserName);
            }
            else
            {
                cromerrAuditLogEntryDto.UserName = "n/a";
                cromerrContentReplacements.Add(key:"userName", value:"n/a");
            }

            // Per discussion between Sundoro, Rajeeb, Shuhao, during re-invitation,
            // tOrganizationRegulatoryProgramUser.IsRemoved should be set to false
            if (existingUser != null)
            {
                var user = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId == existingUser.OrganizationRegulatoryProgramUserId);
                user.IsRemoved = false;
                _dbContext.SaveChanges();
            }

            cromerrAuditLogEntryDto.RegulatoryProgramId = targetOrgRegProgram.RegulatoryProgramId;
            cromerrAuditLogEntryDto.OrganizationId = targetOrgRegProgram.OrganizationId;
            cromerrAuditLogEntryDto.RegulatorOrganizationId = targetOrgRegProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
            cromerrAuditLogEntryDto.UserFirstName = firstName;
            cromerrAuditLogEntryDto.UserLastName = lastName;
            cromerrAuditLogEntryDto.UserEmailAddress = email;
            cromerrAuditLogEntryDto.IPAddress = _httpContextService.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContextService.CurrentUserHostName();
            cromerrContentReplacements.Add(key:"authorityName", value:authority.Organization.Name);
            cromerrContentReplacements.Add(key:"organizationName", value:targetOrgRegProgram.Organization.Name);
            cromerrContentReplacements.Add(key:"regulatoryProgram", value:authority.RegulatoryProgram.Name);
            cromerrContentReplacements.Add(key:"firstName", value:firstName);
            cromerrContentReplacements.Add(key:"lastName", value:lastName);
            cromerrContentReplacements.Add(key:"emailAddress", value:email);
            cromerrContentReplacements.Add(key:"inviterFirstName", value:actorUser.FirstName);
            cromerrContentReplacements.Add(key:"inviterLastName", value:actorUser.LastName);
            cromerrContentReplacements.Add(key:"inviterUserName", value:actorUser.UserName);
            cromerrContentReplacements.Add(key:"inviterEmailAddress", value:actorUser.Email);

            _crommerAuditLogService.Log(eventType:CromerrEvent.Registration_InviteSent, dto:cromerrAuditLogEntryDto, contentReplacements:cromerrContentReplacements).Wait();

            //Invitation email logs only for the program that is invited to   

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

                    throw new Exception(message:"ERROR: unknown EmailType");
            }

            if (existingUser == null)
            {
                existingUser = new OrganizationRegulatoryProgramUserDto
                               {
                                   UserProfileDto = new UserDto
                                                    {
                                                        Email = email,
                                                        FirstName = firstName,
                                                        LastName = lastName
                                                    }
                               };
            }

            var emailEntry = _linkoExchangeEmailService.GetEmailEntryForOrgRegProgramUser(user:existingUser, emailType:emailType, contentReplacements:contentReplacements);
            emailEntry.RecipientOrgulatoryProgramId = targetOrgRegProgram.RegulatoryProgramId;
            emailEntry.RecipientOrganizationId = targetOrgRegProgram.OrganizationId;
            emailEntry.RecipientRegulatorOrganizationId = cromerrAuditLogEntryDto.RegulatorOrganizationId;

            _linkoExchangeEmailService.SendEmails(emailEntries:new List<EmailEntry> {emailEntry});

            _logger.Info(message:$"Leaving InvitationService.SendUserInvite. targetOrgRegProgramId={targetOrgRegProgramId}, email={email}, invitationType={invitationType}");

            return new InvitationServiceResultDto
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
                newInvitation.InvitationDateTimeUtc = DateTimeOffset.Now;
                newInvitation.EmailAddress = dto.EmailAddress;
                newInvitation.FirstName = dto.FirstName;
                newInvitation.LastName = dto.LastName;
                newInvitation.RecipientOrganizationRegulatoryProgramId = dto.RecipientOrganizationRegulatoryProgramId;
                newInvitation.SenderOrganizationRegulatoryProgramId = dto.SenderOrganizationRegulatoryProgramId;
                _dbContext.Invitations.Add(entity:newInvitation);
                _dbContext.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var validationIssues = new List<RuleViolation>();
                foreach (var item in ex.EntityValidationErrors)
                {
                    var entry = item.Entry;
                    var entityTypeName = entry.Entity.GetType().Name;

                    foreach (var subItem in item.ValidationErrors)
                    {
                        string message = $"Error '{subItem.ErrorMessage}' occurred in {entityTypeName} at {subItem.PropertyName}";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                    }
                }

                _logger.Info(message:$"CreateInvitation. DbEntityValidationException. Email={dto.EmailAddress}");
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }
            catch
            {
                _logger.Info(message:$"CreateInvitation. Error creating invitation. Email={dto.EmailAddress}");
                throw;
            }
        }

        public void DeleteInvitation(string invitationId, bool isSystemAction = false)
        {
            var invitation = _dbContext.Invitations
                                       .Include(x => x.RecipientOrganizationRegulatoryProgram.Organization)
                                       .Include(x => x.RecipientOrganizationRegulatoryProgram.RegulatoryProgram)
                                       .Single(i => i.InvitationId == invitationId);
            var recipientOrganizationRegulatoryProgram = invitation.RecipientOrganizationRegulatoryProgram;
            var authorityName = recipientOrganizationRegulatoryProgram.RegulatorOrganization != null ? recipientOrganizationRegulatoryProgram.RegulatorOrganization.Name : "";
            _dbContext.Invitations.Remove(entity:invitation);
            _dbContext.SaveChanges();

            //Only log Cromerr if action was a result of a User Delete scenario
            //and not system automatically deleting Invite after Registration Pending (Bug 1989)
            if (!isSystemAction)
            {
                var existingUser = _userService.GetUserProfileByEmail(emailAddress:invitation.EmailAddress);
                var thisUserOrgRegProgUserId = Convert.ToInt32(value:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));

                var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                                 .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
                var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:actorProgramUser);
                var actorUser = _userService.GetUserProfileById(userProfileId:actorProgramUserDto.UserProfileId);

                var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
                cromerrAuditLogEntryDto.RegulatoryProgramId = recipientOrganizationRegulatoryProgram.RegulatoryProgramId;
                cromerrAuditLogEntryDto.OrganizationId = recipientOrganizationRegulatoryProgram.OrganizationId;
                cromerrAuditLogEntryDto.RegulatorOrganizationId = recipientOrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                var existingUserUserName = "n/a";
                if (existingUser != null)
                {
                    cromerrAuditLogEntryDto.UserProfileId = existingUser.UserProfileId;
                    existingUserUserName = existingUser.UserName;
                }

                cromerrAuditLogEntryDto.UserName = existingUserUserName;
                cromerrAuditLogEntryDto.UserFirstName = invitation.FirstName;
                cromerrAuditLogEntryDto.UserLastName = invitation.LastName;
                cromerrAuditLogEntryDto.UserEmailAddress = invitation.EmailAddress;
                cromerrAuditLogEntryDto.IPAddress = _httpContextService.CurrentUserIPAddress();
                cromerrAuditLogEntryDto.HostName = _httpContextService.CurrentUserHostName();
                var contentReplacements = new Dictionary<string, string>();
                contentReplacements.Add(key:"authorityName", value:authorityName);
                contentReplacements.Add(key:"organizationName", value:recipientOrganizationRegulatoryProgram.Organization.Name);
                contentReplacements.Add(key:"regulatoryProgram", value:recipientOrganizationRegulatoryProgram.RegulatoryProgram.Name);
                contentReplacements.Add(key:"firstName", value:invitation.FirstName);
                contentReplacements.Add(key:"lastName", value:invitation.LastName);
                contentReplacements.Add(key:"userName", value:existingUserUserName);
                contentReplacements.Add(key:"emailAddress", value:invitation.EmailAddress);
                contentReplacements.Add(key:"actorFirstName", value:actorUser.FirstName);
                contentReplacements.Add(key:"actorLastName", value:actorUser.LastName);
                contentReplacements.Add(key:"actorUserName", value:actorUser.UserName);
                contentReplacements.Add(key:"actorEmailAddress", value:actorUser.Email);

                _crommerAuditLogService.Log(eventType:CromerrEvent.Registration_InviteDeleted, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);
            }
        }

        public InvitationCheckEmailResultDto CheckEmailAddress(int orgRegProgramId, string emailAddress)
        {
            var existingUsers = _userService.GetProgramUsersByEmail(emailAddress:emailAddress);
            var existingUsersDifferentPrograms = new List<OrganizationRegulatoryProgramUserDto>();

            if (existingUsers != null && existingUsers.Any())
            {
                foreach (var existingUser in existingUsers)
                {
                    if (existingUser.OrganizationRegulatoryProgramId == orgRegProgramId
                        && existingUser.IsRegistrationDenied != true
                        && existingUser.IsRemoved != true)
                    {
                        return new InvitationCheckEmailResultDto
                               {
                                   ExistingUsersDifferentPrograms = null,
                                   ExistingUserSameProgram = existingUser
                               };
                    }
                    else
                    {
                        existingUsersDifferentPrograms.Add(item:existingUser);
                    }
                }

                //Found match(es) for user not yet part of this program (4.a)
                return new InvitationCheckEmailResultDto
                       {
                           ExistingUserSameProgram = null,
                           ExistingUsersDifferentPrograms = existingUsersDifferentPrograms
                       };
            }

            return new InvitationCheckEmailResultDto
                   {
                       ExistingUsersDifferentPrograms = null,
                       ExistingUserSameProgram = null
                   };
        }

        #endregion
    }
}