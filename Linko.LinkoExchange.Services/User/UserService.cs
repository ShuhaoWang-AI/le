﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
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

namespace Linko.LinkoExchange.Services.User
{
    public class UserService : IUserService
    {
        #region private members

        private readonly LinkoExchangeContext _dbContext;
        private readonly IAuditLogEntry _logger;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IHttpContextService _httpContext;
        private readonly IEmailService _emailService;
        private readonly ISettingService _settingService;
        private readonly ISessionCache _sessionCache;
        private readonly IOrganizationService _orgService;
        private readonly IRequestCache _requestCache;
        private readonly IDictionary<SystemSettingType, string> _globalSettings;  
        private readonly ITimeZoneService _timeZones;


        #endregion

        #region constructor

        public UserService(LinkoExchangeContext dbContext, IAuditLogEntry logger,
            IPasswordHasher passwordHasher, IMapper mapper, IHttpContextService httpContext,
            IEmailService emailService, ISettingService settingService, ISessionCache sessionCache,
            IOrganizationService orgService, IRequestCache requestCache, ITimeZoneService timeZones)
        {
            this._dbContext = dbContext;
            _logger = logger;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _httpContext = httpContext;
            _emailService = emailService;
            _settingService = settingService;
            _sessionCache = sessionCache;
            _orgService = orgService;
            _requestCache = requestCache;
            _globalSettings = _settingService.GetGlobalSettings();
            _timeZones = timeZones;
        }

        #endregion

        #region public methods
        

        public UserDto GetUserProfileById(int userProfileId)
        {
            var userProfile = _dbContext.Users.Single(up => up.UserProfileId == userProfileId);
            UserDto dto = _mapper.Map<UserProfile, UserDto>(userProfile);
            return dto;
        }

        public UserDto GetUserProfileByEmail(string emailAddress)
        {
            UserDto dto = null;
            UserProfile userProfile = _dbContext.Users.SingleOrDefault(u => u.Email == emailAddress);
            if (userProfile != null)
            {
                dto = _mapper.Map<UserProfile, UserDto>(userProfile);
                return dto;
            }
            else
            {
                return null;
            }
        }

