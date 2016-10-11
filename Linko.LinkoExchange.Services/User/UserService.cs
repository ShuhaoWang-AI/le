﻿using Linko.LinkoExchange.Core.Domain;
using Linko.LinkoExchange.Data;
using Linko.LinkoExchange.Services.AuditLog;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void SendEmailCommunication(int userProfileId, UserEmailCommunicationType type, string emailAddressOverride = null)
        {
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

        public void UpdateUserSignatoryStatus(bool isSignatory)
        {
        }

        public void RequestSignatoryStatus(int userProfileId)
        {
        }

        public void ResetUser(int userProfileId)
        {
        }

        public void RemoveUser(int userProfileId)
        {
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