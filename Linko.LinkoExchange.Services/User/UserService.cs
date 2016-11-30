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
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.QuestionAnswer;
using System.Data.Entity.Validation;
using Linko.LinkoExchange.Core.Validation;
using System.Data.Entity.Infrastructure;

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
        private readonly IQuestionAnswerService _questionAnswerServices; 

        #endregion

        #region constructor

        public UserService(LinkoExchangeContext dbContext, IAuditLogEntry logger,
            IPasswordHasher passwordHasher, IMapper mapper, IHttpContextService httpContext,
            IEmailService emailService, ISettingService settingService, ISessionCache sessionCache,
            IOrganizationService orgService, IRequestCache requestCache, ITimeZoneService timeZones,
             IQuestionAnswerService questionAnswerServices)
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
            _questionAnswerServices = questionAnswerServices;
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
            UserProfile userProfile = _dbContext.Users.SingleOrDefault(u => u.Email == emailAddress && u.EmailConfirmed == true);
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

        public List<OrganizationRegulatoryProgramUserDto> GetPendingRegistrationProgramUsers(int orgRegProgramId)
        {
            var dtos = new List<OrganizationRegulatoryProgramUserDto>();

            var users = _dbContext.OrganizationRegulatoryProgramUsers
                .Where(u => u.InviterOrganizationRegulatoryProgramId == orgRegProgramId
                && u.IsRegistrationApproved == false && u.IsRegistrationDenied == false
                && u.IsRemoved == false);

            foreach (var user in users.ToList())
            {
                var dto = _mapper.Map<OrganizationRegulatoryProgramUser, OrganizationRegulatoryProgramUserDto>(user);
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
                var dto = _mapper.Map<OrganizationRegulatoryProgramUser, OrganizationRegulatoryProgramUserDto>(user);
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
            contentReplacements.Add("emailAddress", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoEmailAddress));
            contentReplacements.Add("phoneNumber", _settingService.GetOrgRegProgramSettingValue(authority.OrganizationRegulatoryProgramId, SettingType.EmailContactInfoPhone));

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

            //Cromerr Log TO DO

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

                    user.EmailConfirmed = false;
                }
            }

            //reset flags
            user.IsAccountLocked = false;
            user.PasswordHash = null; 
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


            return new ResetUserResultDto()
            {
                IsSuccess = true
            };

        }

        private void SendAccountLockoutEmails(UserProfile user, bool isForFailedKBQs)
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

                if (!isForFailedKBQs)
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

            if (isForFailedKBQs)
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

        public AccountLockoutResultDto LockUnlockUserAccount(int userProfileId, bool isAttemptingLock, bool isForFailedKBQs)
        {
            var user = _dbContext.Users.Single(u => u.UserProfileId == userProfileId);
            //Check user is not support role
            if (user.IsInternalAccount && isAttemptingLock && !isForFailedKBQs)
                return new Dto.AccountLockoutResultDto()
                {
                    IsSuccess = false,
                    FailureReason = AccountLockoutFailureReason.CannotLockoutSupportRole
                };

            //Check user is not THIS user's own account
            int thisUserProfileId = _httpContext.CurrentUserProfileId();
            if (thisUserProfileId > 0 && userProfileId == thisUserProfileId   && !isForFailedKBQs)
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
                SendAccountLockoutEmails(user, isForFailedKBQs);

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

            return true;
        }

        public void UpdateUser(UserDto dto)
        {
            UserProfile userProfile = _dbContext.Users.Single(up => up.UserProfileId == dto.UserProfileId);
            userProfile = _mapper.Map<UserDto, UserProfile>(dto, userProfile);

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

            return true;
        }

        public OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUser(int orgRegProgUserId)
        {
            var user = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            var dto = _mapper.Map<OrganizationRegulatoryProgramUserDto>(user);
            dto.UserProfileDto = GetUserProfileById(user.UserProfileId);

            //Need to modify datetime to local
            dto.UserProfileDto.CreationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(dto.UserProfileDto.CreationDateTimeUtc.Value.DateTime, orgRegProgUserId);
            dto.RegistrationDateTimeUtc = _timeZones.GetLocalizedDateTimeUsingSettingForThisOrg(dto.RegistrationDateTimeUtc.Value.DateTime, orgRegProgUserId);

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
                int loggedInUsersOrgRegProgUserId = int.Parse(_sessionCache.GetClaimValue(CacheKey.OrganizationRegulatoryProgramId));
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
            if (isApproved)
            {
                //Authority or Industry?
                if (isAuthorityUser)
                    emailType = EmailType.Registration_AuthorityRegistrationApproved;
                else
                    emailType = EmailType.Registration_IndustryRegistrationApproved;
            }
            else
            {
                //Authority or Industry?
                if (isAuthorityUser)
                    emailType = EmailType.Registration_AuthorityRegistrationDenied;
                else
                    emailType = EmailType.Registration_IndustryRegistrationDenied;
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


            return new RegistrationResultDto() { Result = RegistrationResult.Success };

        }
 
        #endregion 
    }
}