        public ICollection<OrganizationRegulatoryProgramUserDto> GetProgramUsersByEmail(string emailAddress)
        {
            var dtos = new List<OrganizationRegulatoryProgramUserDto>();
            UserProfile userProfile = _dbContext.Users.SingleOrDefault(u => u.Email == emailAddress);
            if (userProfile != null)
            {
                var programUsers = _dbContext.OrganizationRegulatoryProgramUsers.Where(o => o.UserProfileId == userProfile.UserProfileId).ToList();
                if (programUsers != null)
                {
                    foreach (var programUser in programUsers)
                    {
                        var dto = _mapper.Map<OrganizationRegulatoryProgramUser, OrganizationRegulatoryProgramUserDto>(programUser);
                        //Need to map UserProfileDto manually here
                        dto.UserProfileDto = this.GetUserProfileById(programUser.UserProfileId);
                        dtos.Add(dto);


                    }
                }
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
            var authority = _orgService.GetAuthority(orgRegProgramId);
            int timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(authority.OrganizationId, SettingType.TimeZone));
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(_timeZones.GetTimeZoneName(timeZoneId));

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
                var dto = _mapper.Map<OrganizationRegulatoryProgramUser, OrganizationRegulatoryProgramUserDto>(user);
                //manually map user profile child
                var userProfileDto = GetUserProfileById(user.UserProfileId);
                
                //Need to modify datetime to local
                dto.RegistrationDateTime = TimeZoneInfo.ConvertTimeFromUtc(dto.RegistrationDateTime.Value.UtcDateTime, authorityLocalZone);
                userProfileDto.CreationDateTimeUtc = TimeZoneInfo.ConvertTimeFromUtc(userProfileDto.CreationDateTimeUtc.Value.UtcDateTime, authorityLocalZone);

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

            try
            {
                return _dbContext.SaveChanges();
            }
            catch (Exception ex) {
                //_logger.Log("ERROR")
                throw new Exception();
            }
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
                user.LastModifierUserId = Convert.ToInt32(_httpContext.Current().User.Identity.UserProfileId());
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
            var user = _dbContext.OrganizationRegulatoryProgramUsers.Single(o => o.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            user.PermissionGroupId = permissionGroupId;
            _dbContext.SaveChanges();
        }

        public void UpdateUserSignatoryStatus(int orgRegProgUserId, bool isSignatory)
        {
            OrganizationRegulatoryProgramUser programUser = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            programUser.IsSignatory = isSignatory;
            programUser.LastModificationDateTimeUtc = DateTimeOffset.UtcNow;
            programUser.LastModifierUserId = _httpContext.CurrentUserProfileId();
            _dbContext.SaveChanges();

            var user = GetUserProfileById(programUser.UserProfileId);
            var org = programUser.OrganizationRegulatoryProgram.Organization;
            var authority = programUser.OrganizationRegulatoryProgram.RegulatorOrganization;
            if (authority == null)
            {
                //This IS an authority
                authority = org;
            }

            var contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("userName", user.UserName);
            contentReplacements.Add("authorityName", _settingService.GetOrganizationSettingValue(authority.OrganizationId, SettingType.EmailContactInfoName));
            contentReplacements.Add("organizationName", org.Name);
            contentReplacements.Add("addressLine1", org.AddressLine1);
            contentReplacements.Add("cityName", org.CityName);
            contentReplacements.Add("stateName", _dbContext.Jurisdictions.Single(j => j.JurisdictionId == org.JurisdictionId).Name);
            contentReplacements.Add("emailAddress", _settingService.GetOrganizationSettingValue(authority.OrganizationId, SettingType.EmailContactInfoEmailAddress));
            contentReplacements.Add("phoneNumber", _settingService.GetOrganizationSettingValue(authority.OrganizationId, SettingType.EmailContactInfoPhone));

            //Email user
            EmailType emailType = isSignatory ? EmailType.Signature_SignatoryGranted : EmailType.Signature_SignatoryRevoked;
            EmailType adminEmailType = isSignatory ? EmailType.Signature_SignatoryGrantedToAdmin : EmailType.Signature_SignatoryRevokedToAdmin;
            _emailService.SendEmail(new[] { user.Email }, emailType, contentReplacements);

            //Email all IU Admins
            var admins = _dbContext.OrganizationRegulatoryProgramUsers
                .Where(o => o.PermissionGroup.Name == "Administrator"
                && o.OrganizationRegulatoryProgramId == programUser.OrganizationRegulatoryProgram.OrganizationRegulatoryProgramId);
            foreach (var admin in admins.ToList())
            {
                var adminProfile = GetUserProfileById(admin.UserProfileId);

                contentReplacements = new Dictionary<string, string>();
                contentReplacements.Add("adminFirstName", adminProfile.FirstName);
                contentReplacements.Add("adminLastName", adminProfile.FirstName);
                contentReplacements.Add("firstName", user.FirstName);
                contentReplacements.Add("lastName", user.LastName);
                contentReplacements.Add("userName", user.UserName);
                contentReplacements.Add("authorityName", _settingService.GetOrganizationSettingValue(authority.OrganizationId, SettingType.EmailContactInfoName));
                contentReplacements.Add("email", user.Email);
                contentReplacements.Add("organizationName", org.Name);
                contentReplacements.Add("addressLine1", org.AddressLine1);
                contentReplacements.Add("cityName", org.CityName);
                contentReplacements.Add("stateName", _dbContext.Jurisdictions.Single(j => j.JurisdictionId == org.JurisdictionId).Name);
                contentReplacements.Add("emailAddress", _settingService.GetOrganizationSettingValue(authority.OrganizationId, SettingType.EmailContactInfoEmailAddress));
                contentReplacements.Add("phoneNumber", _settingService.GetOrganizationSettingValue(authority.OrganizationId, SettingType.EmailContactInfoPhone));

                _emailService.SendEmail(new[] { adminProfile.Email }, adminEmailType, contentReplacements);

            }

            //Cromerr Log TO DO

        }

        public ResetUserResultDto ResetUser(int userProfileId, string newEmailAddress)
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
                if (GetUserProfileByEmail(newEmailAddress) == null)
                    return new ResetUserResultDto()
                    {
                        IsSuccess = false,
                        FailureReason = ResetUserFailureReason.NewEmailAddressAlreadyInUse
                    };

                user.OldEmailAddress = user.Email;
                user.Email = newEmailAddress;
                sendEmailChangedNotifications.Add(user.OldEmailAddress);
                sendEmailChangedNotifications.Add(user.Email);

                user.EmailConfirmed = false;
            }

