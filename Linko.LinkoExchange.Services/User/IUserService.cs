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

        void UpdateUserPermissionGroupId(int orgRegProgUserId, int permissionGroupId);

        void UpdateUserSignatoryStatus(int orgRegProgUserId, bool isSignatory);

        ResetUserResultDto ResetUser(int userProfileId, string newEmailAddress, int? targetOrgRegProgramId = null);

        AccountLockoutResultDto LockUnlockUserAccount(int userProfileId, bool isAttemptingLock, AccountLockEvent reason);

        void EnableDisableUserAccount(int orgRegProgramUserId, bool isAttemptingDisable);

        void SetHashedPassword(int userProfileId, string passwordHash);

        bool RemoveUser(int orgRegProgUserId);

        /// <summary>
        /// Method to be called by application layer to execute business logic.
        /// </summary>
        /// <param name="dto">Profile DTO</param>
        void UpdateProfile(UserDto dto);

        RegistrationResult UpdateProfile(UserDto dto, IEnumerable<AnswerDto> securityQuestions, IEnumerable<AnswerDto> kbqQuestions);

        RegistrationResult ValidateUserProfileData(UserDto userProfile);

        RegistrationResult ValidateRegistrationUserData(UserDto userProfile, IEnumerable<AnswerDto> securityQuestions, IEnumerable<AnswerDto> kbqQuestions);

        bool UpdateEmail(int userProfileId, string newEmailAddress);

        OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUser(int orgRegProgUserId);

        void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int userProfileId, int organizationRegulatoryProgramId, bool isApproved);

        void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int orgRegProgUserId, bool isApproved);

        void UpdateOrganizationRegulatoryProgramUserRole(int orgRegProgUserId, int permissionGroupId);

        /// <summary>
        /// Service method to be called from controller
        /// - Updates the Approval flag (transaction)
        /// - Saves the Role (transaction)
        /// - Send emails out notfying user of approval/denial.
        /// </summary>
        /// <param name="orgRegProgUserId"></param>
        /// <param name="permissionGroupId"></param>
        /// <param name="isApproved"></param>
        RegistrationResultDto ApprovePendingRegistration(int orgRegProgUserId, int permissionGroupId, bool isApproved);

        ICollection<UserDto> GetOrgRegProgSignators(int orgRegProgId);
        ICollection<UserDto> GetAuthorityAdministratorAndStandardUsers(int authorityOrganizationId);
    }
}
