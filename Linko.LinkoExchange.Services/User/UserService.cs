using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Linko.LinkoExchange.Core.Extensions;
using Linko.LinkoExchange.Services.Dto;
using AutoMapper;
using System.Web;
using Linko.LinkoExchange.Services.Cache;
using Linko.LinkoExchange.Services;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Email;
using Linko.LinkoExchange.Services.Settings;
using Linko.LinkoExchange.Services.Authentication;
using Linko.LinkoExchange.Services.Organization;

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

        #endregion

        #region constructor

        public UserService(LinkoExchangeContext dbContext, IAuditLogEntry logger,
            IPasswordHasher passwordHasher, IMapper mapper, IHttpContextService httpContext,
            IEmailService emailService, ISettingService settingService, ISessionCache sessionCache,
            IOrganizationService orgService)
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
            string timeZone = _settingService.GetOrganizationSettingValue(authority.OrganizationId, SettingType.TimeZone);
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);

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

        public void ResetUser(int userProfileId, string newEmailAddress)
        {
            UserProfile user = _dbContext.Users.SingleOrDefault(u => u.UserProfileId == userProfileId);
            if (user != null)
            {
                user.IsAccountLocked = false;
                user.PasswordHash = null;
                user.OldEmailAddress = user.Email;
                user.Email = newEmailAddress;
                user.EmailConfirmed = false;
                user.PhoneNumberConfirmed = false;

                var answers = _dbContext.UserQuestionAnswers.Where(a => a.UserProfileId == userProfileId);
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
            }
            else
            {
                //_logger.Log("ERROR")
                throw new Exception();
            }
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
            if (userProfileId == thisUserProfileId)
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

        public void RemoveUser(int orgRegProgUserId)
        {
            OrganizationRegulatoryProgramUser user = _dbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (user != null)
            {
                user.IsRemoved = true;

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

        public void UpdateUser(UserDto request)
        {
            if (request != null)
            {
                UserProfile userProfile = _dbContext.Users.SingleOrDefault(up => up.UserProfileId == request.UserProfileId);
                if (userProfile != null)
                {
                    userProfile.FirstName = request.FirstName;
                    //TODO: map other fields
                    _dbContext.SaveChanges();
                }
                else
                {
                    //_logger.Log("ERROR")
                    throw new Exception();
                }
            }
            {
                //_logger.Log("ERROR")
                throw new Exception();
            }
        }

        public void ChangePassword(int userProfileId, string oldPassword, string newPassword)
        {
            UserProfile userProfile = _dbContext.Users.SingleOrDefault(up => up.UserProfileId == userProfileId);
            if (userProfile != null)
            {
                //Check old password matches
                if (userProfile.PasswordHash == _passwordHasher.HashPassword(oldPassword))
                {
                    userProfile.PasswordHash = _passwordHasher.HashPassword(newPassword);

                    //create history record
                    UserPasswordHistory history = _dbContext.UserPasswordHistories.Create();
                    history.UserProfileId = userProfileId;
                    history.PasswordHash = userProfile.PasswordHash;
                    history.LastModificationDateTimeUtc = DateTime.UtcNow;

                    _dbContext.SaveChanges();
                }
            }
            else
            {
                throw new Exception("ERROR: Old password does not match");
            }

        }

        public OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUser(int orgRegProgUserId)
        {
            var user = _dbContext.OrganizationRegulatoryProgramUsers.Single(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            var dto = _mapper.Map<OrganizationRegulatoryProgramUserDto>(user);
            dto.UserProfileDto = GetUserProfileById(user.UserProfileId);

            //Need to modify datetime to local
            var authority = _orgService.GetAuthority(dto.OrganizationRegulatoryProgramId);
            string timeZone = _settingService.GetOrganizationSettingValue(authority.OrganizationId, SettingType.TimeZone);
            TimeZoneInfo authorityLocalZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
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