            //reset flags
            user.IsAccountLocked = false;
            user.PasswordHash = null; 
            user.IsAccountResetRequired = true;

            //Delete SQs and KBQs
            var answers = _dbContext.UserQuestionAnswers
                .Include("Question").Where(a => a.UserProfileId == userProfileId);
            if (answers != null && answers.Count() > 0)
            {
                foreach (var answer in answers)
                {
                    //get question too
                    _dbContext.Questions.Remove(answer.Question);

                    _dbContext.UserQuestionAnswers.Remove(answer);
                }
            }

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
                contentReplacements.Add("userName", user.UserName);
                contentReplacements.Add("oldEmail", user.OldEmailAddress);
                contentReplacements.Add("newEmail", user.Email);
                contentReplacements.Add("authorityList", authorityList);
                contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
                contentReplacements.Add("supportEmail", supportEmail);
                _emailService.SendEmail(new[] { email }, EmailType.Profile_EmailChanged, contentReplacements);
            }

            //2) Send "Reg Reset" Email
            var token = Guid.NewGuid().ToString();
            string baseUrl = _httpContext.GetRequestBaseUrl();
            string link = baseUrl + "Account/ResetRegistration/?token=" + token;
            contentReplacements = new Dictionary<string, string>();
            contentReplacements.Add("link", link);
            contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
            contentReplacements.Add("supportEmail", supportEmail);
            _requestCache.SetValue(CacheKey.Token, token);
            _emailService.SendEmail(new[] { user.Email }, EmailType.Profile_ResetProfileRequired, contentReplacements);


