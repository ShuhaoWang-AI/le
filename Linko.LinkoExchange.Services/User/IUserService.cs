using Linko.LinkoExchange.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.User
{

    public class QuestionAnswerPair
    {
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public string AnswerId { get; set; }
        public string Answer { get; set; }
    }

    public class UserQuestionAnswerPairsDTO
    {
        public List<QuestionAnswerPair> QuestionAnswerPairs { get; set; }
    }

    public interface IUserService
    {
        UserDto GetUserProfileById(int userProfileId);

        UserDto GetUserProfileByEmail(string emailAddress);

        List<UserDto> GetUserProfilesForOrgRegProgram(int organizationRegulatoryProgramId,
                             bool? isRegApproved,
                             bool? isRegDenied,
                             bool? isEnabled,
                             bool? isRemoved);

        int AddNewUser(string emailAddress, string firstName, string lastName);

        void UpdateUserProfileState(int userProfileId, bool? isRegApproved, bool? isRegDenied, bool? isEnabled, bool? isRemoved);

        void UpdateUserPermissionGroupId(int userProfileId, int permissionGroupId);

        void UpdateUserSignatoryStatus(int orgRegProgUserId, bool isSignatory);

        void RequestSignatoryStatus(int orgRegProgUserId);

        void ResetUser(int userProfileId, string newEmailAddress);

        void RemoveUser(int userProfileId);

        void UpdateUser(UserDto request);

        void ChangePassword(int userProfileId, string oldPassword, string newPassword);

        void UpdateQuestionAnswerPairs(UserQuestionAnswerPairsDTO questionAnswerPairs);

    }
}
