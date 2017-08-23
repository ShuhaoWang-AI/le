using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.CompilerServices;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Validation;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Linko.LinkoExchange.Services.Base;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.HttpContext;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using NLog;

namespace Linko.LinkoExchange.Services.User
{
    public class UserService : BaseService, IUserService
    {
        #region fields

        private readonly ICromerrAuditLogService _crommerAuditLogService;

        private readonly LinkoExchangeContext _dbContext;
        private readonly IDictionary<SystemSettingType, string> _globalSettings;
        private readonly IHttpContextService _httpContext;
        private readonly ILinkoExchangeEmailService _linkoExchangeEmaiService;
        private readonly ILogger _logService;
        private readonly IMapHelper _mapHelper;
        private readonly IOrganizationService _orgService;
        private readonly IRequestCache _requestCache;
        private readonly ISettingService _settingService;
        private readonly ITimeZoneService _timeZones;

        #endregion

        #region constructors and destructor

        public UserService(LinkoExchangeContext dbContext, IHttpContextService httpContext, ISettingService settingService, IOrganizationService orgService,
                           IRequestCache requestCache, ITimeZoneService timeZones, ILogger logService, IMapHelper mapHelper, ICromerrAuditLogService crommerAuditLogService,
                           ILinkoExchangeEmailService linkoExchangeEmailService)
        {
            _dbContext = dbContext;
            _httpContext = httpContext;
            _settingService = settingService;
            _orgService = orgService;
            _requestCache = requestCache;
            _globalSettings = _settingService.GetGlobalSettings();
            _timeZones = timeZones;
            _logService = logService;
            _mapHelper = mapHelper;
            _crommerAuditLogService = crommerAuditLogService;
            _linkoExchangeEmaiService = linkoExchangeEmailService;
        }

        #endregion

