using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.User
{

    public interface IUserService
    {
        UserDto GetUserProfileById(int userProfileId);

        UserDto GetUserProfileByEmail(string emailAddress);

        ICollection<OrganizationRegulatoryProgramUserDto> GetProgramUsersByEmail(string emailAddress);

        int GetPendingRegistrationProgramUsersCount(int orgRegProgamId);
        List<OrganizationRegulatoryProgramUserDto> GetPendingRegistrationProgramUsers(int orgRegProgramId);

        List<OrganizationRegulatoryProgramUserDto> GetUserProfilesForOrgRegProgram(int orgRegProgramId,
                             bool? isRegApproved,
                             bool? isRegDenied,
                             bool? isEnabled,
                             bool? isRemoved);

        int AddNewUser(int orgRegProgId, int permissionGroupId, string emailAddress, string firstName, string lastName);

        void UpdateUserState(int orgRegProgUserId, bool? isRegApproved, bool? isRegDenied, bool? isEnabled, bool? isRemoved);

        void UpdateUserPermissionGroupId(int orgRegProgUserId, int permissionGroupId, bool isAuthorizationRequired = false);

        void UpdateUserSignatoryStatus(int orgRegProgUserId, bool isSignatory, bool isAuthorizationRequired = false);

        ResetUserResultDto ResetUser(int targetOrgRegProgUserId, string newEmailAddress, bool isAuthorizationRequired = false);

        AccountLockoutResultDto LockUnlockUserAccount(int userProfileId, bool isAttemptingLock, AccountLockEvent reason, int? reportPackageId = null);

        AccountLockoutResultDto LockUnlockUserAccount(int targetOrgRegProgUserId, bool isAttemptingLock, AccountLockEvent reason, bool isAuthorizationRequired = false);

        void EnableDisableUserAccount(int orgRegProgramUserId, bool isAttemptingDisable, bool isAuthorizationRequired = false);

        void SetHashedPassword(int userProfileId, string passwordHash);

        bool RemoveUser(int orgRegProgUserId, bool isAuthorizationRequired = false);

        /// <summary>
        /// Method to be called by application layer to execute business logic.
        /// </summary>
        /// <param name="dto">Profile DTO</param>
        void UpdateProfile(UserDto dto);
        
        RegistrationResult ValidateUserProfileData(UserDto userProfile);

        ICollection<RegistrationResult> KbqValidation(IEnumerable<AnswerDto> kbqQuestions);
        ICollection<RegistrationResult> SecurityValidation(IEnumerable<AnswerDto> securityQuestions); 
        
        RegistrationResult ValidateRegistrationUserData(UserDto userProfile, IEnumerable<AnswerDto> securityQuestions, IEnumerable<AnswerDto> kbqQuestions);

        bool UpdateEmail(int userProfileId, string newEmailAddress);

        OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUser(int orgRegProgUserId, bool isAuthorizationRequired = false);

        void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int userProfileId, int organizationRegulatoryProgramId, bool isApproved);

        void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int orgRegProgUserId, bool isApproved, bool isSigantory);

        void UpdateOrganizationRegulatoryProgramUserRole(int orgRegProgUserId, int permissionGroupId);

        /// <summary>
        /// Service method to be called from controller
        /// - Updates the Approval flag (transaction)
        /// - Saves the Role (transaction)
        /// - Send emails out notifying user of approval/denial.
        /// </summary>
        /// <param name="orgRegProgUserId"></param>
        /// <param name="permissionGroupId"></param>
        /// <param name="isApproved"></param>
        RegistrationResultDto ApprovePendingRegistration(int orgRegProgUserId, int permissionGroupId, bool isApproved, bool isAuthorizationRequired = false, bool isSignatory = false);

        ICollection<UserDto> GetOrgRegProgSignators(int orgRegProgId);
        ICollection<UserDto> GetAuthorityAdministratorAndStandardUsers(int authorityOrganizationId);
    }
}