            return new ResetUserResultDto()
            {
                IsSuccess = true
            };

        }

        private void SendAccountLockoutEmails(UserProfile user)
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

                if (program.RegulatorOrganizationId != null)
                {
                    //Find distinct authorities
                    var authority = _dbContext.OrganizationRegulatoryPrograms
                        .Single(o => o.OrganizationId == program.RegulatorOrganizationId
                            && o.RegulatoryProgramId == program.RegulatoryProgramId);
                    if (!(authorityList.Exists(a => a.OrganizationRegulatoryProgramId == authority.OrganizationRegulatoryProgramId)))
                        authorityList.Add(authority);

                }
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


                //Send to user on behalf of each program's authority
                var authorityName = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoName);
                var authorityEmail = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress);
                var authorityPhone = _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone);

                contentReplacements = new Dictionary<string, string>();
                contentReplacements.Add("authorityName", authorityName);
                contentReplacements.Add("emailAddress", authorityEmail);
                contentReplacements.Add("phoneNumber", authorityPhone);
                _emailService.SendEmail(new[] { user.Email }, EmailType.UserAccess_AccountLockOut, contentReplacements);

            }


        }

        public AccountLockoutResultDto LockUnlockUserAccount(int userProfileId, bool isAttemptingLock)
        {
            var user = _dbContext.Users.Single(u => u.UserProfileId == userProfileId);
            //Check user is not support role
            if (user.IsInternalAccount && isAttemptingLock)
                return new Dto.AccountLockoutResultDto()
                {
                    IsSuccess = false,
                    FailureReason = AccountLockoutFailureReason.CannotLockoutSupportRole
                };

            //Check user is not THIS user's own account
            int thisUserProfileId = _httpContext.CurrentUserProfileId();
            if (thisUserProfileId > 0 && userProfileId == thisUserProfileId)
                return new Dto.AccountLockoutResultDto()
                {
                    IsSuccess = false,
                    FailureReason = AccountLockoutFailureReason.CannotLockoutOwnAccount
                };

            user.IsAccountLocked = isAttemptingLock;
            _dbContext.SaveChanges();

            if (isAttemptingLock)
                SendAccountLockoutEmails(user);

            //Success
            return new Dto.AccountLockoutResultDto()
            {
                IsSuccess = true
            };
        }

        public bool EnableDisableUserAccount(int orgRegProgramUserId, bool isAttemptingDisable)
        {
            //Check user is not THIS user's own account
            int thisUserOrgRegProgUserId = Convert.ToInt32(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramUserId));
            if (orgRegProgramUserId == thisUserOrgRegProgUserId)
                return false;

            var user = _dbContext.OrganizationRegulatoryProgramUsers
                .Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgramUserId);
            user.IsEnabled = isAttemptingDisable;
            _dbContext.SaveChanges();

            //TO DO: Log user access enable to Audit (UC-2)

            return true;
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
            var thisUsersOrgRegProgUserId = Convert.ToInt32(_httpContext.GetSessionValue(CacheKey.OrganizationRegulatoryProgramUserId));
            if (thisUsersOrgRegProgUserId == orgRegProgUserId)
                return false;

            OrganizationRegulatoryProgramUser user = _dbContext.OrganizationRegulatoryProgramUsers
                .SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);

            user.IsRemoved = true;
            //Persist modification date and modifier actor
            user.LastModificationDateTimeUtc = DateTime.UtcNow;
            user.LastModifierUserId = Convert.ToInt32(_httpContext.Current().User.Identity.UserProfileId());
            _dbContext.SaveChanges();

            return true;
        }

        public void UpdateUser(UserDto dto)
        {
            UserProfile userProfile = _dbContext.Users.Single(up => up.UserProfileId == dto.UserProfileId);
            userProfile = _mapper.Map<UserDto, UserProfile>(dto);

            //Additional manual mappings here

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
            contentReplacements.Add("userName", dto.UserName);
            contentReplacements.Add("authorityList", authorityList);
            contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
            contentReplacements.Add("supportEmail", supportEmail);
            _emailService.SendEmail(new[] { dto.Email }, EmailType.Profile_ProfileChanged, contentReplacements);
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
            contentReplacements.Add("userName", userProfile.UserName);
            contentReplacements.Add("oldEmail", oldEmailAddress);
            contentReplacements.Add("newEmail", newEmailAddress);
            contentReplacements.Add("authorityList", authorityList);
            contentReplacements.Add("supportPhoneNumber", supportPhoneNumber);
            contentReplacements.Add("supportEmail", supportEmail);
            _emailService.SendEmail(new[] { oldEmailAddress }, EmailType.Profile_EmailChanged, contentReplacements);
            _emailService.SendEmail(new[] { newEmailAddress }, EmailType.Profile_EmailChanged, contentReplacements);

            return true;
        }

        public OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUser(int orgRegProgUserId)
        {
            var user = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            var dto = _mapper.Map<OrganizationRegulatoryProgramUserDto>(user);
            dto.UserProfileDto = GetUserProfileById(user.UserProfileId);

            //Need to modify datetime to local
            var authority = _orgService.GetAuthority(dto.OrganizationRegulatoryProgramId);
            int timeZoneId = Convert.ToInt32(_settingService.GetOrganizationSettingValue(authority.OrganizationId, SettingType.TimeZone));
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(_timeZones.GetTimeZoneName(timeZoneId));
            dto.UserProfileDto.CreationDateTimeUtc = TimeZoneInfo.ConvertTimeFromUtc(dto.UserProfileDto.CreationDateTimeUtc.Value.UtcDateTime, authorityLocalZone);

            return dto;
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

            user.IsRegistrationApproved = isApproved;
            _dbContext.SaveChangesAsync(); 
        }


        #endregion


    }
}
