using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
    public class InvitationService : IInvitationService
    {
        #region fields

        private readonly ICromerrAuditLogService _crommerAuditLogService;
        private readonly LinkoExchangeContext _dbContext;
        private readonly IHttpContextService _httpContextService;
        private readonly IJurisdictionService _jurisdictionService;
        private readonly ILinkoExchangeEmailService _linkoExchangeEmailService;
        private readonly ILogger _logger;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _organizationService;
        private readonly IProgramService _programService;
        private readonly IRequestCache _requestCache;
        private readonly ISettingService _settingService;
        private readonly ITimeZoneService _timeZones;
        private readonly IUserService _userService;

        #endregion

        #region constructors and destructor

        public InvitationService(LinkoExchangeContext dbContext, ISettingService settingService, IUserService userService, IRequestCache requestCache,
                                 IOrganizationService organizationService, IHttpContextService httpContext, ITimeZoneService timeZones, ILogger logger,
                                 IProgramService programService, IMapHelper mapHelper, ICromerrAuditLogService crommerAuditLogService,
                                 ILinkoExchangeEmailService linkoExchangeEmailService, IJurisdictionService jurisdictionService)
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
            var senderProgram = _programService.GetOrganizationRegulatoryProgram(organizationRegulatoryProgramId:invitation.SenderOrganizationRegulatoryProgramId);
            var recipientProgram = _programService.GetOrganizationRegulatoryProgram(organizationRegulatoryProgramId:invitation.RecipientOrganizationRegulatoryProgramId);

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
                    var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                                  {
                                                      RegulatoryProgramId = recipientProgram.RegulatoryProgramId,
                                                      OrganizationId = recipientProgram.OrganizationId
                                                  };
                    cromerrAuditLogEntryDto.RegulatorOrganizationId = recipientProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                    cromerrAuditLogEntryDto.UserProfileId = user.UserProfileId;
                    cromerrAuditLogEntryDto.UserName = user.UserName;
                    cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
                    cromerrAuditLogEntryDto.UserLastName = user.LastName;
                    cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
                    cromerrAuditLogEntryDto.IPAddress = _httpContextService.CurrentUserIPAddress();
                    cromerrAuditLogEntryDto.HostName = _httpContextService.CurrentUserHostName();
                    var contentReplacements = new Dictionary<string, string>
                                              {
                                                  {"firstName", user.FirstName},
                                                  {"lastName", user.LastName},
                                                  {"userName", user.UserName},
                                                  {"emailAddress", user.Email}
                                              };

                    _crommerAuditLogService.Log(eventType:CromerrEvent.UserAccess_AccountResetExpired, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);
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

        public ICollection<InvitationDto> GetInvitationsForOrgRegProgram(int senderOrgRegProgramId, int targetOrgRegProgramId)
        {
            var dtos = new List<InvitationDto>();
            var org = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == senderOrgRegProgramId);

            //var authority = _dbContext.OrganizationRegulatoryPrograms
            //    .Single(o => o.OrganizationId == org.RegulatorOrganizationId
            //    && o.RegulatoryProgramId == org.RegulatoryProgramId);

            var authority = org.RegulatorOrganization ?? org.Organization;

            var invites = _dbContext.Invitations.Where(i => i.SenderOrganizationRegulatoryProgramId == senderOrgRegProgramId
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
                dto.InvitationDateTimeUtc =
                    _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:dto.InvitationDateTimeUtc.UtcDateTime, orgId:authority.OrganizationId,
                                                                          regProgramId:org.RegulatoryProgramId);
                dto.ExpiryDateTimeUtc = dto.InvitationDateTimeUtc.AddHours(hours:addExpiryHours);

                dtos.Add(item:dto);
            }

            return dtos;
        }

        public void SendUserInvite(int targetOrgRegProgramId, string email, string firstName, string lastName, InvitationType invitationType)
        {
            _logger.Info(message:$"Enter InvitationService.SendUserInvite. targetOrgRegProgramId={targetOrgRegProgramId}, email={email}, invitationType={invitationType}");

            OrganizationRegulatoryProgramUserDto existingUser;

            //See if any existing users belong to this program
            var existingProgramUsers = _userService.GetProgramUsersByEmail(emailAddress:email);
            if (existingProgramUsers.Any())
            {
                existingUser = existingProgramUsers.First();
                email = existingUser.UserProfileDto.Email;
                firstName = existingUser.UserProfileDto.FirstName;
                lastName = existingUser.UserProfileDto.LastName;

                var existingUserForThisProgram = existingProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramId == targetOrgRegProgramId);
                if (existingUserForThisProgram != null)
                {
                    if (existingUserForThisProgram.IsRegistrationApproved)
                    {
                        // user is currently active in target OrganizationRegulatoryProgram
                        _logger.Info(message:$"SendInvitation Failed. User with email={email} already exists within OrgRegProgramId={targetOrgRegProgramId}");

                        var validationIssues = new List<RuleViolation>();
                        var message = $"Invite failed to send. User with email={email} is already associated with this account";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                        throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                    }
                    else
                    {
                        // user was removed from target OrganizationRegulatoryProgram
                        existingUser = existingUserForThisProgram;

                        // Per discussion between Sundoro, Rajeeb, Shuhao, during re-invitation,
                        // tOrganizationRegulatoryProgramUser.IsRemoved should be set to false
                        var user = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId
                                                                                             == existingUser.OrganizationRegulatoryProgramUserId);
                        user.IsRemoved = false;
                        _dbContext.SaveChanges();
                    }
                }
            }
            else
            {
                existingUser = new OrganizationRegulatoryProgramUserDto
                               {
                                   UserProfileDto = new UserDto
                                                    {
                                                        Email = email,
                                                        FirstName = firstName,
                                                        LastName = lastName,
                                                        UserName = "n/a",
                                                        UserProfileId = 0
                                                    }
                               };
            }

            //For Authority inviting Industry, check if an active Admin user does not already exist within that IU
            if (invitationType == InvitationType.AuthorityToIndustry)
            {
                var orgRegProgram = _organizationService.GetOrganizationRegulatoryProgram(orgRegProgId:targetOrgRegProgramId);
                if (orgRegProgram.HasActiveAdmin)
                {
                    _logger.Info(message:$"SendInvitation Failed. Industry already has Admin User. OrgRegProgramId={targetOrgRegProgramId}");

                    var validationIssues = new List<RuleViolation>();
                    var message = "Invite failed to send. This industry already has an Administrator User.";
                    validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                    throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                }
            }

            //Check available license count
            var remaining = _organizationService.GetRemainingUserLicenseCount(orgRegProgramId:targetOrgRegProgramId);

            if (remaining < 1)
            {
                _logger.Info(message:$"SendInvitation Failed. No more remaining user licenses. OrgRegProgramId={targetOrgRegProgramId}, InviteType={invitationType}");

                var validationIssues = new List<RuleViolation>();
                var message = "No more User Licenses are available for this organization. Disable another User and try again.";
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
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
            var targetOrgRegProgram = _dbContext.OrganizationRegulatoryPrograms.Single(o => o.OrganizationRegulatoryProgramId == targetOrgRegProgramId);
            var authority = _organizationService.GetAuthority(orgRegProgramId:targetOrgRegProgramId);
            contentReplacements.Add(key:"firstName", value:firstName);
            contentReplacements.Add(key:"lastName", value:lastName);

            var authorityName =
                _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId, settingType:SettingType.EmailContactInfoName);
            var authorityEmail = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                              settingType:SettingType.EmailContactInfoEmailAddress);
            var authorityPhone = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                              settingType:SettingType.EmailContactInfoPhone);

            contentReplacements.Add(key:"authorityName", value:authorityName);
            contentReplacements.Add(key:"authorityOrganizationName", value:authority.OrganizationDto.OrganizationName);
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
                contentReplacements.Add(key:"stateName",
                                        value:_jurisdictionService.GetJurisdictionById(jurisdictionId:targetOrgRegProgram.Organization.JurisdictionId)?.Code ?? "");
            }

            var baseUrl = _httpContextService.GetRequestBaseUrl();
            var url = baseUrl + "Account/Register?token=" + invitationId;
            contentReplacements.Add(key:"link", value:url);

            var thisUserOrgRegProgUserId = Convert.ToInt32(value:_httpContextService.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
            var actorUser = _userService.GetUserProfileById(userProfileId:actorProgramUser.UserProfileId);

            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
            var cromerrContentReplacements = new Dictionary<string, string>();

            cromerrAuditLogEntryDto.UserProfileId = existingUser.UserProfileId == 0 ? default(int?) : existingUser.UserProfileId;
            cromerrAuditLogEntryDto.UserName = existingUser.UserProfileDto.UserName;

            cromerrContentReplacements.Add(key:"userName", value:existingUser.UserProfileDto.UserName);

            cromerrAuditLogEntryDto.RegulatoryProgramId = targetOrgRegProgram.RegulatoryProgramId;
            cromerrAuditLogEntryDto.OrganizationId = targetOrgRegProgram.OrganizationId;
            cromerrAuditLogEntryDto.RegulatorOrganizationId = authority.OrganizationId;
            cromerrAuditLogEntryDto.UserFirstName = firstName;
            cromerrAuditLogEntryDto.UserLastName = lastName;
            cromerrAuditLogEntryDto.UserEmailAddress = email;
            cromerrAuditLogEntryDto.IPAddress = _httpContextService.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContextService.CurrentUserHostName();

            cromerrContentReplacements.Add(key:"authorityName", value:authority.OrganizationDto.OrganizationName);
            cromerrContentReplacements.Add(key:"organizationName", value:targetOrgRegProgram.Organization.Name);
            cromerrContentReplacements.Add(key:"regulatoryProgram", value:authority.RegulatoryProgramDto.Name);
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
                default: throw new Exception(message:"ERROR: unknown EmailType");
            }

            var emailEntry = _linkoExchangeEmailService.GetEmailEntryForOrgRegProgramUser(user:existingUser, emailType:emailType, contentReplacements:contentReplacements);
            emailEntry.RecipientOrgulatoryProgramId = targetOrgRegProgram.RegulatoryProgramId;
            emailEntry.RecipientOrganizationId = targetOrgRegProgram.OrganizationId;
            emailEntry.RecipientRegulatorOrganizationId = cromerrAuditLogEntryDto.RegulatorOrganizationId;

            _linkoExchangeEmailService.SendEmails(emailEntries:new List<EmailEntry> {emailEntry});

            _logger.Info(message:$"Leaving InvitationService.SendUserInvite. targetOrgRegProgramId={targetOrgRegProgramId}, email={email}, invitationType={invitationType}");
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

                var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                              {
                                                  RegulatoryProgramId = recipientOrganizationRegulatoryProgram.RegulatoryProgramId,
                                                  OrganizationId = recipientOrganizationRegulatoryProgram.OrganizationId
                                              };
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
                var contentReplacements = new Dictionary<string, string>
                                          {
                                              {"authorityName", authorityName},
                                              {"organizationName", recipientOrganizationRegulatoryProgram.Organization.Name},
                                              {"regulatoryProgram", recipientOrganizationRegulatoryProgram.RegulatoryProgram.Name},
                                              {"firstName", invitation.FirstName},
                                              {"lastName", invitation.LastName},
                                              {"userName", existingUserUserName},
                                              {"emailAddress", invitation.EmailAddress},
                                              {"actorFirstName", actorUser.FirstName},
                                              {"actorLastName", actorUser.LastName},
                                              {"actorUserName", actorUser.UserName},
                                              {"actorEmailAddress", actorUser.Email}
                                          };

                _crommerAuditLogService.Log(eventType:CromerrEvent.Registration_InviteDeleted, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);
            }
        }

        public InvitationCheckEmailResultDto CheckEmailAddress(int orgRegProgramId, string emailAddress)
        {
            var existingProgramUsers = _userService.GetProgramUsersByEmail(emailAddress:emailAddress);
            var invitationCheckEmailResultDto = new InvitationCheckEmailResultDto();

            if (existingProgramUsers != null && existingProgramUsers.Any())
            {
                invitationCheckEmailResultDto.ExistingOrgRegProgramUser = existingProgramUsers.First();
                var existingUserForThisProgram = existingProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramId == orgRegProgramId);

                if (existingUserForThisProgram != null)
                {
                    invitationCheckEmailResultDto.ExistingOrgRegProgramUser = existingUserForThisProgram;
                    invitationCheckEmailResultDto.IsUserActiveInSameProgram = existingUserForThisProgram.IsRegistrationApproved;
                }
            }

            return invitationCheckEmailResultDto;
        }

        #endregion
    }
}