        #region public methods

        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            bool retVal;

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var currentOrgRegProgUserId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));
            var currentPortalName = _httpContext.GetClaimValue(claimType:CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value:currentPortalName) ? "" : currentPortalName.Trim().ToLower();

            switch (apiName)
            {
                case "GetOrganizationRegulatoryProgramUser":
                {
                    //If the current user is attempting to access their own information, allow this.
                    var targetOrgRegProgUserId = id[0];
                    if (currentOrgRegProgUserId == targetOrgRegProgUserId)
                    {
                        return true;
                    }

                    goto case "GetOrganizationRegulatoryProgramUser_NotSameUser";
                }

                case "GetOrganizationRegulatoryProgramUser_NotSameUser":
                case "EnableDisableUserAccount":
                case "UpdateUserPermissionGroupId":
                case "UpdateUserSignatoryStatus":
                case "RemoveUser":
                case "ApprovePendingRegistration":
                case "LockUnlockUserAccount":
                case "ResetUser":
                {
                    var targetOrgRegProgUserId = id[0];

                    var targetOrgRegProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                                            .Include(orpu => orpu.OrganizationRegulatoryProgram)
                                                            .SingleOrDefault(orpu => orpu.OrganizationRegulatoryProgramUserId == targetOrgRegProgUserId);

                    var currentUsersPermissionGroup = _dbContext.OrganizationRegulatoryProgramUsers
                                                                .Single(orpu => orpu.OrganizationRegulatoryProgramUserId == currentOrgRegProgUserId)
                                                                .PermissionGroup;

                    var isCurrentUserAdminOfTargetUser = currentUsersPermissionGroup.Name.ToLower().StartsWith(value:"admin")
                                                         && currentUsersPermissionGroup.OrganizationRegulatoryProgramId == targetOrgRegProgramUser.OrganizationRegulatoryProgramId;

                    //
                    //Authorize the correct Authority
                    //

                    if (currentPortalName.Equals(value:"authority"))
                    {
                        //this will also handle scenarios where the current user and target user of from the same org reg program
                        retVal = currentOrgRegProgramId
                                 == _orgService.GetAuthority(orgRegProgramId:targetOrgRegProgramUser.OrganizationRegulatoryProgramId).OrganizationRegulatoryProgramId;

                        //if we are within the same org reg program, current user must be an admin
                        if (retVal && currentOrgRegProgramId == targetOrgRegProgramUser.OrganizationRegulatoryProgramId)
                        {
                            retVal = isCurrentUserAdminOfTargetUser;
                        }
                    }
                    else
                    {
                        //
                        //Authorize Industry Admins only
                        //

                        retVal = isCurrentUserAdminOfTargetUser;
                    }
                }

                    break;

                default: throw new Exception(message:$"ERROR: Unhandled API authorization attempt using name = '{apiName}'");
            }

            return retVal;
        }

        public UserDto GetUserProfileById(int userProfileId)
        {
            var userProfile = _dbContext.Users.Single(up => up.UserProfileId == userProfileId);
            return _mapHelper.GetUserDtoFromUserProfile(userProfile:userProfile);
        }

        public UserDto GetUserProfileByEmail(string emailAddress)
        {
            var userProfile = _dbContext.Users.SingleOrDefault(u => u.Email == emailAddress);
            return userProfile != null ? _mapHelper.GetUserDtoFromUserProfile(userProfile:userProfile) : null;
        }

        public ICollection<OrganizationRegulatoryProgramUserDto> GetProgramUsersByEmail(string emailAddress)
        {
            var dtos = new List<OrganizationRegulatoryProgramUserDto>();
            var userProfile = _dbContext.Users.SingleOrDefault(u => u.Email == emailAddress && u.EmailConfirmed);
            if (userProfile != null)
            {
                var programUsers = _dbContext.OrganizationRegulatoryProgramUsers.Where(o => o.UserProfileId == userProfile.UserProfileId).ToList();
                foreach (var programUser in programUsers)
                {
                    var dto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:programUser);

                    //Need to map UserProfileDto manually here
                    dto.UserProfileDto = GetUserProfileById(userProfileId:programUser.UserProfileId);
                    dtos.Add(item:dto);
                }
            }

            return dtos;
        }

        public int GetPendingRegistrationProgramUsersCount(int orgRegProgramId)
        {
            return _dbContext.OrganizationRegulatoryProgramUsers
                             .Count(u => u.InviterOrganizationRegulatoryProgramId == orgRegProgramId
                                         && u.IsRegistrationApproved == false
                                         && u.IsRegistrationDenied == false
                                         && u.IsRemoved == false);
        }

        public List<OrganizationRegulatoryProgramUserDto> GetPendingRegistrationProgramUsers(int orgRegProgramId)
        {
            var dtos = new List<OrganizationRegulatoryProgramUserDto>();

            var users = _dbContext.OrganizationRegulatoryProgramUsers
                                  .Where(u => u.InviterOrganizationRegulatoryProgramId == orgRegProgramId
                                              && u.IsRegistrationApproved == false
                                              && u.IsRegistrationDenied == false
                                              && u.IsRemoved == false);

            foreach (var user in users.ToList())
            {
                var dto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:user);

                //manually map user profile child
                var userProfileDto = GetUserProfileById(userProfileId:user.UserProfileId);

                //Need to modify date time to local
                dto.RegistrationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:dto.RegistrationDateTimeUtc.Value.UtcDateTime,
                                                                                                    orgRegProgramId:orgRegProgramId);
                userProfileDto.CreationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:userProfileDto.CreationDateTimeUtc.Value.UtcDateTime,
                                                                                                           orgRegProgramId:orgRegProgramId);

                dto.UserProfileDto = userProfileDto;
                dtos.Add(item:dto);
            }

            return dtos;
        }

        public List<OrganizationRegulatoryProgramUserDto> GetUserProfilesForOrgRegProgram(int orgRegProgramId, bool? isRegApproved, bool? isRegDenied, bool? isEnabled,
                                                                                          bool? isRemoved)
        {
            var dtos = new List<OrganizationRegulatoryProgramUserDto>();
            var users = _dbContext.OrganizationRegulatoryProgramUsers.Where(u => u.OrganizationRegulatoryProgramId == orgRegProgramId);

            if (isRegApproved.HasValue)
            {
                users = users.Where(u => u.IsRegistrationApproved == isRegApproved);
            }
            if (isRegDenied.HasValue)
            {
                users = users.Where(u => u.IsRegistrationDenied == isRegDenied);
            }
            if (isEnabled.HasValue)
            {
                users = users.Where(u => u.IsEnabled == isEnabled);
            }
            if (isRemoved.HasValue)
            {
                users = users.Where(u => u.IsRemoved == isRemoved);
            }

            foreach (var user in users.ToList())
            {
                var dto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:user);

                //manually map user profile child
                var userProfileDto = GetUserProfileById(userProfileId:user.UserProfileId);

                //Need to modify date time to local
                dto.RegistrationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:dto.RegistrationDateTimeUtc.Value.UtcDateTime,
                                                                                                    orgRegProgramId:orgRegProgramId);
                userProfileDto.CreationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:userProfileDto.CreationDateTimeUtc.Value.UtcDateTime,
                                                                                                           orgRegProgramId:orgRegProgramId);

                dto.UserProfileDto = userProfileDto;
                dtos.Add(item:dto);
            }

            return dtos;
        }

        public void UpdateUserState(int orgRegProgUserId, bool? isRegApproved, bool? isRegDenied, bool? isEnabled, bool? isRemoved)
        {
            var user = _dbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (user != null)
            {
                if (isRegApproved.HasValue)
                {
                    user.IsRegistrationApproved = isRegApproved.Value;
                }
                if (isRegDenied.HasValue)
                {
                    user.IsRegistrationDenied = isRegDenied.Value;
                }
                if (isEnabled.HasValue)
                {
                    user.IsEnabled = isEnabled.Value;
                }
                if (isRemoved.HasValue)
                {
                    user.IsRemoved = isRemoved.Value;
                }

                //Persist modification date and modifier actor
                user.LastModificationDateTimeUtc = DateTimeOffset.Now;
                user.LastModifierUserId = Convert.ToInt32(value:_httpContext.CurrentUserProfileId());
                _dbContext.SaveChanges();
            }
            else
            {
                throw new Exception(message:$"ERROR: Cannot find Org Reg Program User associated with OrganizationRegulatoryProgramUserId={orgRegProgUserId}.");
            }
        }

        public void UpdateUserPermissionGroupId(int orgRegProgUserId, int permissionGroupId, bool isAuthorizationRequired = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgUserId))
            {
                throw new UnauthorizedAccessException();
            }

            var user = _dbContext.OrganizationRegulatoryProgramUsers
                                 .Include(path:"OrganizationRegulatoryProgram")
                                 .Single(o => o.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            var previousPermissionGroupId = user.PermissionGroupId;
            if (previousPermissionGroupId == permissionGroupId)
            {
                return;
            }

            user.PermissionGroupId = permissionGroupId;
            _dbContext.SaveChanges();

            var thisUserOrgRegProgUserId = Convert.ToInt32(value:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                             .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:actorProgramUser);
            var actorUser = GetUserProfileById(userProfileId:actorProgramUserDto.UserProfileId);

            var targetOrgRegProgram = user.OrganizationRegulatoryProgram;
            var userProfile = GetUserProfileById(userProfileId:user.UserProfileId);

            var previousRoleLabel = _dbContext.PermissionGroups.Single(pg => pg.PermissionGroupId == previousPermissionGroupId).Name;
            var newRoleLabel = _dbContext.PermissionGroups.Single(pg => pg.PermissionGroupId == permissionGroupId).Name;

            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
            cromerrAuditLogEntryDto.RegulatoryProgramId = targetOrgRegProgram.RegulatoryProgramId;
            cromerrAuditLogEntryDto.OrganizationId = targetOrgRegProgram.OrganizationId;
            cromerrAuditLogEntryDto.RegulatorOrganizationId = targetOrgRegProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = userProfile.UserProfileId;
            cromerrAuditLogEntryDto.UserName = userProfile.UserName;
            cromerrAuditLogEntryDto.UserFirstName = userProfile.FirstName;
            cromerrAuditLogEntryDto.UserLastName = userProfile.LastName;
            cromerrAuditLogEntryDto.UserEmailAddress = userProfile.Email;
            cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
            var contentReplacements = new Dictionary<string, string>
                                      {
                                          {"organizationName", targetOrgRegProgram.Organization.Name},
                                          {"firstName", userProfile.FirstName},
                                          {"lastName", userProfile.LastName},
                                          {"userName", userProfile.UserName},
                                          {"emailAddress", userProfile.Email},
                                          {"oldRole", previousRoleLabel},
                                          {"newRole", newRoleLabel},
                                          {"actorOrganizationName", actorProgramUserDto.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName},
                                          {"actorFirstName", actorUser.FirstName},
                                          {"actorLastName", actorUser.LastName},
                                          {"actorUserName", actorUser.UserName},
                                          {"actorEmailAddress", actorUser.Email}
                                      };

            _crommerAuditLogService.Log(eventType:CromerrEvent.UserAccess_RoleChange, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);
        }

        public void UpdateUserSignatoryStatus(int orgRegProgUserId, bool isSignatory, bool isAuthorizationRequired = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgUserId))
            {
                throw new UnauthorizedAccessException();
            }

            var programUser = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (programUser.IsSignatory == isSignatory)
            {
                //No change therefore...
                return;
            }

            programUser.IsSignatory = isSignatory;
            programUser.LastModificationDateTimeUtc = DateTimeOffset.Now;
            programUser.LastModifierUserId = _httpContext.CurrentUserProfileId();
            _dbContext.SaveChanges();

            var user = GetUserProfileById(userProfileId:programUser.UserProfileId);
            var orgRegProgram = programUser.OrganizationRegulatoryProgram;
            var authority = _dbContext.OrganizationRegulatoryPrograms.SingleOrDefault(o => o.OrganizationId == programUser.OrganizationRegulatoryProgram.RegulatorOrganizationId
                                                                                           && o.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId);
            if (authority == null)
            {
                //This IS an authority
                authority = orgRegProgram;
            }

            var authorityName =
                _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId, settingType:SettingType.EmailContactInfoName);
            var stateName = orgRegProgram.Organization.JurisdictionId.HasValue
                                ? _dbContext.Jurisdictions.Single(j => j.JurisdictionId == orgRegProgram.Organization.JurisdictionId.Value).Code
                                : "";
            var emailAddress = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                            settingType:SettingType.EmailContactInfoEmailAddress);
            var phoneNumber = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                           settingType:SettingType.EmailContactInfoPhone);

            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add(key:"firstName", value:user.FirstName);
            contentReplacements.Add(key:"lastName", value:user.LastName);
            contentReplacements.Add(key:"userName", value:user.UserName);
            contentReplacements.Add(key:"emailAddress", value:user.Email);
            contentReplacements.Add(key:"authorityName", value:authorityName);
            contentReplacements.Add(key:"authorityOrganizationName", value:authority.Organization.Name);
            contentReplacements.Add(key:"organizationName", value:orgRegProgram.Organization.Name);
            contentReplacements.Add(key:"addressLine1", value:orgRegProgram.Organization.AddressLine1);
            contentReplacements.Add(key:"cityName", value:orgRegProgram.Organization.CityName);
            contentReplacements.Add(key:"stateName", value:stateName);
            contentReplacements.Add(key:"supportEmail", value:emailAddress);
            contentReplacements.Add(key:"supportPhoneNumber", value:phoneNumber);

            //Email user
            var emailType = isSignatory ? EmailType.Signature_SignatoryGranted : EmailType.Signature_SignatoryRevoked;
            var adminEmailType = isSignatory ? EmailType.Signature_SignatoryGrantedToAdmin : EmailType.Signature_SignatoryRevokedToAdmin;

            _requestCache.SetValue(key:CacheKey.EmailRecipientRegulatoryProgramId, value:orgRegProgram.RegulatoryProgramId);
            _requestCache.SetValue(key:CacheKey.EmailRecipientOrganizationId, value:orgRegProgram.OrganizationId);
            _requestCache.SetValue(key:CacheKey.EmailRecipientRegulatoryOrganizationId, value:orgRegProgram.RegulatorOrganizationId);

            //Log Cromerr
            var cromerrEvent = isSignatory ? CromerrEvent.Signature_SignatoryGranted : CromerrEvent.Signature_SignatoryRevoked;
            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                          {
                                              RegulatoryProgramId = programUser.OrganizationRegulatoryProgram.RegulatoryProgramId,
                                              OrganizationId = programUser.OrganizationRegulatoryProgram.OrganizationId
                                          };
            cromerrAuditLogEntryDto.RegulatorOrganizationId = programUser.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = programUser.UserProfileId;
            cromerrAuditLogEntryDto.UserName = user.UserName;
            cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
            cromerrAuditLogEntryDto.UserLastName = user.LastName;
            cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
            cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();

            _crommerAuditLogService.Log(eventType:cromerrEvent, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);

            //Email the user
            var emailEntries = new List<EmailEntry>();
            var emailEntry = _linkoExchangeEmaiService.GetEmailEntryForUser(user:user, emailType:emailType, contentReplacements:contentReplacements, orgRegProg:orgRegProgram);
            emailEntries.Add(item:emailEntry);

            //Email all IU Admins
            var admins = _dbContext.OrganizationRegulatoryProgramUsers
                                   .Where(o => o.PermissionGroup.Name == "Administrator"
                                               && o.OrganizationRegulatoryProgramId == programUser.OrganizationRegulatoryProgram.OrganizationRegulatoryProgramId
                                               && o.IsEnabled
                                               && !o.IsRemoved
                                               && !o.IsRegistrationDenied
                                               && o.IsRegistrationApproved);

            foreach (var admin in admins.ToList())
            {
                var adminProfile = GetUserProfileById(userProfileId:admin.UserProfileId);
                if (!adminProfile.IsAccountLocked && !adminProfile.IsAccountResetRequired)
                {
                    var contentReplacementsForAdmin = new Dictionary<string, string>
                                                      {
                                                          {"adminFirstName", adminProfile.FirstName},
                                                          {"adminLastName", adminProfile.LastName},
                                                          {"firstName", user.FirstName},
                                                          {"lastName", user.LastName},
                                                          {"authorityName", authorityName},
                                                          {"authorityOrganizationName", authority.Organization.Name},
                                                          {"email", user.Email},
                                                          {"organizationName", orgRegProgram.Organization.Name},
                                                          {"addressLine1", orgRegProgram.Organization.AddressLine1},
                                                          {"cityName", orgRegProgram.Organization.CityName},
                                                          {"stateName", stateName},
                                                          {"emailAddress", emailAddress},
                                                          {"phoneNumber", phoneNumber}
                                                      };

                    emailEntries.Add(item:
                                     _linkoExchangeEmaiService.GetEmailEntryForUser(user:adminProfile, emailType:adminEmailType, contentReplacements:contentReplacementsForAdmin,
                                                                                    orgRegProg:orgRegProgram));
                }
            }

            _dbContext.SaveChanges();
            _linkoExchangeEmaiService.SendEmails(emailEntries:emailEntries);
        }

        public ResetUserResultDto ResetUser(int targetOrgRegProgUserId, string newEmailAddress, bool isAuthorizationRequired = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:targetOrgRegProgUserId))
            {
                throw new UnauthorizedAccessException();
            }

            var targetOrgRegProgUser = _dbContext.OrganizationRegulatoryProgramUsers
                                                 .Single(orpu => orpu.OrganizationRegulatoryProgramUserId == targetOrgRegProgUserId);
            var userProfileId = targetOrgRegProgUser.UserProfileId;
            var targetOrgRegProgramId = targetOrgRegProgUser.OrganizationRegulatoryProgramId;

            var user = _dbContext.Users.Single(u => u.UserProfileId == userProfileId);

            //Check user is not support role
            if (user.IsInternalAccount)
            {
                return new ResetUserResultDto
                       {
                           IsSuccess = false,
                           FailureReason = ResetUserFailureReason.CannotResetSupportRole
                       };
            }

            //Check user is not THIS user's own account
            var thisUserProfileId = _httpContext.CurrentUserProfileId();
            if (userProfileId == thisUserProfileId)
            {
                return new ResetUserResultDto
                       {
                           IsSuccess = false,
                           FailureReason = ResetUserFailureReason.CannotResetOwnAccount
                       };
            }

            var emailEntries = new List<EmailEntry>();

            //Send all email types
            var supportPhoneNumber = _globalSettings[key:SystemSettingType.SupportPhoneNumber];
            var supportEmail = _globalSettings[key:SystemSettingType.SupportEmailAddress];

            //1) Send "Email Changed" emails  
            //Find all possible authorities
            var authorityList = _orgService.GetUserAuthorityListForEmailContent(userProfileId:userProfileId);

            var contentReplacements = new Dictionary<string, string>
                                      {
                                          {"firstName", user.FirstName},
                                          {"lastName", user.LastName},
                                          {"oldEmail", user.OldEmailAddress},
                                          {"newEmail", user.Email},
                                          {"authorityList", authorityList},
                                          {"supportPhoneNumber", supportPhoneNumber},
                                          {"supportEmail", supportEmail}
                                      };

            if (string.IsNullOrEmpty(value:newEmailAddress) || user.Email == newEmailAddress)
            {
                // no change
            }
            else
            {
                //Check if new address is in use
                if (GetUserProfileByEmail(emailAddress:newEmailAddress) != null)
                {
                    return new ResetUserResultDto
                           {
                               IsSuccess = false,
                               FailureReason = ResetUserFailureReason.NewEmailAddressAlreadyInUse
                           };
                }
                else
                {
                    user.OldEmailAddress = user.Email;
                    user.Email = newEmailAddress;
                    var entries =
                        _linkoExchangeEmaiService.GetAllProgramEmailEntiresForUser(userProfile:user, emailType:EmailType.Profile_EmailChanged,
                                                                                   contentReplacements:contentReplacements).ToList();

                    emailEntries.AddRange(collection:entries);
                    var emailEntriesForOldEmailAddress = entries.Select(i => i.Clone(overrideEmailAddress:user.OldEmailAddress)).ToList();
                    emailEntries.AddRange(collection:emailEntriesForOldEmailAddress);
                }
            }

            //reset flags
            //(DO NOT set password hash to NULL or else they will get "Invalid Login Attempt" when attempting to login instead of "You have been RESET")
            user.IsAccountLocked = false;
            user.IsAccountResetRequired = true;

            ////Delete SQs and KBQs //WE NO LONGER DELETE IN CASE INVITATIONS ARE LOST/EXPIRED AND WE NEED TO AUTHENTICATE ANOTHER INVITE REQUEST.
            //var answers = _dbContext.UserQuestionAnswers
            //    .Include("Question").Where(a => a.UserProfileId == userProfileId);
            //if (answers != null && answers.Count() > 0)
            //{
            //    foreach (var answer in answers)
            //    {
            //        _dbContext.UserQuestionAnswers.Remove(answer);
            //    }
            //}

            _dbContext.SaveChanges();

            //2) Send "Reg Reset" Email
            var token = Guid.NewGuid().ToString();

            //Create Invitation entry
            //
            //Case "Authority Reset Authority" : sender org reg program is same as target/recipient org reg program
            //Case "Authority Reset Industry" : sender org reg program is different than target/recipient org reg program
            var senderOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var newInvitation = _dbContext.Invitations.Create();
            newInvitation.InvitationId = token;
            newInvitation.InvitationDateTimeUtc = DateTimeOffset.Now;
            newInvitation.EmailAddress = user.Email;
            newInvitation.FirstName = user.FirstName;
            newInvitation.LastName = user.LastName;
            newInvitation.RecipientOrganizationRegulatoryProgramId = targetOrgRegProgramId;
            newInvitation.SenderOrganizationRegulatoryProgramId = senderOrgRegProgramId;
            newInvitation.IsResetInvitation = true;
            _dbContext.Invitations.Add(entity:newInvitation);
            _dbContext.SaveChanges();

            var baseUrl = _httpContext.GetRequestBaseUrl();
            var link = baseUrl + "Account/Register?token=" + token;
            contentReplacements = new Dictionary<string, string> {{"link", link}, {"supportPhoneNumber", supportPhoneNumber}, {"supportEmail", supportEmail}};
            _requestCache.SetValue(key:CacheKey.Token, value:token);

            //_emailService.SendEmail(new[] { user.Email }, EmailType.Profile_ResetProfileRequired, contentReplacements); 
            emailEntries.AddRange(collection:
                                  _linkoExchangeEmaiService.GetAllProgramEmailEntiresForUser(userProfile:user, emailType:EmailType.Profile_ResetProfileRequired,
                                                                                             contentReplacements:contentReplacements));

            //Log to Cromerr
            var actorOrgRegProgUserId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                             .Include(path:"OrganizationRegulatoryProgram")
                                             .Single(u => u.OrganizationRegulatoryProgramUserId == actorOrgRegProgUserId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:actorProgramUser);
            var actorUser = GetUserProfileById(userProfileId:actorProgramUserDto.UserProfileId);

            var allOrgRegProgramUsers = _dbContext.OrganizationRegulatoryProgramUsers
                                                  .Include(orpu => orpu.OrganizationRegulatoryProgram)
                                                  .Where(orpu => orpu.UserProfileId == userProfileId)
                                                  .ToList();

            foreach (var targetOrgRegProgramUser in allOrgRegProgramUsers)
            {
                var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                              {
                                                  RegulatoryProgramId = targetOrgRegProgramUser.OrganizationRegulatoryProgram.RegulatoryProgramId,
                                                  OrganizationId = targetOrgRegProgramUser.OrganizationRegulatoryProgram.OrganizationId
                                              };
                cromerrAuditLogEntryDto.RegulatorOrganizationId = targetOrgRegProgramUser.OrganizationRegulatoryProgram.RegulatorOrganizationId
                                                                  ?? cromerrAuditLogEntryDto.OrganizationId;
                cromerrAuditLogEntryDto.UserProfileId = user.UserProfileId;
                cromerrAuditLogEntryDto.UserName = user.UserName;
                cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
                cromerrAuditLogEntryDto.UserLastName = user.LastName;
                cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
                cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
                contentReplacements = new Dictionary<string, string>
                                      {
                                          {"firstName", user.FirstName},
                                          {"lastName", user.LastName},
                                          {"userName", user.UserName},
                                          {"emailAddress", user.Email},
                                          {"authorityName", actorProgramUserDto.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName},
                                          {"authorityFirstName", actorUser.FirstName},
                                          {"authorityLastName", actorUser.LastName},
                                          {"authorityUserName", actorUser.UserName},
                                          {"authorityEmailaddress", actorUser.Email}
                                      };

                _crommerAuditLogService.Log(eventType:CromerrEvent.UserAccess_AccountResetInitiated, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);
            }

            // Send emails
            _linkoExchangeEmaiService.SendEmails(emailEntries:emailEntries);

            return new ResetUserResultDto
                   {
                       IsSuccess = true
                   };
        }

        private void SendAccountLockoutEmails(UserProfile user, AccountLockEvent reason)
        {
            var emailEntries = new List<EmailEntry>();

            //Email industry admins and authority admins
            //
            //1. Find all organization reg programs user is associated with
            //2. Email all IU Admin users of those org reg programs
            //3. Find all regulator orgs of those orgs (if exist)
            //4. Email all Authority Admin users of those org reg programs
            var programs = _dbContext.OrganizationRegulatoryProgramUsers
                                     .Where(o => o.UserProfileId == user.UserProfileId)
                                     .Select(o => o.OrganizationRegulatoryProgram).Distinct();
            var authorityList = new List<OrganizationRegulatoryProgram>(); //distinct authorities
            Dictionary<string, string> contentReplacements;

            //To ensure we don't send duplicate admin emails to the same person (if they are Auth admin and locked person belongs
            //to more than one IU under the same program) 
            var adminEmailList = new List<string>();

            foreach (var program in programs.ToList())
            {
                //Find admin users in each of these
                var admins = _dbContext.OrganizationRegulatoryProgramUsers
                                       .Where(o => o.PermissionGroup.Name == "Administrator"
                                                   && o.OrganizationRegulatoryProgramId == program.OrganizationRegulatoryProgramId).Distinct();

                // Send emails to IU admins
                foreach (var admin in admins.ToList())
                {
                    var adminEmail = GetUserProfileById(userProfileId:admin.UserProfileId).Email;
                    if (!adminEmailList.Contains(item:adminEmail))
                    {
                        contentReplacements =
                            new Dictionary<string, string> {{"firstName", user.FirstName}, {"lastName", user.LastName}, {"userName", user.UserName}, {"email", user.Email}};

                        //_emailService.SendEmail(new[] { adminEmail }, EmailType.UserAccess_LockoutToSysAdmins, contentReplacements, perProgram);
                        adminEmailList.Add(item:adminEmail);

                        var userDto = GetUserProfileById(userProfileId:admin.UserProfileId);
                        var emailEntry = _linkoExchangeEmaiService.GetEmailEntryForUser(user:userDto, emailType:EmailType.UserAccess_LockoutToSysAdmins,
                                                                                        contentReplacements:contentReplacements, orgRegProg:program);
                        emailEntries.Add(item:emailEntry);
                    }
                }

                //Get authority's org id, if it exists. If not, they ARE the authority
                var authorityOrganizationId = program.RegulatorOrganizationId.HasValue
                                                  ? program.RegulatorOrganizationId.Value
                                                  : program.OrganizationId;

                //Find distinct authorities
                var authority = _dbContext.OrganizationRegulatoryPrograms
                                          .Single(o => o.OrganizationId == authorityOrganizationId
                                                       && o.RegulatoryProgramId == program.RegulatoryProgramId);
                if (!authorityList.Exists(a => a.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId))
                {
                    authorityList.Add(item:authority);
                }
            }

            var authorityName = "";
            foreach (var authority in authorityList)
            {
                //Find admin users in each of these
                var admins = _dbContext.OrganizationRegulatoryProgramUsers
                                       .Where(o => o.PermissionGroup.Name == "Administrator"
                                                   && o.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId
                                                   && o.IsEnabled
                                                   && !o.IsRemoved
                                                   && !o.IsRegistrationDenied
                                                   && o.IsRegistrationApproved);

                foreach (var admin in admins.ToList())
                {
                    var adminUserProfile = GetUserProfileById(userProfileId:admin.UserProfileId);

                    if (!adminUserProfile.IsAccountLocked
                        && !adminUserProfile.IsAccountResetRequired
                        && !adminEmailList.Contains(item:adminUserProfile.Email))
                    {
                        contentReplacements =
                            new Dictionary<string, string> {{"firstName", user.FirstName}, {"lastName", user.LastName}, {"userName", user.UserName}, {"email", user.Email}};

                        //_emailService.SendEmail(new[] { adminUserProfile.Email }, EmailType.UserAccess_LockoutToSysAdmins, contentReplacements);
                        adminEmailList.Add(item:adminUserProfile.Email);

                        var userDto = GetUserProfileById(userProfileId:admin.UserProfileId);
                        var emailEntry = _linkoExchangeEmaiService.GetEmailEntryForUser(user:userDto, emailType:EmailType.UserAccess_LockoutToSysAdmins,
                                                                                        contentReplacements:contentReplacements, orgRegProg:authority);
                        emailEntries.Add(item:emailEntry);
                    }
                }
            }

            foreach (var program in programs.ToList())
            {
                if (reason == AccountLockEvent.ManualAction)
                {
                    //Send to user on behalf of each program's authority
                    var authority = _orgService.GetAuthority(orgRegProgramId:program.OrganizationRegulatoryProgramId);
                    authorityName = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                                 settingType:SettingType.EmailContactInfoName);
                    var authorityEmail = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                                      settingType:SettingType.EmailContactInfoEmailAddress);
                    var authorityPhone = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                                      settingType:SettingType.EmailContactInfoPhone);

                    contentReplacements = new Dictionary<string, string>
                                          {
                                              {"authorityName", authorityName},
                                              {"authorityOrganizationName", authority.OrganizationDto.OrganizationName},
                                              {"authoritySupportEmail", authorityEmail},
                                              {"authoritySupportPhoneNumber", authorityPhone}
                                          };

                    var emailEntry = _linkoExchangeEmaiService.GetEmailEntryForUser(user:user, emailType:EmailType.UserAccess_AccountLockout,
                                                                                    contentReplacements:contentReplacements, orgRegProg:program);
                    emailEntries.Add(item:emailEntry);
                }
                else
                {
                    var supportPhoneNumber = _globalSettings[key:SystemSettingType.SupportPhoneNumber];
                    var supportEmail = _globalSettings[key:SystemSettingType.SupportEmailAddress];
                    var authorityListString = _orgService.GetUserAuthorityListForEmailContent(userProfileId:user.UserProfileId);

                    contentReplacements = new Dictionary<string, string>
                                          {
                                              {"firstName", user.FirstName},
                                              {"lastName", user.LastName},
                                              {"authorityList", authorityListString},
                                              {"supportPhoneNumber", supportPhoneNumber},
                                              {"supportEmail", supportEmail}
                                          };

                    var orgRegProgramIdStr = _httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId);
                    if (!string.IsNullOrWhiteSpace(value:orgRegProgramIdStr))
                    {
                        var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
                        var authorityOrganization = _orgService.GetAuthority(orgRegProgramId:currentOrgRegProgramId);

                        authorityName = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authorityOrganization.OrganizationRegulatoryProgramId,
                                                                                     settingType:SettingType.EmailContactInfoName);

                        if (!contentReplacements.Keys.Contains(value:"authorityName"))
                        {
                            contentReplacements.Add(key:"authorityName", value:authorityName);
                        }
                    }

                    var emailType = EmailType.Profile_KBQFailedLockout;
                    switch (reason)
                    {
                        case AccountLockEvent.ExceededPasswordMaxAttemptsDuringSignatureCeremony:
                            emailType = EmailType.COR_PasswordFailedLockout;
                            break;
                        case AccountLockEvent.ExceededPasswordMaxAttemptsDuringRepudiationCeremony:
                            emailType = EmailType.Repudiation_PasswordFailedLockout;
                            break;
                        case AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringRepudiationCeremony:
                            emailType = EmailType.Repudiation_KBQFailedLockout;
                            break;
                        case AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringSignatureCeremony:
                            emailType = EmailType.COR_KBQFailedLockout;
                            break;
                        default:
                            if (reason != AccountLockEvent.ManualAction)
                            {
                                emailType = EmailType.Profile_KBQFailedLockout;
                            }
                            break;
                    }

                    var emailEntry = _linkoExchangeEmaiService.GetEmailEntryForUser(user:user, emailType:emailType, contentReplacements:contentReplacements, orgRegProg:program);
                    emailEntries.Add(item:emailEntry);
                }
            }

            // Send emails
            _linkoExchangeEmaiService.SendEmails(emailEntries:emailEntries);
        }

        public AccountLockoutResultDto LockUnlockUserAccount(int targetOrgRegProgUserId, bool isAttemptingLock, AccountLockEvent reason, bool isAuthorizationRequired = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:targetOrgRegProgUserId))
            {
                throw new UnauthorizedAccessException();
            }

            var userProfileId = _dbContext.OrganizationRegulatoryProgramUsers
                                          .Single(orpu => orpu.OrganizationRegulatoryProgramUserId == targetOrgRegProgUserId)
                                          .UserProfileId;

            return LockUnlockUserAccount(userProfileId:userProfileId, isAttemptingLock:isAttemptingLock, reason:reason, reportPackageId:null);
        }

        public AccountLockoutResultDto LockUnlockUserAccount(int userProfileId, bool isAttemptingLock, AccountLockEvent reason, int? reportPackageId = null)
        {
            var user = _dbContext.Users.Single(u => u.UserProfileId == userProfileId);

            //Check user is not support role
            if (user.IsInternalAccount && isAttemptingLock && reason == AccountLockEvent.ManualAction)
            {
                return new AccountLockoutResultDto
                       {
                           IsSuccess = false,
                           FailureReason = AccountLockoutFailureReason.CannotLockoutSupportRole
                       };
            }

            //Check user is not THIS user's own account
            var thisUserProfileId = _httpContext.CurrentUserProfileId();
            if (thisUserProfileId > 0 && userProfileId == thisUserProfileId && reason == AccountLockEvent.ManualAction)
            {
                return new AccountLockoutResultDto
                       {
                           IsSuccess = false,
                           FailureReason = AccountLockoutFailureReason.CannotLockoutOwnAccount
                       };
            }

            user.IsAccountLocked = isAttemptingLock;

            try
            {
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
                        var message = $"Error '{subItem.ErrorMessage}' occurred in {entityTypeName} at {subItem.PropertyName}";
                        validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                    }
                }

                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }

            if (isAttemptingLock)
            {
                SendAccountLockoutEmails(user:user, reason:reason);
            }

            LogLockUnlockActivityToCromerr(isAttemptingLock:isAttemptingLock, user:user, reason:reason, reportPackageId:reportPackageId);

            //Success
            return new AccountLockoutResultDto
                   {
                       IsSuccess = true
                   };
        }

        private void LogLockUnlockActivityToCromerr(bool isAttemptingLock, UserProfile user, AccountLockEvent reason, int? reportPackageId = null)
        {
            CromerrEvent cromerrEvent;
            if (!isAttemptingLock)
            {
                cromerrEvent = CromerrEvent.UserAccess_ManualAccountUnlock;
            }
            else if (reason == AccountLockEvent.ManualAction)
            {
                cromerrEvent = CromerrEvent.UserAccess_ManualAccountLock;
            }
            else if (reason == AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringPasswordReset)
            {
                cromerrEvent = CromerrEvent.ForgotPassword_AccountLocked;
            }
            else if (reason == AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringProfileAccess)
            {
                cromerrEvent = CromerrEvent.Profile_AccountLocked;
            }
            else if (reason == AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringSignatureCeremony)
            {
                cromerrEvent = CromerrEvent.Signature_AccountLockedKBQSigning;
            }
            else if (reason == AccountLockEvent.ExceededPasswordMaxAttemptsDuringSignatureCeremony)
            {
                cromerrEvent = CromerrEvent.Signature_AccountLockedPasswordSigning;
            }
            else if (reason == AccountLockEvent.ExceededPasswordMaxAttemptsDuringRepudiationCeremony)
            {
                cromerrEvent = CromerrEvent.Signature_AccountLockedPasswordRepudiation;
            }
            else if (reason == AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringRepudiationCeremony)
            {
                cromerrEvent = CromerrEvent.Signature_AccountLockedKBQRepudiation;
            }
            else
            {
                _logService.Info(message:$"LogLockUnlockActivityToCromerr. isAttemptingLock={isAttemptingLock}, reason={reason}, "
                                         + $"Cannot associate a CromerrEvent with reason provided.");
                return;
            }

            //Log Cromerr
            var programUsers = _dbContext.OrganizationRegulatoryProgramUsers.Where(u => u.UserProfileId == user.UserProfileId);
            foreach (var programUser in programUsers.ToList())
            {
                int thisUserOrgRegProgUserId;
                if (cromerrEvent == CromerrEvent.UserAccess_ManualAccountLock
                    || cromerrEvent == CromerrEvent.UserAccess_ManualAccountUnlock)
                {
                    //Different actor user
                    thisUserOrgRegProgUserId = Convert.ToInt32(value:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));
                }
                else
                {
                    //User did this to herself
                    thisUserOrgRegProgUserId = programUser.OrganizationRegulatoryProgramUserId;
                }

                var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                                 .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
                var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:actorProgramUser);
                var actorUser = GetUserProfileById(userProfileId:actorProgramUserDto.UserProfileId);

                var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
                cromerrAuditLogEntryDto.RegulatoryProgramId = programUser.OrganizationRegulatoryProgram.RegulatoryProgramId;
                cromerrAuditLogEntryDto.OrganizationId = programUser.OrganizationRegulatoryProgram.OrganizationId;
                cromerrAuditLogEntryDto.RegulatorOrganizationId = programUser.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                cromerrAuditLogEntryDto.UserProfileId = programUser.UserProfileId;
                cromerrAuditLogEntryDto.UserName = user.UserName;
                cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
                cromerrAuditLogEntryDto.UserLastName = user.LastName;
                cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
                cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();

                var contentReplacements = new Dictionary<string, string>
                                          {
                                              {"organizationName", programUser.OrganizationRegulatoryProgram.Organization.Name},
                                              {"firstName", user.FirstName},
                                              {"lastName", user.LastName},
                                              {"userName", user.UserName},
                                              {"emailAddress", user.Email}
                                          };

                if (!isAttemptingLock || reason == AccountLockEvent.ManualAction)
                {
                    contentReplacements.Add(key:"authorityName", value:actorProgramUserDto.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName);
                    contentReplacements.Add(key:"authorityUserName", value:actorUser.UserName);
                    contentReplacements.Add(key:"authorityFirstName", value:actorUser.FirstName);
                    contentReplacements.Add(key:"authorityLastName", value:actorUser.LastName);
                    contentReplacements.Add(key:"authorityEmailaddress", value:actorUser.Email);
                }

                if (reason == AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringSignatureCeremony
                    || reason == AccountLockEvent.ExceededPasswordMaxAttemptsDuringSignatureCeremony
                    || reason == AccountLockEvent.ExceededKBQMaxAnswerAttemptsDuringRepudiationCeremony
                    || reason == AccountLockEvent.ExceededPasswordMaxAttemptsDuringRepudiationCeremony)
                {
                    if (reportPackageId.HasValue)
                    {
                        var reportPackage = _dbContext.ReportPackages
                                                      .Single(rp => rp.ReportPackageId == reportPackageId.Value);
                        var periodStartDateLocal = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:reportPackage.PeriodStartDateTimeUtc.UtcDateTime,
                                                                                                         orgRegProgramId:programUser.OrganizationRegulatoryProgramId);
                        var periodEndDateLocal = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:reportPackage.PeriodEndDateTimeUtc.UtcDateTime,
                                                                                                       orgRegProgramId:programUser.OrganizationRegulatoryProgramId);
                        if (contentReplacements.Keys.Contains(value:"organizationName"))
                        {
                            contentReplacements.Remove(key:"organizationName");
                        }
                        contentReplacements.Add(key:"organizationName", value:reportPackage.OrganizationName);
                        contentReplacements.Add(key:"reportPackageName", value:reportPackage.Name);
                        contentReplacements.Add(key:"periodStartDate", value:periodStartDateLocal.ToString(format:"MMM dd, yyyy"));
                        contentReplacements.Add(key:"periodEndDate", value:periodEndDateLocal.ToString(format:"MMM dd, yyyy"));
                    }
                    else
                    {
                        throw new Exception(message:$"ERROR: Account lock Cromerr log entry could not be completed without report package Id");
                    }
                }

                _crommerAuditLogService.Log(eventType:cromerrEvent, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);
            }
        }

        public void EnableDisableUserAccount(int orgRegProgramUserId, bool isAttemptingDisable, bool isAuthorizationRequired = false)
        {
            _logService.Info(message:$"EnableDisableUserAccount. OrgRegProgUserId={orgRegProgramUserId}, IsAttemptingDisable={isAttemptingDisable}...");

            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgramUserId))
            {
                throw new UnauthorizedAccessException();
            }

            //Check user is not THIS user's own account
            var thisUserOrgRegProgUserId = Convert.ToInt32(value:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));
            if (orgRegProgramUserId == thisUserOrgRegProgUserId)
            {
                _logService.Info(message:$"EnableDisableUserAccount. OrgRegProgUserId={orgRegProgramUserId}, IsAttemptingDisable={isAttemptingDisable}... CannotUpdateOwnAccount.");

                var validationIssues = new List<RuleViolation>();
                var message = "User cannot update own account.";
                validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
            }

            if (!isAttemptingDisable)
            {
                //Check user license count
                //
                //For Authority or Industry?
                var orgRegProgramId = _dbContext.OrganizationRegulatoryProgramUsers
                                                .Single(o => o.OrganizationRegulatoryProgramUserId == orgRegProgramUserId).OrganizationRegulatoryProgramId;
                var remainingUsersAllowed = _orgService.GetRemainingUserLicenseCount(orgRegProgramId:orgRegProgramId);
                if (remainingUsersAllowed < 1)
                {
                    _logService.Info(message:$"EnableDisableUserAccount. OrgRegProgUserId={orgRegProgramUserId}, IsAttemptingDisable={isAttemptingDisable}... "
                                             + $"NoMoreRemainingUserLicenses.");

                    var validationIssues = new List<RuleViolation>();
                    var message = "No more User Licenses are available for this organization. Disable another User and try again.";
                    validationIssues.Add(item:new RuleViolation(propertyName:string.Empty, propertyValue:null, errorMessage:message));
                    throw new RuleViolationException(message:"Validation errors", validationIssues:validationIssues);
                }
            }

            var programUser = _dbContext.OrganizationRegulatoryProgramUsers
                                        .Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgramUserId);
            programUser.IsEnabled = !isAttemptingDisable;
            _dbContext.SaveChanges();

            //Log Cromerr
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                             .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:actorProgramUser);
            var actorUser = GetUserProfileById(userProfileId:actorProgramUserDto.UserProfileId);

            var userDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:programUser);
            var user = GetUserProfileById(userProfileId:userDto.UserProfileId);
            var cromerrEvent = isAttemptingDisable ? CromerrEvent.UserAccess_Disabled : CromerrEvent.UserAccess_Enabled;
            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                          {
                                              RegulatoryProgramId = programUser.OrganizationRegulatoryProgram.RegulatoryProgramId,
                                              OrganizationId = programUser.OrganizationRegulatoryProgram.OrganizationId
                                          };
            cromerrAuditLogEntryDto.RegulatorOrganizationId = programUser.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = programUser.UserProfileId;
            cromerrAuditLogEntryDto.UserName = user.UserName;
            cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
            cromerrAuditLogEntryDto.UserLastName = user.LastName;
            cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
            cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
            var contentReplacements = new Dictionary<string, string>
                                      {
                                          {"organizationName", programUser.OrganizationRegulatoryProgram.Organization.Name},
                                          {"firstName", user.FirstName},
                                          {"lastName", user.LastName},
                                          {"userName", user.UserName},
                                          {"emailAddress", user.Email},
                                          {"actorFirstName", actorUser.FirstName},
                                          {"actorLastName", actorUser.LastName},
                                          {"actorUserName", actorUser.UserName}
                                      };

            _crommerAuditLogService.Log(eventType:cromerrEvent, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);

            _logService.Info(message:$"EnableDisableUserAccount. OrgRegProgUserId={orgRegProgramUserId}, IsAttemptingDisable={isAttemptingDisable}... Success.");
        }

        public void SetHashedPassword(int userProfileId, string passwordHash)
        {
            var user = _dbContext.Users.Single(u => u.UserProfileId == userProfileId);
            user.PasswordHash = passwordHash;
            _dbContext.SaveChanges();
        }

        public bool RemoveUser(int orgRegProgUserId, bool isAuthorizationRequired = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgUserId))
            {
                throw new UnauthorizedAccessException();
            }

            //Ensure this is not the calling User's account
            var thisUsersOrgRegProgUserId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));
            if (thisUsersOrgRegProgUserId == orgRegProgUserId)
            {
                return false;
            }

            var user = _dbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);

            //Set applicable flags
            user.IsRemoved = true;
            user.IsRegistrationApproved = false;
            user.IsEnabled = false;
            user.IsRegistrationDenied = false;

            //Persist modification date and modifier actor
            user.LastModificationDateTimeUtc = DateTimeOffset.Now;
            user.LastModifierUserId = Convert.ToInt32(value:_httpContext.CurrentUserProfileId());
            _dbContext.SaveChanges();

            //Log Cromerr
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                             .Single(u => u.OrganizationRegulatoryProgramUserId == thisUsersOrgRegProgUserId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:actorProgramUser);
            var actorUser = GetUserProfileById(userProfileId:actorProgramUserDto.UserProfileId);

            var userDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:user);
            userDto.UserProfileDto = GetUserProfileById(userProfileId:user.UserProfileId);
            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                          {
                                              RegulatoryProgramId = user.OrganizationRegulatoryProgram.RegulatoryProgramId,
                                              OrganizationId = user.OrganizationRegulatoryProgram.OrganizationId
                                          };
            cromerrAuditLogEntryDto.RegulatorOrganizationId = user.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = user.UserProfileId;
            cromerrAuditLogEntryDto.UserName = userDto.UserProfileDto.UserName;
            cromerrAuditLogEntryDto.UserFirstName = userDto.UserProfileDto.FirstName;
            cromerrAuditLogEntryDto.UserLastName = userDto.UserProfileDto.LastName;
            cromerrAuditLogEntryDto.UserEmailAddress = userDto.UserProfileDto.Email;
            cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
            var contentReplacements = new Dictionary<string, string>
                                      {
                                          {"organizationName", user.OrganizationRegulatoryProgram.Organization.Name},
                                          {"firstName", userDto.UserProfileDto.FirstName},
                                          {"lastName", userDto.UserProfileDto.LastName},
                                          {"userName", userDto.UserProfileDto.UserName},
                                          {"emailAddress", userDto.UserProfileDto.Email},
                                          {"actorFirstName", actorUser.FirstName},
                                          {"actorLastName", actorUser.LastName},
                                          {"actorUserName", actorUser.UserName}
                                      };

            _crommerAuditLogService.Log(eventType:CromerrEvent.UserAccess_Removed, dto:cromerrAuditLogEntryDto, contentReplacements:contentReplacements);

            return true;
        }

        private void UpdateUser(UserDto dto)
        {
            var userProfile = _dbContext.Users.Single(up => up.UserProfileId == dto.UserProfileId);
            userProfile = _mapHelper.GetUserProfileFromUserDto(dto:dto, userProfile:userProfile);

            _dbContext.SaveChanges();
        }

        public void UpdateProfile(UserDto dto)
        {
            UpdateUser(dto:dto);

            //Send email
            var contentReplacements = new Dictionary<string, string>();
            var supportPhoneNumber = _globalSettings[key:SystemSettingType.SupportPhoneNumber];
            var supportEmail = _globalSettings[key:SystemSettingType.SupportEmailAddress];

            var authorityList = _orgService.GetUserAuthorityListForEmailContent(userProfileId:dto.UserProfileId);
            contentReplacements.Add(key:"firstName", value:dto.FirstName);
            contentReplacements.Add(key:"lastName", value:dto.LastName);
            contentReplacements.Add(key:"authorityList", value:authorityList);
            contentReplacements.Add(key:"supportPhoneNumber", value:supportPhoneNumber);
            contentReplacements.Add(key:"supportEmail", value:supportEmail);
            var emailEntries = _linkoExchangeEmaiService.GetAllProgramEmailEntiresForUser(user:dto, emailType:EmailType.Profile_ProfileChanged,
                                                                                          contentReplacements:contentReplacements);
            _linkoExchangeEmaiService.SendEmails(emailEntries:emailEntries);
        }

        public RegistrationResult ValidateUserProfileData(UserDto userProfile)
        {
            if (userProfile == null
                || string.IsNullOrWhiteSpace(value:userProfile.FirstName)
                || string.IsNullOrWhiteSpace(value:userProfile.LastName)
                || string.IsNullOrWhiteSpace(value:userProfile.AddressLine1)
                || string.IsNullOrWhiteSpace(value:userProfile.CityName)
                || string.IsNullOrWhiteSpace(value:userProfile.ZipCode)
                || string.IsNullOrWhiteSpace(value:userProfile.Email)
                || string.IsNullOrWhiteSpace(value:userProfile.UserName)
            )
            {
                return RegistrationResult.BadUserProfileData;
            }

            return RegistrationResult.Success;
        }

        public RegistrationResult ValidateRegistrationUserData(UserDto userProfile, IEnumerable<AnswerDto> securityQuestions, IEnumerable<AnswerDto> kbqQuestions)
        {
            if (userProfile == null
                || string.IsNullOrWhiteSpace(value:userProfile.FirstName)
                || string.IsNullOrWhiteSpace(value:userProfile.LastName)
                || string.IsNullOrWhiteSpace(value:userProfile.AddressLine1)
                || string.IsNullOrWhiteSpace(value:userProfile.CityName)
                || string.IsNullOrWhiteSpace(value:userProfile.ZipCode)
                || string.IsNullOrWhiteSpace(value:userProfile.Email)
                || string.IsNullOrWhiteSpace(value:userProfile.UserName)
                || securityQuestions == null
                || kbqQuestions == null)
            {
                return RegistrationResult.BadUserProfileData;
            }

            if (securityQuestions.Count() < 2)
            {
                return RegistrationResult.MissingSecurityQuestion;
            }

            if (kbqQuestions.Count() < 5)
            {
                return RegistrationResult.MissingKBQ;
            }

            // Test duplicated security questions
            if (securityQuestions.GroupBy(i => i.QuestionId).Any(i => i.Count() > 1))
            {
                return RegistrationResult.DuplicatedSecurityQuestion;
            }

            // Test duplicated KBQ questions
            if (kbqQuestions.GroupBy(i => i.QuestionId).Any(i => i.Count() > 1))
            {
                return RegistrationResult.DuplicatedKBQ;
            }

            // Test duplicated security question answers
            if (securityQuestions.GroupBy(i => i.Content).Any(i => i.Count() > 1))
            {
                return RegistrationResult.DuplicatedSecurityQuestionAnswer;
            }

            // Test duplicated KBQ question answers
            if (kbqQuestions.GroupBy(i => i.Content).Any(i => i.Count() > 1))
            {
                return RegistrationResult.DuplicatedKBQAnswer;
            }

            // Test security questions must have answer
            if (securityQuestions.Any(i => i.Content == null))
            {
                return RegistrationResult.MissingSecurityQuestionAnswer;
            }

            // Test KBQ questions must have answer
            if (kbqQuestions.Any(i => i.Content == null))
            {
                return RegistrationResult.MissingKBQAnswer;
            }

            return RegistrationResult.Success;
        }

        public ICollection<RegistrationResult> KbqValidation(IEnumerable<AnswerDto> kbqQuestions)
        {
            var result = new List<RegistrationResult>();

            if (kbqQuestions.Count() < 5)
            {
                result.Add(item:RegistrationResult.MissingKBQ);
            }

            // Test duplicated KBQ questions
            if (kbqQuestions.GroupBy(i => i.QuestionId).Any(i => i.Count() > 1))
            {
                result.Add(item:RegistrationResult.DuplicatedKBQ);
            }

            // Test duplicated KBQ question answers
            if (kbqQuestions.GroupBy(i => i.Content).Any(i => i.Count() > 1))
            {
                result.Add(item:RegistrationResult.DuplicatedKBQAnswer);
            }

            // Test KBQ questions must have answer
            if (kbqQuestions.Any(i => i.Content == null))
            {
                result.Add(item:RegistrationResult.MissingKBQAnswer);
            }

            return result;
        }

        public ICollection<RegistrationResult> SecurityValidation(IEnumerable<AnswerDto> securityQuestions)
        {
            var result = new List<RegistrationResult>();

            if (securityQuestions.Count() < 2)
            {
                result.Add(item:RegistrationResult.MissingSecurityQuestion);
            }

            // Test duplicated security questions
            if (securityQuestions.GroupBy(i => i.QuestionId).Any(i => i.Count() > 1))
            {
                result.Add(item:RegistrationResult.DuplicatedSecurityQuestion);
            }

            // Test duplicated security question answers
            if (securityQuestions.GroupBy(i => i.Content).Any(i => i.Count() > 1))
            {
                result.Add(item:RegistrationResult.DuplicatedSecurityQuestionAnswer);
            }

            // Test security questions must have answer
            if (securityQuestions.Any(i => i.Content == null))
            {
                result.Add(item:RegistrationResult.MissingSecurityQuestionAnswer);
            }

            return result;
        }

        public bool UpdateEmail(int userProfileId, string newEmailAddress)
        {
            UserProfile userProfile;
            string oldEmailAddress;

            //Check if email in use
            var dbContextTransaction = _dbContext.Database.BeginTransaction(isolationLevel:IsolationLevel.RepeatableRead);
            try
            {
                var isExistsAlready = _dbContext.Users.Any(u => u.Email == newEmailAddress);

                if (isExistsAlready)
                {
                    return false;
                }

                userProfile = _dbContext.Users.Single(up => up.UserProfileId == userProfileId);
                oldEmailAddress = userProfile.Email;
                userProfile.Email = newEmailAddress;
                userProfile.OldEmailAddress = oldEmailAddress;
                _dbContext.SaveChanges();
                dbContextTransaction.Commit();
            }
            catch (Exception)
            {
                // TODO:need to log the exception

                dbContextTransaction.Rollback();
                return false;
            }
            finally
            {
                dbContextTransaction.Dispose();
            }

            //Need to log this activity for all Org Reg Program users
            var orgRegProgramUsers = _dbContext.OrganizationRegulatoryProgramUsers
                                               .Include(orpu => orpu.OrganizationRegulatoryProgram)
                                               .Where(orpu => orpu.UserProfileId == userProfileId);

            foreach (var orgRegProgramUser in orgRegProgramUsers)
            {
                var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                              {
                                                  RegulatoryProgramId = orgRegProgramUser.OrganizationRegulatoryProgram.RegulatoryProgramId,
                                                  OrganizationId = orgRegProgramUser.OrganizationRegulatoryProgram.OrganizationId
                                              };
                cromerrAuditLogEntryDto.RegulatorOrganizationId = orgRegProgramUser.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                cromerrAuditLogEntryDto.UserProfileId = userProfile.UserProfileId;
                cromerrAuditLogEntryDto.UserName = userProfile.UserName;
                cromerrAuditLogEntryDto.UserFirstName = userProfile.FirstName;
                cromerrAuditLogEntryDto.UserLastName = userProfile.LastName;
                cromerrAuditLogEntryDto.UserEmailAddress = newEmailAddress;
                cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();

                var cromerrContentReplacements = new Dictionary<string, string>
                                                 {
                                                     {"firstName", userProfile.FirstName},
                                                     {"lastName", userProfile.LastName},
                                                     {"userName", userProfile.UserName},
                                                     {"oldEmail", oldEmailAddress},
                                                     {"newEmail", newEmailAddress},
                                                     {"emailAddress", newEmailAddress}
                                                 };

                _crommerAuditLogService.Log(eventType:CromerrEvent.Profile_EmailChanged, dto:cromerrAuditLogEntryDto, contentReplacements:cromerrContentReplacements);
            }

            //Send emails (to old and new address)
            var supportPhoneNumber = _globalSettings[key:SystemSettingType.SupportPhoneNumber];
            var supportEmail = _globalSettings[key:SystemSettingType.SupportEmailAddress];
            var authorityList = _orgService.GetUserAuthorityListForEmailContent(userProfileId:userProfileId);

            var contentReplacements = new Dictionary<string, string>
                                      {
                                          {"firstName", userProfile.FirstName},
                                          {"lastName", userProfile.LastName},
                                          {"oldEmail", oldEmailAddress},
                                          {"newEmail", newEmailAddress},
                                          {"authorityList", authorityList},
                                          {"supportPhoneNumber", supportPhoneNumber},
                                          {"supportEmail", supportEmail}
                                      };

            var emailEntries =
                _linkoExchangeEmaiService.GetAllProgramEmailEntiresForUser(userProfile:userProfile, emailType:EmailType.Profile_EmailChanged,
                                                                           contentReplacements:contentReplacements).ToList();
            var emailEntriesForOldEmailAddress = emailEntries.Select(i => i.Clone(overrideEmailAddress:userProfile.OldEmailAddress)).ToList();
            emailEntries.AddRange(collection:emailEntriesForOldEmailAddress);

            //Send out emails;
            _linkoExchangeEmaiService.SendEmails(emailEntries:emailEntries);
            return true;
        }

        public OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUser(int orgRegProgUserId, bool isAuthorizationRequired = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgUserId))
            {
                throw new UnauthorizedAccessException();
            }

            var currentOrgRegProgramId = int.Parse(s:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramId));
            var user = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            var userDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:user);
            userDto.UserProfileDto = GetUserProfileById(userProfileId:user.UserProfileId);
            var jurisdiction = _dbContext.Jurisdictions.Single(j => j.JurisdictionId == userDto.UserProfileDto.JurisdictionId);
            userDto.UserProfileDto.Jurisdiction = _mapHelper.GetJurisdictionDtoFromJurisdiction(jurisdiction:jurisdiction);

            //Need to modify datetime to local
            userDto.UserProfileDto.CreationDateTimeUtc =
                _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:userDto.UserProfileDto.CreationDateTimeUtc.Value.UtcDateTime,
                                                                      orgRegProgramId:currentOrgRegProgramId);
            userDto.RegistrationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(utcDateTime:userDto.RegistrationDateTimeUtc.Value.UtcDateTime,
                                                                                                    orgRegProgramId:currentOrgRegProgramId);

            return userDto;
        }

        public void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int orgRegProgUserId, bool isApproved, bool isSignatory)
        {
            var user = _dbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (user == null)
            {
                return;
            }

            user.IsRegistrationApproved = isApproved;
            user.IsRegistrationDenied = !isApproved;

            if (isApproved)
            {
                // Call UpdateUserSignatoryStatus to set IsSignatory flag to avoid duplicated code for emails and Cromerr
                UpdateUserSignatoryStatus(orgRegProgUserId:orgRegProgUserId, isSignatory:isSignatory, isAuthorizationRequired:true);
            }
            _dbContext.SaveChanges();
        }

        public void UpdateOrganizationRegulatoryProgramUserRole(int orgRegProgUserId, int permissionGroupId)
        {
            var user = _dbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (user == null)
            {
                return;
            }

            user.PermissionGroupId = permissionGroupId;
            _dbContext.SaveChanges();
        }

        public RegistrationResultDto ApprovePendingRegistration(
            int orgRegProgUserId,
            int permissionGroupId,
            bool isApproved,
            bool isAuthorizationRequired = false,
            bool isSignatory = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgUserId))
            {
                throw new UnauthorizedAccessException();
            }

            var programUser = GetOrganizationRegulatoryProgramUser(orgRegProgUserId:orgRegProgUserId);
            var orgRegProgramId = programUser.OrganizationRegulatoryProgramId;
            var authority = _orgService.GetAuthority(orgRegProgramId:orgRegProgramId);
            var isAuthorityUser = !(authority != null && authority.OrganizationRegulatoryProgramId != orgRegProgramId);

            //Check user license count
            if (isApproved)
            {
                var remainingUserLicenseCount = _orgService.GetRemainingUserLicenseCount(orgRegProgramId:orgRegProgramId);
                if (remainingUserLicenseCount < 1)
                {
                    //ACTION BLOCKED -- NO MORE USER LICENSES
                    return isAuthorityUser
                               ? new RegistrationResultDto {Result = RegistrationResult.NoMoreUserLicensesForAuthority}
                               : new RegistrationResultDto {Result = RegistrationResult.NoMoreUserLicensesForIndustry};
                }
            }

            var transaction = _dbContext.BeginTransaction();
            try
            {
                if (isAuthorityUser)
                {
                    isSignatory = false;
                }

                //Only update Role if we are Approving
                if (isApproved)
                {
                    UpdateOrganizationRegulatoryProgramUserRole(orgRegProgUserId:orgRegProgUserId, permissionGroupId:permissionGroupId);
                }

                UpdateOrganizationRegulatoryProgramUserApprovedStatus(orgRegProgUserId:orgRegProgUserId, isApproved:isApproved, isSignatory:isSignatory);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                transaction.Dispose();
            }

            //Send email(s)
            //
            EmailType emailType;
            CromerrEvent cromerrEvent;
            if (isApproved)
            {
                //Authority or Industry?
                emailType = isAuthorityUser ? EmailType.Registration_AuthorityRegistrationApproved : EmailType.Registration_IndustryRegistrationApproved;
                cromerrEvent = CromerrEvent.Registration_RegistrationApproved;
            }
            else
            {
                //Authority or Industry?
                emailType = isAuthorityUser ? EmailType.Registration_AuthorityRegistrationDenied : EmailType.Registration_IndustryRegistrationDenied;
                cromerrEvent = CromerrEvent.Registration_RegistrationDenied;
            }

            //Cromerr log
            var thisUserOrgRegProgUserId = Convert.ToInt32(value:_httpContext.GetClaimValue(claimType:CacheKey.OrganizationRegulatoryProgramUserId));
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                                             .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user:actorProgramUser);
            var actorUser = GetUserProfileById(userProfileId:actorProgramUserDto.UserProfileId);

            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto
                                          {
                                              RegulatoryProgramId = programUser.OrganizationRegulatoryProgramDto.RegulatoryProgramId,
                                              OrganizationId = programUser.OrganizationRegulatoryProgramDto.OrganizationId
                                          };
            cromerrAuditLogEntryDto.RegulatorOrganizationId = programUser.OrganizationRegulatoryProgramDto.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = programUser.UserProfileId;
            cromerrAuditLogEntryDto.UserName = programUser.UserProfileDto.UserName;
            cromerrAuditLogEntryDto.UserFirstName = programUser.UserProfileDto.FirstName;
            cromerrAuditLogEntryDto.UserLastName = programUser.UserProfileDto.LastName;
            cromerrAuditLogEntryDto.UserEmailAddress = programUser.UserProfileDto.Email;
            cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();

            var cromerrContentReplacements = new Dictionary<string, string>
                                             {
                                                 {"authorityName", authority.OrganizationDto.OrganizationName},
                                                 {"organizationName", programUser.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName},
                                                 {"regulatoryProgram", authority.RegulatoryProgramDto.Name},
                                                 {"firstName", programUser.UserProfileDto.FirstName},
                                                 {"lastName", programUser.UserProfileDto.LastName},
                                                 {"userName", programUser.UserProfileDto.UserName},
                                                 {"emailAddress", programUser.UserProfileDto.Email},
                                                 {"actorFirstName", actorUser.FirstName},
                                                 {"actorLastName", actorUser.LastName},
                                                 {"actorUserName", actorUser.UserName},
                                                 {"actorEmailAddress", actorUser.Email}
                                             };

            _crommerAuditLogService.Log(eventType:cromerrEvent, dto:cromerrAuditLogEntryDto, contentReplacements:cromerrContentReplacements);

            // Sending emails
            var contentReplacements = new Dictionary<string, string>();
            var authorityName =
                _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId, settingType:SettingType.EmailContactInfoName);
            var authorityPhoneNumber = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                                    settingType:SettingType.EmailContactInfoPhone);
            var authorityEmail = _settingService.GetOrgRegProgramSettingValue(orgRegProgramId:authority.OrganizationRegulatoryProgramId,
                                                                              settingType:SettingType.EmailContactInfoEmailAddress);

            contentReplacements.Add(key:"firstName", value:programUser.UserProfileDto.FirstName);
            contentReplacements.Add(key:"lastName", value:programUser.UserProfileDto.LastName);
            contentReplacements.Add(key:"authorityName", value:authorityName);
            contentReplacements.Add(key:"authorityOrganizationName", value:authority.OrganizationDto.OrganizationName);
            contentReplacements.Add(key:"phoneNumber", value:authorityPhoneNumber);
            contentReplacements.Add(key:"emailAddress", value:authorityEmail);
            contentReplacements.Add(key:"supportPhoneNumber", value:authorityPhoneNumber);
            contentReplacements.Add(key:"supportEmail", value:authorityEmail);

            if (!isAuthorityUser)
            {
                var org = _dbContext.Organizations.Single(o => o.OrganizationId == programUser.OrganizationRegulatoryProgramDto.OrganizationId);

                contentReplacements.Add(key:"organizationName", value:org.Name);
                contentReplacements.Add(key:"addressLine1", value:org.AddressLine1);
                contentReplacements.Add(key:"cityName", value:org.CityName);
                contentReplacements.Add(key:"stateName", value:org.JurisdictionId.HasValue
                                                                   ? _dbContext.Jurisdictions.Single(j => j.JurisdictionId == org.JurisdictionId.Value).Code
                                                                   : "");
            }

            if (isApproved)
            {
                var baseUrl = _httpContext.GetRequestBaseUrl();
                var link = baseUrl + "Account/SignIn";
                contentReplacements.Add(key:"link", value:link);
            }

            _linkoExchangeEmaiService.SendEmails(emailEntries:
                                                 new List<EmailEntry>
                                                 {
                                                     _linkoExchangeEmaiService.GetEmailEntryForOrgRegProgramUser(user:programUser, emailType:emailType,
                                                                                                                 contentReplacements:contentReplacements)
                                                 });

            return new RegistrationResultDto {Result = RegistrationResult.Success};
        }

        public ICollection<UserDto> GetOrgRegProgSignators(int orgRegProgId)
        {
            var signatoryIds = _dbContext.OrganizationRegulatoryProgramUsers
                                         .Where(i => i.OrganizationRegulatoryProgramId == orgRegProgId
                                                     && i.IsSignatory
                                                     && i.IsEnabled
                                                     && !i.IsRemoved
                                                     && !i.IsRegistrationDenied
                                                     && i.IsRegistrationApproved)
                                         .Select(b => b.UserProfileId);

            // ReSharper disable once ArgumentsStyleNamedExpression
            var userProfiles = _dbContext.Users.Where(i => i.IsAccountLocked == false && i.IsAccountResetRequired == false && signatoryIds.Contains(i.UserProfileId));

            var userDtos = new List<UserDto>();
            foreach (var userProfile in userProfiles)
            {
                var userDto = _mapHelper.GetUserDtoFromUserProfile(userProfile:userProfile);
                userDtos.Add(item:userDto);
            }

            return userDtos;
        }

        public ICollection<UserDto> GetAuthorityAdministratorAndStandardUsers(int authorityOrganizationId)
        {
            var userIds = _dbContext.OrganizationRegulatoryProgramUsers
                                    .Where(i => i.OrganizationRegulatoryProgram.OrganizationId == authorityOrganizationId
                                                && i.OrganizationRegulatoryProgram.Organization.OrganizationType.Name == OrganizationTypeName.Authority.ToString()
                                                && i.IsEnabled
                                                && !i.IsRemoved
                                                && !i.IsRegistrationDenied
                                                && i.IsRegistrationApproved
                                                && (i.PermissionGroup.Name == PermissionGroupName.Administrator.ToString()
                                                    || i.PermissionGroup.Name == PermissionGroupName.Standard.ToString())
                                          ).Select(i => i.UserProfileId);

            // ReSharper disable once ArgumentsStyleNamedExpression
            var userProfiles = _dbContext.Users.Where(i => i.IsAccountLocked == false && i.IsAccountResetRequired == false && userIds.Contains(i.UserProfileId));
            var userDtos = new List<UserDto>();
            foreach (var userProfile in userProfiles)
            {
                var userDto = _mapHelper.GetUserDtoFromUserProfile(userProfile:userProfile);
                userDtos.Add(item:userDto);
            }

            return userDtos;

            //var userProfiles = _dbContext.Users.Where(i => userIds.Contains(i.UserProfileId));
            //return userProfiles.Select(a => _mapHelper.GetUserDtoFromUserProfile(a)).ToList();
        }

        #endregion
    }
}