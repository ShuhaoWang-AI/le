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

        void RemoveUser(int orgRegProgUserId);

        void UpdateUser(UserDto request);

        void ChangePassword(int userProfileId, string oldPassword, string newPassword);


    }
}
