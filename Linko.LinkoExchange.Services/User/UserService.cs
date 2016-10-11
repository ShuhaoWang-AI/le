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
        

        public UserProfileDTO GetUserProfileById(int userProfileId)
        {
            return null;
        }

        public UserProfileDTO GetUserProfileByEmail(string emailAddress)
        {
            return null;
        }

        public List<UserProfileDTO> GetUserProfilesForOrgRegProgram(int organizationRegulatoryProgramId,
                             bool? isRegApproved,
                             bool? isRegDenied,
                             bool? isEnabled,
                             bool? isRemoved)
        {
            return null;
        }


        public int AddNewUser(string emailAddress, string firstName, string lastName)
        {
            return -1;
        }

        public void UpdateUserProfileState(int userProfileId, bool? isRegApproved, bool? isRegDenied, bool? isEnabled, bool? isRemoved)
        {
        }

        public void UpdateUserPermissionGroupId(int userProfileId, int permissionGroupId)
        {
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

        public void RequestSignatoryStatus(int userProfileId)
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
                        var question = _dbContext.Questions.Single(q => q.QuestionId == answer.QuestionId);
                        _dbContext.Questions.Remove(question);

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

        public void UpdateUserProfile(UserProfileDTO request)
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

        public void UpdateQuestionAnswerPairs(UserQuestionAnswerPairsDTO questionAnswerPairs)
        {
        }

        #endregion


    }
}
