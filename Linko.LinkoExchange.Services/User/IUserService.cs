using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.User
{
    public class UserProfileDTO
    {
        public int UserProfileId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string TitleRole { get; set; }
        public string BusinessName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public string ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneExt { get; set; }
    }

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
        UserProfileDTO GetUserProfileById(int userProfileId);

        UserProfileDTO GetUserProfileByEmail(string emailAddress);

        List<UserProfileDTO> GetUserProfilesForOrgRegProgram(int organizationRegulatoryProgramId,
                             bool? isRegApproved,
                             bool? isRegDenied,
                             bool? isEnabled,
                             bool? isRemoved);

        int AddNewUser(string emailAddress, string firstName, string lastName);

        void UpdateUserProfileState(int userProfileId, bool? isRegApproved, bool? isRegDenied, bool? isEnabled, bool? isRemoved);

        void UpdateUserPermissionGroupId(int userProfileId, int permissionGroupId);

        void UpdateUserSignatoryStatus(int orgRegProgUserId, bool isSignatory);

        void RequestSignatoryStatus(int userProfileId);

        void ResetUser(int userProfileId, string newEmailAddress);

        void RemoveUser(int userProfileId);

        void UpdateUserProfile(UserProfileDTO request);

        void ChangePassword(int userProfileId, string oldPassword, string newPassword);

        void UpdateQuestionAnswerPairs(UserQuestionAnswerPairsDTO questionAnswerPairs);

    }
}
