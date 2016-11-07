using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.User
{

    public interface IUserService
    {
        UserDto GetUserProfileById(int userProfileId);

        UserDto GetUserProfileByEmail(string emailAddress);

        ICollection<OrganizationRegulatoryProgramUserDto> GetProgramUsersByEmail(string emailAddress);

        List<OrganizationRegulatoryProgramUserDto> GetUserProfilesForOrgRegProgram(int orgRegProgramId,
                             bool? isRegApproved,
                             bool? isRegDenied,
                             bool? isEnabled,
                             bool? isRemoved);

        int AddNewUser(int orgRegProgId, int permissionGroupId, string emailAddress, string firstName, string lastName);

        void UpdateUserState(int orgRegProgUserId, bool? isRegApproved, bool? isRegDenied, bool? isEnabled, bool? isRemoved);

        void UpdateUserPermissionGroupId(int orgRegProgUserId, int permissionGroupId);

        void UpdateUserSignatoryStatus(int orgRegProgUserId, bool isSignatory);

        ResetUserResultDto ResetUser(int userProfileId, string newEmailAddress);

        AccountLockoutResultDto LockUnlockUserAccount(int userProfileId, bool isAttemptingLock);

        bool EnableDisableUserAccount(int orgRegProgramUserId, bool isAttemptingDisable);

        void SetHashedPassword(int userProfileId, string passwordHash);

        bool RemoveUser(int orgRegProgUserId);

        /// <summary>
        /// Method to be called by application layer to execute business logic.
        /// </summary>
        /// <param name="dto">Profile DTO</param>
        void UpdateProfile(UserDto dto);

        bool UpdateEmail(int userProfileId, string newEmailAddress);

        OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUser(int orgRegProgUserId);

        void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int userProfileId, int organizationRegulatoryProgramId, bool isApproved);

        void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int orgRegProgUserId, bool isApproved);

    }
}
