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

namespace Linko.LinkoExchange.Services.User
{
    public class UserService : IUserService
    {
        #region private members

        private readonly ApplicationDbContext _dbContext;
        private readonly IAuditLogEntry _logger;
        private readonly IPasswordHasher _passwordHasher;

        #endregion

        #region constructor

        public UserService(ApplicationDbContext dbContext, IAuditLogEntry logger, IPasswordHasher passwordHasher)
        {
            this._dbContext = dbContext;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        #endregion

        #region public methods
        

        public UserDto GetUserProfileById(int userProfileId)
        {
            var dto = new UserDto();

            var userProfile = _dbContext.UserProfiles.Single(up => up.UserProfileId == userProfileId);

            dto.FirstName = userProfile.FirstName;
            dto.LastName = userProfile.LastName;
            //TODO map remaining properties

            return dto;
        }

        public UserDto GetUserProfileByEmail(string emailAddress)
        {
            UserDto dto = null;
            UserProfile userProfile = _dbContext.UserProfiles.SingleOrDefault(u => u.Email == emailAddress);
            if (userProfile != null)
            {
                dto.FirstName = userProfile.FirstName;
                dto.LastName = userProfile.LastName;
                //TODO map remaining properties
                return dto;
            }
            else
            {
                //_logger.Log("ERROR")
                throw new Exception();
            }
        }

        public List<UserDto> GetUserProfilesForOrgRegProgram(int orgRegProgramId,
                             bool? isRegApproved,
                             bool? isRegDenied,
                             bool? isEnabled,
                             bool? isRemoved)
        {
            var dtos = new List<UserDto>();
            IQueryable<OrganizationRegulatoryProgramUser> users = _dbContext.OrganizationRegulatoryProgramUsers.Where(u => u.OrganizationRegulatoryProgramId == orgRegProgramId);

            if (isRegApproved.HasValue)
                users = users.Where(u => u.IsRegistrationApproved == isRegApproved);
            if (isRegDenied.HasValue)
                users = users.Where(u => u.IsRegistrationDenied == isRegDenied);
            if (isEnabled.HasValue)
                users = users.Where(u => u.IsEnabled == isEnabled);
            if (isRemoved.HasValue)
                users = users.Where(u => u.IsRemoved == isRemoved);

            foreach (var user in users)
            {
                var dto = new UserDto();
                dto.IsEnabled = user.IsEnabled;
                //TODO map remaining properties
                dtos.Add(dto);
            }

            return dtos;
        }


        public int AddNewUser(int orgRegProgId, int permissionGroupId, string emailAddress, string firstName, string lastName)
        {
            var newOrgRegProgUser = _dbContext.OrganizationRegulatoryProgramUsers.Create();
            newOrgRegProgUser.OrganizationRegulatoryProgramId = orgRegProgId;
            newOrgRegProgUser.PermissionGroupId = permissionGroupId;

            var newUserProfile = new UserProfile();
            newUserProfile.Email = emailAddress;
            newUserProfile.FirstName = firstName;
            newUserProfile.LastName = lastName;
            newOrgRegProgUser.UserProfile = newUserProfile;

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
                user.LastModificationDateTime = DateTime.Now;
                user.LastModificationUserId = System.Web.HttpContext.Current.User.Identity.GetOrganizationRegulatoryProgramUserId();
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
            OrganizationRegulatoryProgramUser user = _dbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (user != null)
            {
                user.IsSignatory = isSignatory;

                //Persist modification date and modifier actor
                user.LastModificationDateTime = DateTime.Now;
                user.LastModificationUserId = System.Web.HttpContext.Current.User.Identity.GetOrganizationRegulatoryProgramUserId();
                _dbContext.SaveChanges();
            }
            else
            {
                //_logger.Log("ERROR")
                throw new Exception();
            }
        }

        public void RequestSignatoryStatus(int orgRegProgUserId)
        {
        }

        public void ResetUser(int userProfileId, string newEmailAddress)
        {
            UserProfile user = _dbContext.UserProfiles.SingleOrDefault(u => u.UserProfileId == userProfileId);
            if (user != null)
            {
                user.IsAccountLocked = false;
                user.PasswordHash = null;
                user.OldEmailAddress = user.Email;
                user.Email = newEmailAddress;
                user.IsEmailConfirmed = false;
                user.IsPhoneNumberConfirmed = false;

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

        public void RemoveUser(int orgRegProgUserId)
        {
            OrganizationRegulatoryProgramUser user = _dbContext.OrganizationRegulatoryProgramUsers.SingleOrDefault(u => u.OrganizationRegulatoryProgramUserId == orgRegProgUserId);
            if (user != null)
            {
                user.IsRemoved = true;

                //Persist modification date and modifier actor
                user.LastModificationDateTime = DateTime.Now;
                user.LastModificationUserId = System.Web.HttpContext.Current.User.Identity.GetOrganizationRegulatoryProgramUserId();
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
                UserProfile userProfile = _dbContext.UserProfiles.SingleOrDefault(up => up.UserProfileId == request.UserProfileId);
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
            UserProfile userProfile = _dbContext.UserProfiles.SingleOrDefault(up => up.UserProfileId == userProfileId);
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
                    history.LastModificationDateTime = DateTime.Now;

                    _dbContext.SaveChanges();
                }
                else
                {
                    //_logger.Log("ERROR")
                    throw new Exception();
                }

            }
            else
            {
                //_logger.Log("ERROR")
                throw new Exception();
            }
        }

        #endregion


    }
}
