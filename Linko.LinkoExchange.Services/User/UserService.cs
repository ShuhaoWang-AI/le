using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Microsoft.AspNet.Identity;
using Linko.LinkoExchange.Services.QuestionAnswer;
using System.Data.Entity.Validation;
using Linko.LinkoExchange.Core.Validation;
using System.Data.Entity.Infrastructure;
using NLog;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.AuditLog;
using System.Data.Entity;
using System.Runtime.CompilerServices; 

namespace Linko.LinkoExchange.Services.User
{
    public class UserService : BaseService, IUserService
    {
        #region private members

        private readonly LinkoExchangeContext _dbContext;
        private readonly IAuditLogEntry _logger;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IHttpContextService _httpContext;
        private readonly IEmailService _emailService;
        private readonly ISettingService _settingService;
        private readonly ISessionCache _sessionCache;
        private readonly IOrganizationService _orgService;
        private readonly IRequestCache _requestCache;
        private readonly IDictionary<SystemSettingType, string> _globalSettings;
        private readonly ITimeZoneService _timeZones;
        private readonly IQuestionAnswerService _questionAnswerServices;
        private readonly ILogger _logService;
        private readonly IMapHelper _mapHelper;
        private readonly ICromerrAuditLogService _crommerAuditLogService;
        #endregion

        #region constructor

        public UserService(LinkoExchangeContext dbContext, IAuditLogEntry logger,
            IPasswordHasher passwordHasher, IHttpContextService httpContext,
            IEmailService emailService, ISettingService settingService, ISessionCache sessionCache,
            IOrganizationService orgService, IRequestCache requestCache, ITimeZoneService timeZones,
             IQuestionAnswerService questionAnswerServices, ILogger logService, IMapHelper mapHelper,
             ICromerrAuditLogService crommerAuditLogService)
        {
            this._dbContext = dbContext;
            _logger = logger;
            _passwordHasher = passwordHasher;
            _httpContext = httpContext;
            _emailService = emailService;
            _settingService = settingService;
            _sessionCache = sessionCache;
            _orgService = orgService;
            _requestCache = requestCache;
            _globalSettings = _settingService.GetGlobalSettings();
            _timeZones = timeZones;
            _questionAnswerServices = questionAnswerServices;
            _logService = logService;
            _mapHelper = mapHelper;
            _crommerAuditLogService = crommerAuditLogService; 
        }

        #endregion

        #region public methods

        public override bool CanUserExecuteApi([CallerMemberName] string apiName = "", params int[] id)
        {
            bool retVal = false;

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var currentOrgRegProgUserId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
            var currentOrganizationId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationId));
            var currentRegulatoryProgramId = _dbContext.OrganizationRegulatoryPrograms.Single(orp => orp.OrganizationRegulatoryProgramId == currentOrgRegProgramId).RegulatoryProgramId;
            var currentPortalName = _httpContext.GetClaimValue(CacheKey.PortalName);
            currentPortalName = string.IsNullOrWhiteSpace(value: currentPortalName) ? "" : currentPortalName.Trim().ToLower();

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

                        bool isCurrentUserAdminOfTargetUser = currentUsersPermissionGroup.Name.ToLower().StartsWith("admin")
                                        && currentUsersPermissionGroup.OrganizationRegulatoryProgramId == targetOrgRegProgramUser.OrganizationRegulatoryProgramId;

                        //
                        //Authorize the correct Authority
                        //

                        if (currentPortalName.Equals("authority"))
                        {
                            //this will also handle scenarios where the current user and target user of from the same org reg program
                            retVal = currentOrgRegProgramId == _orgService.GetAuthority(targetOrgRegProgramUser.OrganizationRegulatoryProgramId).OrganizationRegulatoryProgramId;

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

                default:

                    throw new Exception($"ERROR: Unhandled API authorization attempt using name = '{apiName}'");


            }

