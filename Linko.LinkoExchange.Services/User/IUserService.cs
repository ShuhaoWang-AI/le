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

        List<UserDto> GetUserProfilesForOrgRegProgram(int orgRegProgramId,
                             bool? isRegApproved,
                             bool? isRegDenied,
                             bool? isEnabled,
                             bool? isRemoved);

        int AddNewUser(int orgRegProgId, int permissionGroupId, string emailAddress, string firstName, string lastName);

        void UpdateUserState(int orgRegProgUserId, bool? isRegApproved, bool? isRegDenied, bool? isEnabled, bool? isRemoved);

        void UpdateUserPermissionGroupId(int orgRegProgUserId, int permissionGroupId);

        void UpdateUserSignatoryStatus(int orgRegProgUserId, bool isSignatory);

        void RequestSignatoryStatus(int orgRegProgUserId);

        void ResetUser(int userProfileId, string newEmailAddress);

        AccountLockoutResultDto LockUnlockUserAccount(int userProfileId, bool isAttemptingLock);

        bool EnableDisableUserAccount(int orgRegProgramUserId, bool isAttemptingDisable);

        void SetHashedPassword(int userProfileId, string passwordHash);

        void RemoveUser(int orgRegProgUserId);

        void UpdateUser(UserDto request);

        void ChangePassword(int userProfileId, string oldPassword, string newPassword);

        OrganizationRegulatoryProgramUserDto GetOrganizationRegulatoryProgramUser(int userProfileId);

        void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int userProfileId, int organizationRegulatoryProgramId, bool isApproved);

        void UpdateOrganizationRegulatoryProgramUserApprovedStatus(int orgRegProgUserId, bool isApproved);
    }
}
