using System;
using System.Collections.Generic;
using System.Linq;
using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services.Dto;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.Organization;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.TimeZone;
using Microsoft.AspNet.Identity;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.QuestionAnswer;
using System.Data.Entity.Validation;
using Linko.LinkoExchange.Core.Validation;
using System.Data.Entity.Infrastructure;
using NLog;
using Linko.LinkoExchange.Services.Mapping;
using Linko.LinkoExchange.Services.AuditLog;

namespace Linko.LinkoExchange.Services.User
{
    public class UserService : IUserService
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
                dto.RegistrationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(dto.RegistrationDateTimeUtc.Value.DateTime, orgRegProgramId);
                userProfileDto.CreationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(userProfileDto.CreationDateTimeUtc.Value.DateTime, orgRegProgramId);

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
                dto.RegistrationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(dto.RegistrationDateTimeUtc.Value.DateTime, orgRegProgramId);
                userProfileDto.CreationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(userProfileDto.CreationDateTimeUtc.Value.DateTime, orgRegProgramId);

                dto.UserProfileDto = userProfileDto;
                dtos.Add(dto);
            }

            return dtos;
        }


        public int AddNewUser(int orgRegProgId, int permissionGroupId, string emailAddress, string firstName, string lastName)
        {
            var newOrgRegProgUser = _dbContext.OrganizationRegulatoryProgramUsers.Create();
            newOrgRegProgUser.OrganizationRegulatoryProgramId = orgRegProgId;
            newOrgRegProgUser.PermissionGroup = _dbContext.PermissionGroups.Single(p => p.PermissionGroupId == permissionGroupId);

            var newUserProfile = _dbContext.Users.Create();
            newUserProfile.Email = emailAddress;
            newUserProfile.FirstName = firstName;
            newUserProfile.LastName = lastName;
            _dbContext.Users.Add(newUserProfile);
            newOrgRegProgUser.UserProfileId = newUserProfile.UserProfileId;

            return _dbContext.SaveChanges();
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
                user.LastModificationDateTimeUtc = DateTime.UtcNow;
                user.LastModifierUserId = Convert.ToInt32(_httpContext.CurrentUserProfileId());
                _dbContext.SaveChanges();
            }
            else
            {
                //_logger.Log("ERROR")
                throw new Exception();
            }
        }

        public void UpdateUserPermissionGroupId(int orgRegProgUserId, int permissionGroupId)
        {
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

            int thisUserOrgRegProgUserId = Convert.ToInt32(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
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

        public void UpdateUserSignatoryStatus(int orgRegProgUserId, bool isSignatory)
        {
            OrganizationRegulatoryProgramUser programUser = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            programUser.IsSignatory = isSignatory;
            programUser.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
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

            contentReplacements.Add("authorityName", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName));
            contentReplacements.Add("authorityOrganizationName", authority.Organization.Name);
            contentReplacements.Add("organizationName", orgRegProgram.Organization.Name);
            contentReplacements.Add("addressLine1", orgRegProgram.Organization.AddressLine1);
            contentReplacements.Add("cityName", orgRegProgram.Organization.CityName);
            contentReplacements.Add("stateName", _dbContext.Jurisdictions.Single(j => j.JurisdictionId == orgRegProgram.Organization.JurisdictionId).Code);
            contentReplacements.Add("supportEmail", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress));
            contentReplacements.Add("supportPhoneNumber", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone));

            //Email user
            EmailType emailType = isSignatory ? EmailType.Signature_SignatoryGranted : EmailType.Signature_SignatoryRevoked;
            EmailType adminEmailType = isSignatory ? EmailType.Signature_SignatoryGrantedToAdmin : EmailType.Signature_SignatoryRevokedToAdmin;
            _emailService.SendEmail(new[] { user.Email }, emailType, contentReplacements);

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
            contentReplacements.Add("userName", user.UserName);

            _crommerAuditLogService.Log(cromerrEvent, cromerrAuditLogEntryDto, contentReplacements);

            //Email all IU Admins
            var admins = _dbContext.OrganizationRegulatoryProgramUsers
                .Where(o => o.PermissionGroup.Name == "Administrator"
                && o.OrganizationRegulatoryProgramId == programUser.OrganizationRegulatoryProgram.OrganizationRegulatoryProgramId);
            foreach (var admin in admins.ToList())
            {
                var adminProfile = GetUserProfileById(admin.UserProfileId);

                contentReplacements = new Dictionary<string, string>();
                contentReplacements.Add("adminFirstName", adminProfile.FirstName);
                contentReplacements.Add("adminLastName", adminProfile.LastName);
                contentReplacements.Add("firstName", user.FirstName);
                contentReplacements.Add("lastName", user.LastName);
                contentReplacements.Add("authorityName", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName));
                contentReplacements.Add("authorityOrganizationName", authority.Organization.Name);
                contentReplacements.Add("email", user.Email);
                contentReplacements.Add("organizationName", orgRegProgram.Organization.Name);
                contentReplacements.Add("addressLine1", orgRegProgram.Organization.AddressLine1);
                contentReplacements.Add("cityName", orgRegProgram.Organization.CityName);
                contentReplacements.Add("stateName", _dbContext.Jurisdictions.Single(j => j.JurisdictionId == orgRegProgram.Organization.JurisdictionId).Code);
                contentReplacements.Add("emailAddress", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress));
                contentReplacements.Add("phoneNumber", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone));

                _emailService.SendEmail(new[] { adminProfile.Email }, adminEmailType, contentReplacements);

            }

        }

        public ResetUserResultDto ResetUser(int userProfileId, string newEmailAddress, int? targetOrgRegProgramId = null)
        {
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
            int senderOrgRegProgramId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            var newInvitation = _dbContext.Invitations.Create();
            newInvitation.InvitationId = token;
            newInvitation.InvitationDateTimeUtc = DateTimeOffset.Now;
            newInvitation.EmailAddress = user.Email;
            newInvitation.FirstName = user.FirstName;
            newInvitation.LastName = user.LastName;
            newInvitation.RecipientOrganizationRegulatoryProgramId = targetOrgRegProgramId.HasValue ? targetOrgRegProgramId.Value : senderOrgRegProgramId;
            newInvitation.SenderOrganizationRegulatoryProgramId = senderOrgRegProgramId;
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
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                .Include("OrganizationRegulatoryProgram")
                .Single(u => u.OrganizationRegulatoryProgramUserId == senderOrgRegProgramId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(actorProgramUser);
            var actorUser = this.GetUserProfileById(actorProgramUserDto.UserProfileId);

            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
            cromerrAuditLogEntryDto.RegulatoryProgramId = actorProgramUserDto.OrganizationRegulatoryProgramDto.RegulatoryProgramId;
            cromerrAuditLogEntryDto.OrganizationId = actorProgramUserDto.OrganizationRegulatoryProgramDto.OrganizationId;
            cromerrAuditLogEntryDto.RegulatorOrganizationId = actorProgramUserDto.OrganizationRegulatoryProgramDto.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
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
            contentReplacements.Add("authorityFirstame", actorUser.LastName);
            contentReplacements.Add("authorityUserName", actorUser.UserName);
            contentReplacements.Add("authorityEmailaddress", actorUser.Email);

            _crommerAuditLogService.Log(CromerrEvent.UserAccess_AccountResetInitiated, cromerrAuditLogEntryDto, contentReplacements);

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

            foreach (var program in programs.ToList())
            {
                //Find admin users in each of these
                var admins = _dbContext.OrganizationRegulatoryProgramUsers
                    .Where(o => o.PermissionGroup.Name == "Administrator" 
                    && o.OrganizationRegulatoryProgramId == program.OrganizationRegulatoryProgramId);
                foreach (var admin in admins.ToList())
                {
                    string adminEmail = GetUserProfileById(admin.UserProfileId).Email;
                    contentReplacements = new Dictionary<string, string>();
                    contentReplacements.Add("firstName", user.FirstName);
                    contentReplacements.Add("lastName", user.LastName);
                    contentReplacements.Add("userName", user.UserName);
                    contentReplacements.Add("email", user.Email);
                    _emailService.SendEmail(new[] { adminEmail }, EmailType.UserAccess_LockOutToSysAdmins, contentReplacements);

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

         
            foreach (var authority in authorityList)
            {
                //Find admin users in each of these
                var admins = _dbContext.OrganizationRegulatoryProgramUsers
                    .Where(o => o.PermissionGroup.Name == "Administrator"
                    && o.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId);
                foreach (var admin in admins.ToList())
                {
                    string adminEmail = GetUserProfileById(admin.UserProfileId).Email;
                    contentReplacements = new Dictionary<string, string>();
                    contentReplacements.Add("firstName", user.FirstName);
                    contentReplacements.Add("lastName", user.LastName);
                    contentReplacements.Add("userName", user.UserName);
                    contentReplacements.Add("email", user.Email);
                    _emailService.SendEmail(new[] { adminEmail }, EmailType.UserAccess_LockOutToSysAdmins, contentReplacements);

                }

                if (reason == AccountLockEvent.ManualAction)
                {
                    //Send to user on behalf of each program's authority
                    var authorityName = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName);
                    var authorityEmail = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress);
                    var authorityPhone = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone);

                    contentReplacements = new Dictionary<string, string>();
                    contentReplacements.Add("authorityName", authorityName);
                    contentReplacements.Add("authorityOrganizationName", authority.Organization.Name);
                    contentReplacements.Add("authoritySupportEmail", authorityEmail);
                    contentReplacements.Add("authoritySupportPhoneNumber", authorityPhone);
                    _emailService.SendEmail(new[] { user.Email }, EmailType.UserAccess_AccountLockOut, contentReplacements);

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
                _emailService.SendEmail(new[] { user.Email }, EmailType.Profile_KBQFailedLockOut, contentReplacements);

            }
        }

        public AccountLockoutResultDto LockUnlockUserAccount(int userProfileId, bool isAttemptingLock, AccountLockEvent reason)
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

            try {
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
            catch (Exception ex)
            {
                throw (ex);
            }

            if (isAttemptingLock)
            {
                SendAccountLockoutEmails(user, reason);
            }

            LogLockUnlockActivityToCromerr(isAttemptingLock, user, reason);

            //Success
            return new Dto.AccountLockoutResultDto()
            {
                IsSuccess = true
            };
        }

        private void LogLockUnlockActivityToCromerr(bool isAttemptingLock, UserProfile user, AccountLockEvent reason)
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
                    thisUserOrgRegProgUserId = Convert.ToInt32(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
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

                _crommerAuditLogService.Log(cromerrEvent, cromerrAuditLogEntryDto, contentReplacements);

            }

        }

        public void EnableDisableUserAccount(int orgRegProgramUserId, bool isAttemptingDisable)
        {
            _logService.Info($"EnableDisableUserAccount. OrgRegProgUserId={orgRegProgramUserId}, IsAttemptingDisable={isAttemptingDisable}...");

            //Check user is not THIS user's own account
            int thisUserOrgRegProgUserId = Convert.ToInt32(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
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

        public bool RemoveUser(int orgRegProgUserId)
        {
            //Ensure this is not the calling User's account
            int thisUsersOrgRegProgUserId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
            if (thisUsersOrgRegProgUserId == orgRegProgUserId)
                return false;

            OrganizationRegulatoryProgramUser user = _dbContext.OrganizationRegulatoryProgramUsers
                .SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);

            user.IsRemoved = true;
            //Persist modification date and modifier actor
            user.LastModificationDateTimeUtc = DateTime.UtcNow;
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

        public void UpdateUser(UserDto dto)
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

        public RegistrationResult UpdateProfile(UserDto dto, IEnumerable<AnswerDto> securityQuestions, IEnumerable<AnswerDto> kbqQuestions)
        {
            var validationResult = ValidateRegistrationUserData(dto, securityQuestions, kbqQuestions);

            if (validationResult != RegistrationResult.Success)
                return validationResult;

            var transaction = _dbContext.BeginTransaction();
            try
            {
                UpdateProfile(dto);
                ICollection<AnswerDto> securityQuestionCollection = securityQuestions.ToList();
                ICollection<AnswerDto> kbqQuestionCollection = kbqQuestions.ToList();
                _questionAnswerServices.CreateOrUpdateUserQuestionAnswers(dto.UserProfileId, securityQuestionCollection);
                _questionAnswerServices.CreateOrUpdateUserQuestionAnswers(dto.UserProfileId, kbqQuestionCollection);

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
            finally
            {
                transaction.Dispose();
            }

            return RegistrationResult.Success;

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

            // Test security questions mush have answer
            if (securityQuestions.Any(i => i.Content == null))
            {
                return RegistrationResult.MissingSecurityQuestionAnswer;
            }

            // Test KBQ questions mush have answer
            if (kbqQuestions.Any(i => i.Content == null))
            {
                return RegistrationResult.MissingKBQAnswer;
            }

            return RegistrationResult.Success;
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
                    return false;

                userProfile = _dbContext.Users.Single(up => up.UserProfileId == userProfileId);
                oldEmailAddress = userProfile.Email;
                userProfile.Email = newEmailAddress;
                userProfile.OldEmailAddress = oldEmailAddress;
                _dbContext.SaveChanges();
                dbContextTransaction.Commit();
            }
            catch (Exception)
            {
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

            int thisUserOrgRegProgUserId = Convert.ToInt32(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
            var actorProgramUser = _dbContext.OrganizationRegulatoryProgramUsers
                .Single(u => u.OrganizationRegulatoryProgramUserId == thisUserOrgRegProgUserId);
            var actorProgramUserDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(actorProgramUser);
            var actorUser = this.GetUserProfileById(actorProgramUserDto.UserProfileId);

            var cromerrAuditLogEntryDto = new CromerrAuditLogEntryDto();
            cromerrAuditLogEntryDto.RegulatoryProgramId = actorProgramUser.OrganizationRegulatoryProgram.RegulatoryProgramId;
            cromerrAuditLogEntryDto.OrganizationId = actorProgramUser.OrganizationRegulatoryProgram.OrganizationId;
            cromerrAuditLogEntryDto.RegulatorOrganizationId = actorProgramUser.OrganizationRegulatoryProgram.RegulatorOrganizationId ?? cromerrAuditLogEntryDto.OrganizationId;
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

            _crommerAuditLogService.Log(CromerrEvent.Profile_EmailChange, cromerrAuditLogEntryDto, contentReplacements);


            return true;
        }

        public OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUser(int orgRegProgUserId)
        {
            OrganizationRegulatoryProgramUser user = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            OrganizationRegulatoryProgramUserDto userDto = _mapHelper.GetOrganizationRegulatoryProgramUserDtoFromOrganizationRegulatoryProgramUser(user);
            userDto.UserProfileDto = GetUserProfileById(user.UserProfileId);
            var jurisdiction = _dbContext.Jurisdictions.Single(j => j.JurisdictionId == userDto.UserProfileDto.JurisdictionId);
            userDto.UserProfileDto.Jurisdiction = _mapHelper.GetJurisdictionDtoFromJurisdiction(jurisdiction);

            //Need to modify datetime to local
            userDto.UserProfileDto.CreationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(userDto.UserProfileDto.CreationDateTimeUtc.Value.DateTime, orgRegProgUserId);
            userDto.RegistrationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(userDto.RegistrationDateTimeUtc.Value.DateTime, orgRegProgUserId);

            return userDto;
        }

        public void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int userProfileId, int organizationRegulatoryProgramId,
            bool isApproved)
        {
            var user = _dbContext.OrganizationRegulatoryProgramUsers
                .SingleOrDefault(u => u.UserProfileId == userProfileId && u.OrganizationRegulatoryProgramId == organizationRegulatoryProgramId); 

            if (user == null) return;

            user.IsRegistrationApproved = isApproved;
            _dbContext.SaveChangesAsync();
        }

        public void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int orgRegProgUserId, bool isApproved)
        {
            var user = _dbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (user == null) return;

            if (isApproved)
            {
                user.IsRegistrationApproved = isApproved;
                user.IsRegistrationDenied = false;
            }
            else
            {
                user.IsRegistrationApproved = isApproved;
                user.IsRegistrationDenied = true;
            }
            _dbContext.SaveChanges(); 
        }

        public void UpdateOrganizationRegulatoryProgramUserRole(int orgRegProgUserId, int permissionGroupId)
        {
            var user = _dbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (user == null) return;

            user.PermissionGroupId = permissionGroupId;
            _dbContext.SaveChanges();

        }

        public RegistrationResultDto ApprovePendingRegistration(int orgRegProgUserId, int permissionGroupId, bool isApproved)
        {
            var programUser = GetOrganizationRegulatoryProgramUser(orgRegProgUserId);
            int orgRegProgramId = programUser.OrganizationRegulatoryProgramId;
            var authority = _orgService.GetAuthority(orgRegProgramId);
            bool isAuthorityUser = true;
            if (authority != null && authority.OrganizationRegulatoryProgramId != orgRegProgramId)
            {
                //We know this is "industry user", otherwise the authority would be the same THIS user's org.
                isAuthorityUser = false;
            }

            if (!isAuthorityUser) // ALLOW THIS ACTION OR DISALLOW (UC-7.1, 5.b)
            {
                //Get the list of IU admin users for this industry user's organization
                var allProgramUsers = GetUserProfilesForOrgRegProgram(orgRegProgramId, true, null, true, false);
                var IUAdmins = allProgramUsers.Where(u => u.PermissionGroup.Name.StartsWith("Admin"));

                //Is the logged in actor user in the set of IU admins?
                var isCurrentUserIUAdmin = false;
                int loggedInUsersOrgRegProgUserId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
                if (IUAdmins != null && IUAdmins.Any(x => x.OrganizationRegulatoryProgramUserId == loggedInUsersOrgRegProgUserId))
                {
                    //Logged in user is one of the IU Admins
                    isCurrentUserIUAdmin = true;
                }

                //Is the currently logged in user an Authority User?
                var isCurrentUserAuthorityUser = false;
                if (_dbContext.OrganizationRegulatoryProgramUsers
                    .Any(o => o.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId
                    && o.OrganizationRegulatoryProgramUserId == loggedInUsersOrgRegProgUserId))
                    isCurrentUserAuthorityUser = true;

                //Is this a re-reg from a reset? (UC-7.1, 5.c)
                var isResetScenario = false;
                if (programUser.UserProfileDto.IsAccountResetRequired)
                    isResetScenario = true;

                if (isResetScenario)
                {
                    if (!isCurrentUserAuthorityUser)
                    {
                        //ACTION BLOCKED -- ONLY AUTHORITY CAN APPROVE
                        return new RegistrationResultDto() { Result = RegistrationResult.UnauthorizedApprovalAfterReset };
                    }

                    //AUTHORITY CANNOT CHANGE PREVIOUS ROLE
                    if (programUser.PermissionGroup.PermissionGroupId != permissionGroupId)
                        return new RegistrationResultDto() { Result = RegistrationResult.ApprovalAfterResetCannotChangeRole };
                }
                else
                {
                    if (IUAdmins != null && IUAdmins.Count() > 1 && !isCurrentUserIUAdmin)
                    {
                        //ACTION BLOCKED -- ONLY AUTHORITY CAN APPROVE
                        return new RegistrationResultDto() { Result = RegistrationResult.UnauthorizedNotIUAdmin };
                    }
                }


            }

            //Check user license count
            if (isApproved)
            {
                var remainingUserLicenseCount = _orgService.GetRemainingUserLicenseCount(orgRegProgramId);
                if (remainingUserLicenseCount < 1)
                {
                    //ACTION BLOCKED -- NO MORE USER LICENSES
                    if (isAuthorityUser)
                        return new RegistrationResultDto() { Result = RegistrationResult.NoMoreUserLicensesForAuthority };
                    else
                        return new RegistrationResultDto() { Result = RegistrationResult.NoMoreUserLicensesForIndustry };
                }

            }

            var transaction = _dbContext.BeginTransaction();
            try
            {
                UpdateOrganizationRegulatoryProgramUserApprovedStatus(orgRegProgUserId, isApproved);
                UpdateOrganizationRegulatoryProgramUserRole(orgRegProgUserId, permissionGroupId);
                transaction.Commit();
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                throw ex;
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
                if (isAuthorityUser)
                    emailType = EmailType.Registration_AuthorityRegistrationApproved;
                else
                    emailType = EmailType.Registration_IndustryRegistrationApproved;

                cromerrEvent = CromerrEvent.Registration_RegistrationApproved;
            }
            else
            {
                //Authority or Industry?
                if (isAuthorityUser)
                    emailType = EmailType.Registration_AuthorityRegistrationDenied;
                else
                    emailType = EmailType.Registration_IndustryRegistrationDenied;

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
                contentReplacements.Add("stateName", _dbContext.Jurisdictions.Single(j => j.JurisdictionId == org.JurisdictionId).Code);
            }

            if (isApproved)
            {
                string baseUrl = _httpContext.GetRequestBaseUrl();
                string link = baseUrl + "Account/SignIn";
                contentReplacements.Add("link", link);
            }

            _emailService.SendEmail(new[] { programUser.UserProfileDto.Email }, emailType, contentReplacements);

            //Cromerr log
            int thisUserOrgRegProgUserId = Convert.ToInt32(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
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
            contentReplacements.Add("authorityName", authorityName);
            contentReplacements.Add("organizationName", authority.OrganizationDto.OrganizationName);
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

        #endregion
    }
}
