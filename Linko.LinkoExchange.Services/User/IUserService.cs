using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;
using System.Collections.Generic;

namespace Linko.LinkoExchange.Services.User
{

    public interface IUserService
    {
        /// <summary>
        /// Returns a user profile for a valid id.
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <returns></returns>
        UserDto GetUserProfileById(int userProfileId);

        /// <summary>
        /// Returns a user profile for a valid email address.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        UserDto GetUserProfileByEmail(string emailAddress);

        /// <summary>
        /// Returns a collection of org reg program users that share the same user profile that is associated with a given email address.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        ICollection<OrganizationRegulatoryProgramUserDto> GetProgramUsersByEmail(string emailAddress);

        /// <summary>
        /// Returns the number of valid users that are waiting for their registration to get approved/denied
        /// for a given org reg program.
        /// </summary>
        /// <param name="orgRegProgamId"></param>
        /// <returns></returns>
        int GetPendingRegistrationProgramUsersCount(int orgRegProgamId);


        /// <summary>
        /// Returns the valid org reg program user that are waiting for their registration to get approved/denied
        /// for a given org reg program.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <returns></returns>
        List<OrganizationRegulatoryProgramUserDto> GetPendingRegistrationProgramUsers(int orgRegProgramId);

        /// <summary>
        /// Returns the org reg program users that belong to a given org reg program
        /// optionally filtered by the various state-related flags.
        /// </summary>
        /// <param name="orgRegProgramId"></param>
        /// <param name="isRegApproved"></param>
        /// <param name="isRegDenied"></param>
        /// <param name="isEnabled"></param>
        /// <param name="isRemoved"></param>
        /// <returns></returns>
        List<OrganizationRegulatoryProgramUserDto> GetUserProfilesForOrgRegProgram(int orgRegProgramId,
                             bool? isRegApproved,
                             bool? isRegDenied,
                             bool? isEnabled,
                             bool? isRemoved);

        /// <summary>
        /// Used to update one or more of the various state-related flags for an org reg program user.
        /// </summary>
        /// <param name="orgRegProgUserId"></param>
        /// <param name="isRegApproved"></param>
        /// <param name="isRegDenied"></param>
        /// <param name="isEnabled"></param>
        /// <param name="isRemoved"></param>
        void UpdateUserState(int orgRegProgUserId, bool? isRegApproved, bool? isRegDenied, bool? isEnabled, bool? isRemoved);

        /// <summary>
        /// Updates an org reg program user's permissions group.
        /// The current accessing user can be optionally authorized to perform this action.
        /// </summary>
        /// <param name="orgRegProgUserId"></param>
        /// <param name="permissionGroupId"></param>
        /// <param name="isAuthorizationRequired"></param>
        void UpdateUserPermissionGroupId(int orgRegProgUserId, int permissionGroupId, bool isAuthorizationRequired = false);

        /// <summary>
        /// Updates an org reg program user's signatory rights. Emails are then sent to appropriate stakeholders informing them
        /// of the action.
        /// The current accessing user can be optionally authorized to perform this action. 
        /// </summary>
        /// <param name="orgRegProgUserId"></param>
        /// <param name="isSignatory"></param>
        /// <param name="isAuthorizationRequired"></param>
        void UpdateUserSignatoryStatus(int orgRegProgUserId, bool isSignatory, bool isAuthorizationRequired = false);

        /// <summary>
        /// Resets an org reg program user. Emails are then sent to the appropriate stakeholders including the user's email address
        /// and potentially a different unassociated email address.
        /// The current accessing user can be optionally authorized to perform this action. 
        /// </summary>
        /// <param name="targetOrgRegProgUserId"></param>
        /// <param name="newEmailAddress"></param>
        /// <param name="isAuthorizationRequired"></param>
        /// <returns></returns>
        ResetUserResultDto ResetUser(int targetOrgRegProgUserId, string newEmailAddress, bool isAuthorizationRequired = false);

        /// <summary>
        /// Locks (or unlocks) a user profile across all related org reg programs. 
        /// Emails are then sent to the appropriate stakeholders and this action is logged for Cromerr purposes.
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <param name="isAttemptingLock"></param>
        /// <param name="reason"></param>
        /// <param name="reportPackageId"></param>
        /// <returns></returns>
        AccountLockoutResultDto LockUnlockUserAccount(int userProfileId, bool isAttemptingLock, AccountLockEvent reason, int? reportPackageId = null);

        /// <summary>
        /// Locks (or unlocks) an org reg program user. Emails are then sent to the appropriate stakeholders.
        /// The current accessing user can be optionally authorized to perform this action.
        /// </summary>
        /// <param name="targetOrgRegProgUserId"></param>
        /// <param name="isAttemptingLock"></param>
        /// <param name="reason"></param>
        /// <param name="isAuthorizationRequired"></param>
        /// <returns></returns>
        AccountLockoutResultDto LockUnlockUserAccount(int targetOrgRegProgUserId, bool isAttemptingLock, AccountLockEvent reason, bool isAuthorizationRequired = false);