            return retVal;
        }

        public UserDto GetUserProfileById(int userProfileId)
        {
            var userProfile = _dbContext.Users.Single(up => up.UserProfileId == userProfileId);
            return _mapHelper.GetUserDtoFromUserProfile(userProfile);
        }

        public UserDto GetUserProfileByEmail(string emailAddress)
        {
            UserProfile userProfile = _dbContext.Users.SingleOrDefault(u => u.Email == emailAddress);
            if (userProfile != null)
            {
                return _mapHelper.GetUserDtoFromUserProfile(userProfile);
            }
            else
            {
                return null;
            }
        }

        public ICollection<OrganizationRegulatoryProgramUserDto> GetProgramUsersByEmail(string emailAddress)
        {
            var dtos = new List<OrganizationRegulatoryProgramUserDto>();
            UserProfile userProfile = _dbContext.Users.SingleOrDefault(u => u.Email == emailAddress && u.EmailConfirmed == true);
            if (userProfile != null)
            {
                var programUsers = _dbContext.OrganizationRegulatoryProgramUsers.Where(o => o.UserProfileId == userProfile.UserProfileId).ToList();
                if (programUsers != null)
                {
                    foreach (var programUser in programUsers)
                    {
                        var dto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(programUser);
                        //Need to map UserProfileDto manually here
                        dto.UserProfileDto = this.GetUserProfileById(programUser.UserProfileId);
                        dtos.Add(dto);


                    }
                }
            }

            return dtos;
        }

        public int GetPendingRegistrationProgramUsersCount(int orgRegProgramId)
        {
            return _dbContext.OrganizationRegulatoryProgramUsers
               .Count(u => u.InviterOrganizationRegulatoryProgramId == orgRegProgramId
               && u.IsRegistrationApproved == false && u.IsRegistrationDenied == false
               && u.IsRemoved == false);
        }

        public List<OrganizationRegulatoryProgramUserDto> GetPendingRegistrationProgramUsers(int orgRegProgramId)
        {
            var dtos = new List<OrganizationRegulatoryProgramUserDto>();

            var users = _dbContext.OrganizationRegulatoryProgramUsers
                .Where(u => u.InviterOrganizationRegulatoryProgramId == orgRegProgramId
                && u.IsRegistrationApproved == false && u.IsRegistrationDenied == false
                && u.IsRemoved == false);

            foreach (var user in users.ToList())
            {
                var dto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user);
                //manually map user profile child
                var userProfileDto = GetUserProfileById(user.UserProfileId);

                //Need to modify datetime to local
                dto.RegistrationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(dto.RegistrationDateTimeUtc.Value.UtcDateTime, orgRegProgramId);
                userProfileDto.CreationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(userProfileDto.CreationDateTimeUtc.Value.UtcDateTime, orgRegProgramId);

                dto.UserProfileDto = userProfileDto;
                dtos.Add(dto);
            }

            return dtos;
        }

        public List<OrganizationRegulatoryProgramUserDto> GetUserProfilesForOrgRegProgram(int orgRegProgramId,
                             bool? isRegApproved,
                             bool? isRegDenied,
                             bool? isEnabled,
                             bool? isRemoved)
        {
            var dtos = new List<OrganizationRegulatoryProgramUserDto>();
            var users = _dbContext.OrganizationRegulatoryProgramUsers.Where(u => u.OrganizationRegulatoryProgramId == orgRegProgramId);

            if (isRegApproved.HasValue)
                users = users.Where(u => u.IsRegistrationApproved == isRegApproved);
            if (isRegDenied.HasValue)
                users = users.Where(u => u.IsRegistrationDenied == isRegDenied);
            if (isEnabled.HasValue)
                users = users.Where(u => u.IsEnabled == isEnabled);
            if (isRemoved.HasValue)
                users = users.Where(u => u.IsRemoved == isRemoved);

            foreach (var user in users.ToList())
            {
                var dto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user);
                //manually map user profile child
                var userProfileDto = GetUserProfileById(user.UserProfileId);

                //Need to modify datetime to local
                dto.RegistrationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(dto.RegistrationDateTimeUtc.Value.UtcDateTime, orgRegProgramId);
                userProfileDto.CreationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(userProfileDto.CreationDateTimeUtc.Value.UtcDateTime, orgRegProgramId);

                dto.UserProfileDto = userProfileDto;
                dtos.Add(dto);
            }

            return dtos;
        }

        public void UpdateUserState(int orgRegProgUserId, bool? isRegApproved, bool? isRegDenied, bool? isEnabled, bool? isRemoved)
        {
            OrganizationRegulatoryProgramUser user = _dbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (user != null)
            {
                if (isRegApproved.HasValue)
                    user.IsRegistrationApproved = isRegApproved.Value;
                if (isRegDenied.HasValue)
                    user.IsRegistrationDenied = isRegDenied.Value;
                if (isEnabled.HasValue)
                    user.IsEnabled = isEnabled.Value;
                if (isRemoved.HasValue)
                    user.IsRemoved = isRemoved.Value;

                //Persist modification date and modifier actor
                user.LastModificationDateTimeUtc = DateTimeOffset.Now;
                user.LastModifierUserId = Convert.ToInt32(_httpContext.CurrentUserProfileId());
                _dbContext.SaveChanges();
            }
            else
            {
                throw new Exception($"ERROR: Cannot find Org Reg Program User associated with OrganizationRegulatoryProgramUserId={orgRegProgUserId}.");
            }
        }

        public void UpdateUserPermissionGroupId(int orgRegProgUserId, int permissionGroupId, bool isAuthorizationRequired = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgUserId))
            {
                throw new UnauthorizedAccessException();
            }

            int? previousPermissionGroupId;
            var user = _dbContext.OrganizationRegulatoryProgramUsers
                .Include("OrganizationRegulatoryProgram")
                .Single(o => o.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            previousPermissionGroupId = user.PermissionGroupId;
            if (previousPermissionGroupId == permissionGroupId)
            {
                return;
            }

            user.PermissionGroupId = permissionGroupId;
            _dbContext.SaveChanges();

            int thisUserOrgRegProgUserId = Convert.ToInt32(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(actorProgramUser);
            var actorUser = this.GetUserProfileById(actorProgramUserDto.UserProfileId);

            var targetOrgRegProgram = user.OrganizationRegulatoryProgram;
            var userProfile = GetUserProfileById(user.UserProfileId);

            string previousRoleLabel = _dbContext.PermissionGroups.Single(pg => pg.PermissionGroupId == previousPermissionGroupId).Name;
            string newRoleLabel = _dbContext.PermissionGroups.Single(pg => pg.PermissionGroupId == permissionGroupId).Name;

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
            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("organizationName", targetOrgRegProgram.Organization.Name);
            contentReplacements.Add("firstName", userProfile.FirstName);
            contentReplacements.Add("lastName", userProfile.LastName);
            contentReplacements.Add("userName", userProfile.UserName);
            contentReplacements.Add("emailAddress", userProfile.Email);

            contentReplacements.Add("oldRole", previousRoleLabel);
            contentReplacements.Add("newRole", newRoleLabel);

            contentReplacements.Add("actorOrganizationName", actorProgramUserDto.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName);
            contentReplacements.Add("actorFirstName", actorUser.FirstName);
            contentReplacements.Add("actorLastName", actorUser.LastName);
            contentReplacements.Add("actorUserName", actorUser.UserName);
            contentReplacements.Add("actorEmailAddress", actorUser.Email);

            _crommerAuditLogService.Log(CromerrEvent.UserAccess_RoleChange, cromerrAuditLogEntryDto, contentReplacements);


        }

        public void UpdateUserSignatoryStatus(int orgRegProgUserId, bool isSignatory, bool isAuthorizationRequired = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgUserId))
            {
                throw new UnauthorizedAccessException();
            }

            OrganizationRegulatoryProgramUser programUser = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (programUser.IsSignatory == isSignatory)
            {
                //No change therefore...
                return;
            }
            programUser.IsSignatory = isSignatory;
            programUser.LastModificationDateTimeUtc = DateTimeOffset.Now;
            programUser.LastModifierUserId = _httpContext.CurrentUserProfileId();
            _dbContext.SaveChanges();

            var user = GetUserProfileById(programUser.UserProfileId);
            var orgRegProgram = programUser.OrganizationRegulatoryProgram;
            var authority = _dbContext.OrganizationRegulatoryPrograms
                .SingleOrDefault(o => o.OrganizationId == programUser.OrganizationRegulatoryProgram.RegulatorOrganizationId
                && o.RegulatoryProgramId == orgRegProgram.RegulatoryProgramId);
            if (authority == null)
            {
                //This IS an authority
                authority = orgRegProgram;
            }

            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("firstName", user.FirstName);
            contentReplacements.Add("lastName", user.LastName);
            contentReplacements.Add("userName", user.UserName);
            contentReplacements.Add("emailAddress", user.Email);
            contentReplacements.Add("authorityName", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName));
            contentReplacements.Add("authorityOrganizationName", authority.Organization.Name);
            contentReplacements.Add("organizationName", orgRegProgram.Organization.Name);
            contentReplacements.Add("addressLine1", orgRegProgram.Organization.AddressLine1);
            contentReplacements.Add("cityName", orgRegProgram.Organization.CityName);
            contentReplacements.Add("stateName", orgRegProgram.Organization.JurisdictionId.HasValue ?
                                                 _dbContext.Jurisdictions.Single(j => j.JurisdictionId == orgRegProgram.Organization.JurisdictionId.Value).Code : "");
            contentReplacements.Add("supportEmail", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress));
            contentReplacements.Add("supportPhoneNumber", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone));

            //Email user
            EmailType emailType = isSignatory ? EmailType.Signature_SignatoryGranted : EmailType.Signature_SignatoryRevoked;
            EmailType adminEmailType = isSignatory ? EmailType.Signature_SignatoryGrantedToAdmin : EmailType.Signature_SignatoryRevokedToAdmin;

            _requestCache.SetValue(CacheKey.EmailRecipientRegulatoryProgramId, orgRegProgram.RegulatoryProgramId);
            _requestCache.SetValue(CacheKey.EmailRecipientOrganizationId, orgRegProgram.OrganizationId);
            _requestCache.SetValue(CacheKey.EmailRecipientRegulatoryOrganizationId, orgRegProgram.RegulatorOrganizationId);
 
            //Log Cromerr
            CromerrEvent cromerrEvent = isSignatory ? CromerrEvent.Signature_SignatoryGranted : CromerrEvent.Signature_SignatoryRevoked;
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
            
            _crommerAuditLogService.Log(cromerrEvent, cromerrAuditLogEntryDto, contentReplacements);

            //Email the user
            _emailService.SendEmail(new[] { user.Email }, emailType, contentReplacements);

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
                var adminProfile = GetUserProfileById(admin.UserProfileId);
                if (!adminProfile.IsAccountLocked && !adminProfile.IsAccountResetRequired)
                {
                    var contentReplacementsForAdmin = new Dictionary<string, string>();
                    contentReplacementsForAdmin.Add("adminFirstName", adminProfile.FirstName);
                    contentReplacementsForAdmin.Add("adminLastName", adminProfile.LastName);
                    contentReplacementsForAdmin.Add("firstName", user.FirstName);
                    contentReplacementsForAdmin.Add("lastName", user.LastName);
                    contentReplacementsForAdmin.Add("authorityName", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName));
                    contentReplacementsForAdmin.Add("authorityOrganizationName", authority.Organization.Name);
                    contentReplacementsForAdmin.Add("email", user.Email);
                    contentReplacementsForAdmin.Add("organizationName", orgRegProgram.Organization.Name);
                    contentReplacementsForAdmin.Add("addressLine1", orgRegProgram.Organization.AddressLine1);
                    contentReplacementsForAdmin.Add("cityName", orgRegProgram.Organization.CityName);
                    contentReplacementsForAdmin.Add("stateName", orgRegProgram.Organization.JurisdictionId.HasValue ?
                                                         _dbContext.Jurisdictions.Single(j => j.JurisdictionId == orgRegProgram.Organization.JurisdictionId.Value).Code : "");
                    contentReplacementsForAdmin.Add("emailAddress", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress));
                    contentReplacementsForAdmin.Add("phoneNumber", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone));

                    _emailService.SendEmail(new[] { adminProfile.Email }, adminEmailType, contentReplacementsForAdmin).Wait();

                } 
            }  
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
                return new ResetUserResultDto()
                {
                    IsSuccess = false,
                    FailureReason = ResetUserFailureReason.CannotResetSupportRole
                };

            //Check user is not THIS user's own account
            int thisUserProfileId = _httpContext.CurrentUserProfileId();
            if (userProfileId == thisUserProfileId)
                return new ResetUserResultDto()
                {
                    IsSuccess = false,
                    FailureReason = ResetUserFailureReason.CannotResetOwnAccount
                };


            var sendEmailChangedNotifications = new List<string>();
            if (String.IsNullOrEmpty(newEmailAddress) || user.Email == newEmailAddress)
            {
                // no change
            }
            else
            {
                //Check if new address is in use
                if (GetUserProfileByEmail(newEmailAddress) != null)
                {
                    return new ResetUserResultDto()
                    {
                        IsSuccess = false,
                        FailureReason = ResetUserFailureReason.NewEmailAddressAlreadyInUse
                    };
                }
                else
                {
                    user.OldEmailAddress = user.Email;
                    user.Email = newEmailAddress;
                    sendEmailChangedNotifications.Add(user.OldEmailAddress);
                    sendEmailChangedNotifications.Add(user.Email);
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

            //Send all email types
            string supportPhoneNumber = _globalSettings[SystemSettingType.SupportPhoneNumber];
            string supportEmail = _globalSettings[SystemSettingType.SupportEmailAddress];


            //1) Send "Email Changed" emails
            var contentReplacements = new Dictionary<string, string>();

            //Find all possible authorities
            var authorityList = _orgService.GetUserAuthorityListForEmailContent(userProfileId);

            foreach (var email in sendEmailChangedNotifications)
            {
                contentReplacements = new Dictionary<string, string>();
                contentReplacements.Add("firstName", user.FirstName);
                contentReplacements.Add("lastName", user.LastName);
                contentReplacements.Add("oldEmail", user.OldEmailAddress);
                contentReplacements.Add("newEmail", user.Email);
                contentReplacements.Add("authorityList", authorityList);
                contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
                contentReplacements.Add("supportEmail", supportEmail);
                _emailService.SendEmail(new[] { email }, EmailType.Profile_EmailChanged, contentReplacements);
            }

            //2) Send "Reg Reset" Email
            var token = Guid.NewGuid().ToString();

            //Create Invitation entry
            //
            //Case "Authority Reset Authority" : sender org reg program is same as target/recipient org reg program
            //Case "Authority Reset Industry" : sender org reg program is different than target/recipient org reg program
            int senderOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var newInvitation = _dbContext.Invitations.Create();
            newInvitation.InvitationId = token;
            newInvitation.InvitationDateTimeUtc = DateTimeOffset.Now;
            newInvitation.EmailAddress = user.Email;
            newInvitation.FirstName = user.FirstName;
            newInvitation.LastName = user.LastName;
            newInvitation.RecipientOrganizationRegulatoryProgramId = targetOrgRegProgramId;
            newInvitation.SenderOrganizationRegulatoryProgramId = senderOrgRegProgramId;
            newInvitation.IsResetInvitation = true;
            _dbContext.Invitations.Add(newInvitation);
            _dbContext.SaveChanges();

            string baseUrl = _httpContext.GetRequestBaseUrl();
            string link = baseUrl + "Account/Register?token=" + token;
            contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("link", link);
            contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
            contentReplacements.Add("supportEmail", supportEmail);
            _requestCache.SetValue(CacheKey.Token, token);
            _emailService.SendEmail(new[] { user.Email }, EmailType.Profile_ResetProfileRequired, contentReplacements);

            //Log to Cromerr
            int actorOrgRegProgUserId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                .Include("OrganizationRegulatoryProgram")
                .Single(u => u.OrganizationRegulatoryProgramUserId == actorOrgRegProgUserId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(actorProgramUser);
            var actorUser = this.GetUserProfileById(actorProgramUserDto.UserProfileId);

            var allOrgRegProgramUsers = _dbContext.OrganizationRegulatoryProgramUsers
                .Include(orpu => orpu.OrganizationRegulatoryProgram)
                .Where(orpu => orpu.UserProfileId == userProfileId)
                .ToList();

            foreach (var targetOrgRegProgramUser in allOrgRegProgramUsers)
            {
                var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
                cromerrAuditLogEntryDto.RegulatoryProgramId = targetOrgRegProgramUser.OrganizationRegulatoryProgram.RegulatoryProgramId;
                cromerrAuditLogEntryDto.OrganizationId = targetOrgRegProgramUser.OrganizationRegulatoryProgram.OrganizationId;
                cromerrAuditLogEntryDto.RegulatorOrganizationId = targetOrgRegProgramUser.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                cromerrAuditLogEntryDto.UserProfileId = user.UserProfileId;
                cromerrAuditLogEntryDto.UserName = user.UserName;
                cromerrAuditLogEntryDto.UserFirstName = user.FirstName;
                cromerrAuditLogEntryDto.UserLastName = user.LastName;
                cromerrAuditLogEntryDto.UserEmailAddress = user.Email;
                cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
                contentReplacements = new Dictionary<string, string>();
                contentReplacements.Add("firstName", user.FirstName);
                contentReplacements.Add("lastName", user.LastName);
                contentReplacements.Add("userName", user.UserName);
                contentReplacements.Add("emailAddress", user.Email);
                contentReplacements.Add("authorityName", actorProgramUserDto.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName);
                contentReplacements.Add("authorityFirstName", actorUser.FirstName);
                contentReplacements.Add("authorityLastName", actorUser.LastName);
                contentReplacements.Add("authorityUserName", actorUser.UserName);
                contentReplacements.Add("authorityEmailaddress", actorUser.Email);

                _crommerAuditLogService.Log(CromerrEvent.UserAccess_AccountResetInitiated, cromerrAuditLogEntryDto, contentReplacements);
            }

            return new ResetUserResultDto()
            {
                IsSuccess = true
            };

        }

        private void SendAccountLockoutEmails(UserProfile user, AccountLockEvent reason)
        {
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
                    && o.OrganizationRegulatoryProgramId == program.OrganizationRegulatoryProgramId);

                var perProgram = program.RegulatorOrganizationId.HasValue;

                foreach (var admin in admins.ToList())
                {
                    string adminEmail = GetUserProfileById(admin.UserProfileId).Email;
                    if (!adminEmailList.Contains(adminEmail))
                    {
                        contentReplacements = new Dictionary<string, string>();
                        contentReplacements.Add("firstName", user.FirstName);
                        contentReplacements.Add("lastName", user.LastName);
                        contentReplacements.Add("userName", user.UserName);
                        contentReplacements.Add("email", user.Email);
                        _emailService.SendEmail(new[] { adminEmail }, EmailType.UserAccess_LockoutToSysAdmins, contentReplacements, perProgram);
                        adminEmailList.Add(adminEmail);
                    }
                }

                //Get authority's org id, if it exists. If not, they ARE the authority
                int authorityOrganizationId = program.RegulatorOrganizationId.HasValue ?
                    program.RegulatorOrganizationId.Value : program.OrganizationId;

                //Find distinct authorities
                var authority = _dbContext.OrganizationRegulatoryPrograms
                    .Single(o => o.OrganizationId == authorityOrganizationId
                        && o.RegulatoryProgramId == program.RegulatoryProgramId);
                if (!(authorityList.Exists(a => a.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId)))
                    authorityList.Add(authority);
            }

            string authorityName = "";
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
                    var adminUserProfile = GetUserProfileById(admin.UserProfileId);

                    if (!adminUserProfile.IsAccountLocked 
                        && !adminUserProfile.IsAccountResetRequired
                        && !adminEmailList.Contains(adminUserProfile.Email))
                    {
                        contentReplacements = new Dictionary<string, string>();
                        contentReplacements.Add("firstName", user.FirstName);
                        contentReplacements.Add("lastName", user.LastName);
                        contentReplacements.Add("userName", user.UserName);
                        contentReplacements.Add("email", user.Email);
                        _emailService.SendEmail(new[] { adminUserProfile.Email }, EmailType.UserAccess_LockoutToSysAdmins, contentReplacements); // sending email to locked user's authority Admin's
                        adminEmailList.Add(adminUserProfile.Email);
                    }
                }

                if (reason == AccountLockEvent.ManualAction)
                {
                    //Send to user on behalf of each program's authority
                    authorityName = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName);
                    var authorityEmail = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress);
                    var authorityPhone = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone);

                    contentReplacements = new Dictionary<string, string>();
                    contentReplacements.Add("authorityName", authorityName);
                    contentReplacements.Add("authorityOrganizationName", authority.Organization.Name);
                    contentReplacements.Add("authoritySupportEmail", authorityEmail);
                    contentReplacements.Add("authoritySupportPhoneNumber", authorityPhone);
                    _emailService.SendEmail(new[] { user.Email }, EmailType.UserAccess_AccountLockout, contentReplacements);
                }
            }

            if (reason != AccountLockEvent.ManualAction)
            {
                string supportPhoneNumber = _globalSettings[SystemSettingType.SupportPhoneNumber];
                string supportEmail = _globalSettings[SystemSettingType.SupportEmailAddress];
                var authorityListString = _orgService.GetUserAuthorityListForEmailContent(user.UserProfileId);
                contentReplacements = new Dictionary<string, string>();
                contentReplacements.Add("firstName", user.FirstName);
                contentReplacements.Add("lastName", user.LastName);
                contentReplacements.Add("authorityList", authorityListString);
                contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
                contentReplacements.Add("supportEmail", supportEmail);

                var orgRegProgramIdStr = _httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId);
                if (!string.IsNullOrWhiteSpace(orgRegProgramIdStr))
                {
                    var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
                    var authorityOrganization = _orgService.GetAuthority(currentOrgRegProgramId);

                    authorityName = _settingService.GetOrgRegProgramSettingValue(authorityOrganization.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName);

                    if (!contentReplacements.Keys.Contains("authorityName"))
                    {
                        contentReplacements.Add("authorityName", authorityName);
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

                _emailService.SendEmail(new[] { user.Email }, emailType, contentReplacements);
            }
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

            return LockUnlockUserAccount(userProfileId, isAttemptingLock, reason, reportPackageId:null);
        }

        public AccountLockoutResultDto LockUnlockUserAccount(int userProfileId, bool isAttemptingLock, AccountLockEvent reason, int? reportPackageId = null)
        {
            var user = _dbContext.Users.Single(u => u.UserProfileId == userProfileId);
            //Check user is not support role
            if (user.IsInternalAccount && isAttemptingLock && reason == AccountLockEvent.ManualAction)
                return new Dto.AccountLockoutResultDto()
                {
                    IsSuccess = false,
                    FailureReason = AccountLockoutFailureReason.CannotLockoutSupportRole
                };

            //Check user is not THIS user's own account
            int thisUserProfileId = _httpContext.CurrentUserProfileId();
            if (thisUserProfileId > 0 && userProfileId == thisUserProfileId && reason == AccountLockEvent.ManualAction)
                return new Dto.AccountLockoutResultDto()
                {
                    IsSuccess = false,
                    FailureReason = AccountLockoutFailureReason.CannotLockoutOwnAccount
                };

            user.IsAccountLocked = isAttemptingLock;

            try
            {
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
                throw new RuleViolationException("Validation errors", validationIssues);

            }

            if (isAttemptingLock)
            {
                SendAccountLockoutEmails(user, reason);
            }

            LogLockUnlockActivityToCromerr(isAttemptingLock, user, reason, reportPackageId);

            //Success
            return new Dto.AccountLockoutResultDto()
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
                _logService.Info($"LogLockUnlockActivityToCromerr. isAttemptingLock={isAttemptingLock}, reason={reason}, Cannot associate a CromerrEvent with reason provided.");
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
                    thisUserOrgRegProgUserId = Convert.ToInt32(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
                }
                else
                {
                    //User did this to herself
                    thisUserOrgRegProgUserId = programUser.OrganizationRegulatoryProgramUserId;
                }

                var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                    .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
                var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(actorProgramUser);
                var actorUser = this.GetUserProfileById(actorProgramUserDto.UserProfileId);

                var userDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(programUser);

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

                var contentReplacements = new Dictionary<string, string>();

                contentReplacements.Add("organizationName", programUser.OrganizationRegulatoryProgram.Organization.Name);
                contentReplacements.Add("firstName", user.FirstName);
                contentReplacements.Add("lastName", user.LastName);
                contentReplacements.Add("userName", user.UserName);
                contentReplacements.Add("emailAddress", user.Email);

                if (!isAttemptingLock || reason == AccountLockEvent.ManualAction)
                {
                    contentReplacements.Add("authorityName", actorProgramUserDto.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName);
                    contentReplacements.Add("authorityUserName", actorUser.UserName);
                    contentReplacements.Add("authorityFirstName", actorUser.FirstName);
                    contentReplacements.Add("authorityLastName", actorUser.LastName);
                    contentReplacements.Add("authorityEmailaddress", actorUser.Email);
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
                        var periodStartDateLocal = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(reportPackage.PeriodStartDateTimeUtc.UtcDateTime, programUser.OrganizationRegulatoryProgramId);
                        var periodEndDateLocal = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(reportPackage.PeriodEndDateTimeUtc.UtcDateTime, programUser.OrganizationRegulatoryProgramId);
                        if(contentReplacements.Keys.Contains("organizationName"))
                        {
                            contentReplacements.Remove("organizationName");  
                        }
                        contentReplacements.Add("organizationName", reportPackage.OrganizationName);
                        contentReplacements.Add("reportPackageName", reportPackage.Name);
                        contentReplacements.Add("periodStartDate", periodStartDateLocal.ToString("MMM dd, yyyy"));
                        contentReplacements.Add("periodEndDate", periodEndDateLocal.ToString("MMM dd, yyyy"));
                    }
                    else
                    {
                        throw new Exception($"ERROR: Account lock Cromerr log entry could not be completed without report package Id");
                    }
                }

                _crommerAuditLogService.Log(cromerrEvent, cromerrAuditLogEntryDto, contentReplacements);

            }

        }

        public void EnableDisableUserAccount(int orgRegProgramUserId, bool isAttemptingDisable, bool isAuthorizationRequired = false)
        {
            _logService.Info($"EnableDisableUserAccount. OrgRegProgUserId={orgRegProgramUserId}, IsAttemptingDisable={isAttemptingDisable}...");

            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgramUserId))
            {
                throw new UnauthorizedAccessException();
            }

            //Check user is not THIS user's own account
            int thisUserOrgRegProgUserId = Convert.ToInt32(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
            if (orgRegProgramUserId == thisUserOrgRegProgUserId)
            {
                _logService.Info($"EnableDisableUserAccount. OrgRegProgUserId={orgRegProgramUserId}, IsAttemptingDisable={isAttemptingDisable}... CannotUpdateOwnAccount.");

                List<RuleViolation> validationIssues = new List<RuleViolation>();
                string message = "User cannot update own account.";
                validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);
            }

            if (!isAttemptingDisable)
            {
                //Check user license count
                //
                //For Authority or Industry?
                var orgRegProgramId = _dbContext.OrganizationRegulatoryProgramUsers
                    .Single(o => o.OrganizationRegulatoryProgramUserId == orgRegProgramUserId).OrganizationRegulatoryProgramId;
                var remainingUsersAllowed = _orgService.GetRemainingUserLicenseCount(orgRegProgramId);
                if (remainingUsersAllowed < 1)
                {
                    _logService.Info($"EnableDisableUserAccount. OrgRegProgUserId={orgRegProgramUserId}, IsAttemptingDisable={isAttemptingDisable}... NoMoreRemainingUserLicenses.");

                    List<RuleViolation> validationIssues = new List<RuleViolation>();
                    string message = "No more User Licenses are available for this organization. Disable another User and try again.";
                    validationIssues.Add(new RuleViolation(string.Empty, propertyValue: null, errorMessage: message));
                    throw new RuleViolationException(message: "Validation errors", validationIssues: validationIssues);

                }
            }

            var programUser = _dbContext.OrganizationRegulatoryProgramUsers
                .Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgramUserId);
            programUser.IsEnabled = !isAttemptingDisable;
            _dbContext.SaveChanges();

            //Log Cromerr
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(actorProgramUser);
            var actorUser = this.GetUserProfileById(actorProgramUserDto.UserProfileId);

            var userDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(programUser);
            var user = this.GetUserProfileById(userDto.UserProfileId);
            CromerrEvent cromerrEvent = isAttemptingDisable ? CromerrEvent.UserAccess_Disabled : CromerrEvent.UserAccess_Enabled;
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
            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("organizationName", programUser.OrganizationRegulatoryProgram.Organization.Name);
            contentReplacements.Add("firstName", user.FirstName);
            contentReplacements.Add("lastName", user.LastName);
            contentReplacements.Add("userName", user.UserName);
            contentReplacements.Add("emailAddress", user.Email);
            contentReplacements.Add("actorFirstName", actorUser.FirstName);
            contentReplacements.Add("actorLastName", actorUser.LastName);
            contentReplacements.Add("actorUserName", actorUser.UserName);

            _crommerAuditLogService.Log(cromerrEvent, cromerrAuditLogEntryDto, contentReplacements);

            _logService.Info($"EnableDisableUserAccount. OrgRegProgUserId={orgRegProgramUserId}, IsAttemptingDisable={isAttemptingDisable}... Success.");

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
            int thisUsersOrgRegProgUserId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
            if (thisUsersOrgRegProgUserId == orgRegProgUserId)
                return false;

            OrganizationRegulatoryProgramUser user = _dbContext.OrganizationRegulatoryProgramUsers
                .SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);

            //Set applicable flags
            user.IsRemoved = true;
            user.IsRegistrationApproved = false;
            user.IsEnabled = false;
            user.IsRegistrationDenied = false;

            //Persist modification date and modifier actor
            user.LastModificationDateTimeUtc = DateTimeOffset.Now;
            user.LastModifierUserId = Convert.ToInt32(_httpContext.CurrentUserProfileId());
            _dbContext.SaveChanges();

            //Log Cromerr
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                .Single(u => u.OrganizationRegulatoryProgramUserId == thisUsersOrgRegProgUserId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(actorProgramUser);
            var actorUser = this.GetUserProfileById(actorProgramUserDto.UserProfileId);

            var userDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user);
            userDto.UserProfileDto = GetUserProfileById(user.UserProfileId);
            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
            cromerrAuditLogEntryDto.RegulatoryProgramId = user.OrganizationRegulatoryProgram.RegulatoryProgramId;
            cromerrAuditLogEntryDto.OrganizationId = user.OrganizationRegulatoryProgram.OrganizationId;
            cromerrAuditLogEntryDto.RegulatorOrganizationId = user.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = user.UserProfileId;
            cromerrAuditLogEntryDto.UserName = userDto.UserProfileDto.UserName;
            cromerrAuditLogEntryDto.UserFirstName = userDto.UserProfileDto.FirstName;
            cromerrAuditLogEntryDto.UserLastName = userDto.UserProfileDto.LastName;
            cromerrAuditLogEntryDto.UserEmailAddress = userDto.UserProfileDto.Email;
            cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("organizationName", user.OrganizationRegulatoryProgram.Organization.Name);
            contentReplacements.Add("firstName", userDto.UserProfileDto.FirstName);
            contentReplacements.Add("lastName", userDto.UserProfileDto.LastName);
            contentReplacements.Add("userName", userDto.UserProfileDto.UserName);
            contentReplacements.Add("emailAddress", userDto.UserProfileDto.Email);
            contentReplacements.Add("actorFirstName", actorUser.FirstName);
            contentReplacements.Add("actorLastName", actorUser.LastName);
            contentReplacements.Add("actorUserName", actorUser.UserName);

            _crommerAuditLogService.Log(CromerrEvent.UserAccess_Removed, cromerrAuditLogEntryDto, contentReplacements);

            return true;
        }

        private void UpdateUser(UserDto dto)
        {
            UserProfile userProfile = _dbContext.Users.Single(up => up.UserProfileId == dto.UserProfileId);
            userProfile = _mapHelper.GetUserProfileFromUserDto(dto, userProfile);

            _dbContext.SaveChanges();
        }

        public void UpdateProfile(UserDto dto)
        {
            UpdateUser(dto);

            //Send email
            var contentReplacements = new Dictionary<string, string>();
            string supportPhoneNumber = _globalSettings[SystemSettingType.SupportPhoneNumber];
            string supportEmail = _globalSettings[SystemSettingType.SupportEmailAddress];

            var authorityList = _orgService.GetUserAuthorityListForEmailContent(dto.UserProfileId);
            contentReplacements.Add("firstName", dto.FirstName);
            contentReplacements.Add("lastName", dto.LastName);
            contentReplacements.Add("authorityList", authorityList);
            contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
            contentReplacements.Add("supportEmail", supportEmail);
            _emailService.SendEmail(new[] { dto.Email }, EmailType.Profile_ProfileChanged, contentReplacements);
        }
  
        public RegistrationResult ValidateUserProfileData(UserDto userProfile)
        {

            if (userProfile == null ||
                string.IsNullOrWhiteSpace(userProfile.FirstName) ||
                string.IsNullOrWhiteSpace(userProfile.LastName) ||
                string.IsNullOrWhiteSpace(userProfile.AddressLine1) ||
                string.IsNullOrWhiteSpace(userProfile.CityName) ||
                string.IsNullOrWhiteSpace(userProfile.ZipCode) ||
                string.IsNullOrWhiteSpace(userProfile.Email) ||
                string.IsNullOrWhiteSpace(userProfile.UserName)
               )
            {
                return RegistrationResult.BadUserProfileData;
            }

            return RegistrationResult.Success;
        }

        public RegistrationResult ValidateRegistrationUserData(UserDto userProfile, IEnumerable<AnswerDto> securityQuestions, IEnumerable<AnswerDto> kbqQuestions)
        {
            if (userProfile == null ||
                string.IsNullOrWhiteSpace(userProfile.FirstName) ||
                string.IsNullOrWhiteSpace(userProfile.LastName) ||
                string.IsNullOrWhiteSpace(userProfile.AddressLine1) ||
                string.IsNullOrWhiteSpace(userProfile.CityName) ||
                string.IsNullOrWhiteSpace(userProfile.ZipCode) ||
                string.IsNullOrWhiteSpace(userProfile.Email) ||
                string.IsNullOrWhiteSpace(userProfile.UserName) ||
                securityQuestions == null || kbqQuestions == null)
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
                result.Add(RegistrationResult.MissingKBQ);
            }

            // Test duplicated KBQ questions
            if (kbqQuestions.GroupBy(i => i.QuestionId).Any(i => i.Count() > 1))
            {
                result.Add(RegistrationResult.DuplicatedKBQ);
            }
 
            // Test duplicated KBQ question answers
            if (kbqQuestions.GroupBy(i => i.Content).Any(i => i.Count() > 1))
            {
                result.Add(RegistrationResult.DuplicatedKBQAnswer);
            }
            
            // Test KBQ questions must have answer
            if (kbqQuestions.Any(i => i.Content == null))
            {
                result.Add(RegistrationResult.MissingKBQAnswer);
            }

            return result;
        } 

        public ICollection<RegistrationResult> SecurityValidation(IEnumerable<AnswerDto> securityQuestions)
        {
            var result = new List<RegistrationResult>(); 

            if (securityQuestions.Count() < 2)
            {
                result.Add(RegistrationResult.MissingSecurityQuestion);
            } 

            // Test duplicated security questions
            if (securityQuestions.GroupBy(i => i.QuestionId).Any(i => i.Count() > 1))
            {
                result.Add(RegistrationResult.DuplicatedSecurityQuestion);
            }
 
            // Test duplicated security question answers
            if (securityQuestions.GroupBy(i => i.Content).Any(i => i.Count() > 1))
            {
                result.Add(RegistrationResult.DuplicatedSecurityQuestionAnswer);
            }
 
            // Test security questions must have answer
            if (securityQuestions.Any(i => i.Content == null))
            {
                result.Add(RegistrationResult.MissingSecurityQuestionAnswer);
            } 

            return result;
        }

        public bool UpdateEmail(int userProfileId, string newEmailAddress)
        {
            UserProfile userProfile;
            string oldEmailAddress;
            //Check if email in use
            var dbContextTransaction = _dbContext.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);
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

            //Send emails (to old and new address)
            var contentReplacements = new Dictionary<string, string>();
            string supportPhoneNumber = _globalSettings[SystemSettingType.SupportPhoneNumber];
            string supportEmail = _globalSettings[SystemSettingType.SupportEmailAddress];

            var authorityList = _orgService.GetUserAuthorityListForEmailContent(userProfileId);
            contentReplacements.Add("firstName", userProfile.FirstName);
            contentReplacements.Add("lastName", userProfile.LastName);
            contentReplacements.Add("oldEmail", oldEmailAddress);
            contentReplacements.Add("newEmail", newEmailAddress);
            contentReplacements.Add("authorityList", authorityList);
            contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
            contentReplacements.Add("supportEmail", supportEmail);
            _emailService.SendEmail(new[] { oldEmailAddress }, EmailType.Profile_EmailChanged, contentReplacements);
            _emailService.SendEmail(new[] { newEmailAddress }, EmailType.Profile_EmailChanged, contentReplacements);

            //Need to log this activity for all Org Reg Program users
            var orgRegProgramUsers = _dbContext.OrganizationRegulatoryProgramUsers
                .Include(orpu => orpu.OrganizationRegulatoryProgram)
                .Where(orpu => orpu.UserProfileId == userProfileId);

            foreach (var orgRegProgramUser in orgRegProgramUsers)
            {
                var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
                cromerrAuditLogEntryDto.RegulatoryProgramId = orgRegProgramUser.OrganizationRegulatoryProgram.RegulatoryProgramId;
                cromerrAuditLogEntryDto.OrganizationId = orgRegProgramUser.OrganizationRegulatoryProgram.OrganizationId;
                cromerrAuditLogEntryDto.RegulatorOrganizationId = orgRegProgramUser.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
                cromerrAuditLogEntryDto.UserProfileId = userProfile.UserProfileId;
                cromerrAuditLogEntryDto.UserName = userProfile.UserName;
                cromerrAuditLogEntryDto.UserFirstName = userProfile.FirstName;
                cromerrAuditLogEntryDto.UserLastName = userProfile.LastName;
                cromerrAuditLogEntryDto.UserEmailAddress = newEmailAddress;
                cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
                cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
                contentReplacements = new Dictionary<string, string>();
                contentReplacements.Add("firstName", userProfile.FirstName);
                contentReplacements.Add("lastName", userProfile.LastName);
                contentReplacements.Add("userName", userProfile.UserName);
                contentReplacements.Add("oldEmail", oldEmailAddress);
                contentReplacements.Add("newEmail", newEmailAddress);
                contentReplacements.Add("emailAddress", newEmailAddress);

                _crommerAuditLogService.Log(CromerrEvent.Profile_EmailChanged, cromerrAuditLogEntryDto, contentReplacements);
            }

            return true;
        }

        public OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUser(int orgRegProgUserId, bool isAuthorizationRequired = false)
        {
            if (isAuthorizationRequired && !CanUserExecuteApi(id:orgRegProgUserId))
            {
                throw new UnauthorizedAccessException();
            }

            var currentOrgRegProgramId = int.Parse(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            OrganizationRegulatoryProgramUser user = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            OrganizationRegulatoryProgramUserDto userDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user);
            userDto.UserProfileDto = GetUserProfileById(user.UserProfileId);
            var jurisdiction = _dbContext.Jurisdictions.Single(j => j.JurisdictionId == userDto.UserProfileDto.JurisdictionId);
            userDto.UserProfileDto.Jurisdiction = _mapHelper.GetJurisdictionDtoFromJurisdiction(jurisdiction);

            //Need to modify datetime to local
            userDto.UserProfileDto.CreationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(userDto.UserProfileDto.CreationDateTimeUtc.Value.UtcDateTime, currentOrgRegProgramId);
            userDto.RegistrationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(userDto.RegistrationDateTimeUtc.Value.UtcDateTime, currentOrgRegProgramId);

            return userDto;
        }

        public void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int orgRegProgUserId, bool isApproved, bool isSignatory)
        {
            var user = _dbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (user == null) return;

            user.IsRegistrationApproved = isApproved;
            user.IsRegistrationDenied = !isApproved;
             
            // Call UpdateUserSignatoryStatus to set IsSignatory flag to avoid duplicated code for emails and Cromerr
            UpdateUserSignatoryStatus(orgRegProgUserId, isSignatory, true); 
            _dbContext.SaveChanges();
        }

        public void UpdateOrganizationRegulatoryProgramUserRole(int orgRegProgUserId, int permissionGroupId)
        {
            var user = _dbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (user == null) return;

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

            var programUser = GetOrganizationRegulatoryProgramUser(orgRegProgUserId);
            int orgRegProgramId = programUser.OrganizationRegulatoryProgramId;
            var authority = _orgService.GetAuthority(orgRegProgramId);
            var isAuthorityUser = !(authority != null && authority.OrganizationRegulatoryProgramId != orgRegProgramId);
            
            //Check user license count
            if (isApproved)
            {
                var remainingUserLicenseCount = _orgService.GetRemainingUserLicenseCount(orgRegProgramId);
                if (remainingUserLicenseCount < 1)
                {
                    //ACTION BLOCKED -- NO MORE USER LICENSES
                    return isAuthorityUser
                               ? new RegistrationResultDto() {Result = RegistrationResult.NoMoreUserLicensesForAuthority}
                               : new RegistrationResultDto() {Result = RegistrationResult.NoMoreUserLicensesForIndustry};
                }

            }

            var transaction = _dbContext.BeginTransaction();
            try
            { 

                if(isAuthorityUser)
                { 
                    isSignatory = false; 
                }
                
                //Only update Role if we are Approving
                if (isApproved)
                {
                    UpdateOrganizationRegulatoryProgramUserRole(orgRegProgUserId, permissionGroupId);
                }

                UpdateOrganizationRegulatoryProgramUserApprovedStatus(orgRegProgUserId, isApproved, isSignatory);
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

            var contentReplacements = new Dictionary<string, string>();
            string authorityName = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName);
            string authorityPhoneNumber = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone);
            string authorityEmail = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress);

            contentReplacements.Add("firstName", programUser.UserProfileDto.FirstName);
            contentReplacements.Add("lastName", programUser.UserProfileDto.LastName);
            contentReplacements.Add("authorityName", authorityName);
            contentReplacements.Add("authorityOrganizationName", authority.OrganizationDto.OrganizationName);
            contentReplacements.Add("phoneNumber", authorityPhoneNumber);
            contentReplacements.Add("emailAddress", authorityEmail);
            contentReplacements.Add("supportPhoneNumber", authorityPhoneNumber);
            contentReplacements.Add("supportEmail", authorityEmail);

            if (!isAuthorityUser)
            {
                var org = _dbContext.Organizations.Single(o => o.OrganizationId == programUser.OrganizationRegulatoryProgramDto.OrganizationId);

                contentReplacements.Add("organizationName", org.Name);
                contentReplacements.Add("addressLine1", org.AddressLine1);
                contentReplacements.Add("cityName", org.CityName);
                contentReplacements.Add("stateName", org.JurisdictionId.HasValue ?
                                                     _dbContext.Jurisdictions.Single(j => j.JurisdictionId == org.JurisdictionId.Value).Code : "");
            }

            if (isApproved)
            {
                string baseUrl = _httpContext.GetRequestBaseUrl();
                string link = baseUrl + "Account/SignIn";
                contentReplacements.Add("link", link);
            }

            _emailService.SendEmail(new[] { programUser.UserProfileDto.Email }, emailType, contentReplacements);

            //Cromerr log
            int thisUserOrgRegProgUserId = Convert.ToInt32(_httpContext.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(actorProgramUser);
            var actorUser = this.GetUserProfileById(actorProgramUserDto.UserProfileId);

            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
            cromerrAuditLogEntryDto.RegulatoryProgramId = programUser.OrganizationRegulatoryProgramDto.RegulatoryProgramId;
            cromerrAuditLogEntryDto.OrganizationId = programUser.OrganizationRegulatoryProgramDto.OrganizationId;
            cromerrAuditLogEntryDto.RegulatorOrganizationId = programUser.OrganizationRegulatoryProgramDto.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
            cromerrAuditLogEntryDto.UserProfileId = programUser.UserProfileId;
            cromerrAuditLogEntryDto.UserName = programUser.UserProfileDto.UserName;
            cromerrAuditLogEntryDto.UserFirstName = programUser.UserProfileDto.FirstName;
            cromerrAuditLogEntryDto.UserLastName = programUser.UserProfileDto.LastName;
            cromerrAuditLogEntryDto.UserEmailAddress = programUser.UserProfileDto.Email;
            cromerrAuditLogEntryDto.IPAddress = _httpContext.CurrentUserIPAddress();
            cromerrAuditLogEntryDto.HostName = _httpContext.CurrentUserHostName();
            contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("authorityName", authority.OrganizationDto.OrganizationName);
            contentReplacements.Add("organizationName", programUser.OrganizationRegulatoryProgramDto.OrganizationDto.OrganizationName);
            contentReplacements.Add("regulatoryProgram", authority.RegulatoryProgramDto.Name);
            contentReplacements.Add("firstName", programUser.UserProfileDto.FirstName);
            contentReplacements.Add("lastName", programUser.UserProfileDto.LastName);
            contentReplacements.Add("userName", programUser.UserProfileDto.UserName);
            contentReplacements.Add("emailAddress", programUser.UserProfileDto.Email);
            contentReplacements.Add("actorFirstName", actorUser.FirstName);
            contentReplacements.Add("actorLastName", actorUser.LastName);
            contentReplacements.Add("actorUserName", actorUser.UserName);
            contentReplacements.Add("actorEmailAddress", actorUser.Email);

            _crommerAuditLogService.Log(cromerrEvent, cromerrAuditLogEntryDto, contentReplacements);


            return new RegistrationResultDto() { Result = RegistrationResult.Success };

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
            var userProfiles = _dbContext.Users.Where(i => i.IsAccountLocked == false && i.IsAccountResetRequired == false && signatoryIds.Contains(i.UserProfileId));

            var userDtos = new List<UserDto>();
            foreach (var userProfile in userProfiles)
            {
                var userDto = _mapHelper.GetUserDtoFromUserProfile(userProfile);
                userDtos.Add(userDto);
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
                                                && (i.PermissionGroup.Name == PermissionGroupName.Administrator.ToString() ||
                                                    i.PermissionGroup.Name == PermissionGroupName.Standard.ToString())
                                          ).Select(i => i.UserProfileId);


            var userProfiles = _dbContext.Users.Where(i => i.IsAccountLocked == false && i.IsAccountResetRequired == false && userIds.Contains(i.UserProfileId));
            var userDtos = new List<UserDto>();
            foreach (var userProfile in userProfiles)
            {
                var userDto = _mapHelper.GetUserDtoFromUserProfile(userProfile);
                userDtos.Add(userDto);
            }
            return userDtos;
            //var userProfiles = _dbContext.Users.Where(i => userIds.Contains(i.UserProfileId));
            //return userProfiles.Select(a => _mapHelper.GetUserDtoFromUserProfile(a)).ToList();
        }

        #endregion
    }
}