        /// <summary>
        /// Enables (or disables) an org reg program user. Emails are then sent to the appropriate stakeholders.
        /// The current accessing user can be optionally authorized to perform this action.
        /// </summary>
        /// <param name="orgRegProgramUserId"></param>
        /// <param name="isAttemptingDisable"></param>
        /// <param name="isAuthorizationRequired"></param>
        void EnableDisableUserAccount(int orgRegProgramUserId, bool isAttemptingDisable, bool isAuthorizationRequired = false);

        /// <summary>
        /// Updates the hashed password property of given user profile.
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <param name="passwordHash"></param>
        void SetHashedPassword(int userProfileId, string passwordHash);

        /// <summary>
        /// User is soft-deleted from the database. The current accessing user is optionally checked for authorization to perform this action.
        /// Cromerr logging occurs as well.
        /// </summary>
        /// <param name="orgRegProgUserId"></param>
        /// <param name="isAuthorizationRequired"></param>
        /// <returns></returns>
        bool RemoveUser(int orgRegProgUserId, bool isAuthorizationRequired = false);

        /// <summary>
        /// Updates the various properties of a user profile.
        /// </summary>
        /// <param name="dto"></param>
        void UpdateProfile(UserDto dto);

        /// <summary>
        /// Checks that required fields contain text
        /// </summary>
        /// <param name="userProfile"></param>
        /// <returns></returns>
        RegistrationResult ValidateUserProfileData(UserDto userProfile);

        /// <summary>
        /// Validates knowledge-based questions and answers
        /// </summary>
        /// <param name="kbqQuestions"></param>
        /// <returns></returns>
        ICollection<RegistrationResult> KbqValidation(IEnumerable<AnswerDto> kbqQuestions);

        /// <summary>
        /// Validates security questions and answers
        /// </summary>
        /// <param name="securityQuestions"></param>
        /// <returns></returns>
        ICollection<RegistrationResult> SecurityValidation(IEnumerable<AnswerDto> securityQuestions);

        /// <summary>
        /// - Check if required fields contain text
        /// - Check if minimum number of SQ/KBQ questions are present
        /// - Check for duplicated questions and/or answers
        /// - Check if answers are provided
        /// </summary>
        /// <param name="userProfile"></param>
        /// <param name="securityQuestions"></param>
        /// <param name="kbqQuestions"></param>
        /// <returns></returns>
        RegistrationResult ValidateRegistrationUserData(UserDto userProfile, IEnumerable<AnswerDto> securityQuestions, IEnumerable<AnswerDto> kbqQuestions);

        /// <summary>
        /// Checks if new email address is already in use and returns false if so. If the email address is unused, the user profile
        /// is updated with it. An email communication is sent out to the old and new email address as well as logging occurs.
        /// </summary>
        /// <param name="userProfileId"></param>
        /// <param name="newEmailAddress"></param>
        /// <returns></returns>
        bool UpdateEmail(int userProfileId, string newEmailAddress);

        /// <summary>
        /// Returns an org reg program user. The current accessing user can also optionally checked for authorization to query the target user.
        /// </summary>
        /// <param name="orgRegProgUserId"></param>
        /// <param name="isAuthorizationRequired"></param>
        /// <returns></returns>
        OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUser(int orgRegProgUserId, bool isAuthorizationRequired = false);

        /// <summary>
        /// Updates an org reg program user's approved status and signatory permissions if there is a change. If there is a change in signatory
        /// permissions, various email communications are sent out and logging occurs.
        /// </summary>
        /// <param name="orgRegProgUserId"></param>
        /// <param name="isApproved"></param>
        /// <param name="isSigantory"></param>
        void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int orgRegProgUserId, bool isApproved, bool isSigantory);

        /// <summary>
        /// Updates the PermissionGroupId field of the OrganizationRegulatoryProgramUser
        ///     (* No validation performed)
        /// </summary>
        /// <param name="orgRegProgUserId"></param>
        /// <param name="permissionGroupId"></param>
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

        /// <summary>
        /// Returns a collection of active users with signatory permission belonging to a given org reg program.
        /// </summary>
        /// <param name="orgRegProgId"></param>
        /// <returns></returns>
        ICollection<UserDto> GetOrgRegProgSignators(int orgRegProgId);

        /// <summary>
        /// Returns all active admin and standard users for the authority
        /// </summary>
        /// <param name="authorityOrganizationId"></param>
        /// <returns></returns>
        ICollection<UserDto> GetAuthorityAdministratorAndStandardUsers(int authorityOrganizationId);
    }
}
