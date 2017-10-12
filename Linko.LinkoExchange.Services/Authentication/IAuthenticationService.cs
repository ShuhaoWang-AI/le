using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Linko.LinkoExchange.Core.Enum;
using Linko.LinkoExchange.Services.Dto;

namespace Linko.LinkoExchange.Services.Authentication
{
    public interface IAuthenticationService
    {
        ICollection<RegistrationResult> ValidateUserProfileForRegistration(UserDto userInfo, RegistrationType registrationType);

        /// <summary>
        ///     Validate KBQ answer and password
        /// </summary>
        /// <param name="password"> The password user input </param>
        /// <param name="userQuestionAnswerId"> The KBQ question Id </param>
        /// <param name="kbqAnswer"> The KBQ question answer </param>
        /// <param name="failedPasswordCount"> Failed password count </param>
        /// <param name="failedKbqCount"> Failed KBQ answer count </param>
        /// <param name="reportOperation"> Sign & Submit or Repudiate </param>
        /// <param name="reportPackageId"> </param>
        /// <returns> </returns>
        PasswordAndKbqValidationResultDto ValidatePasswordAndKbq(string password, int userQuestionAnswerId, string kbqAnswer, int failedPasswordCount, int failedKbqCount,
                                                              ReportOperation reportOperation, int? reportPackageId = null);

        /// <summary>
        ///     Register a new user
        /// </summary>
        /// <param name="userInfo"> The user information object </param>
        /// <param name="registrationToken"> The registration token string </param>
        /// <param name="securityQuestions"> Security questions and answers </param>
        /// <param name="kbqQuestions"> KBQ question and answers </param>
        /// <param name="registrationType"> Registration type </param>
        /// <returns> </returns>
        RegistrationResultDto Register(UserDto userInfo, string registrationToken, IEnumerable<AnswerDto> securityQuestions, IEnumerable<AnswerDto> kbqQuestions,
                                       RegistrationType registrationType);

        // Change or reset password
        /// <summary>
        ///     Change password happens after a user login, and change his password
        /// </summary>
        /// <param name="userId"> User Id </param>
        /// <param name="newPassword"> The new password </param>
        /// <returns> </returns>
        Task<AuthenticationResultDto> ChangePasswordAsync(string userId, string newPassword);

        /// <summary>
        ///     Reset password happens when user request a 'reset password', and system generates a reset password token and sends to user's email
        ///     And user click the link in the email to reset the password.
        /// </summary>
        /// <param name="token"> The reset password token </param>
        /// <param name="userQuestionAnswerId"> The user question answer identifier. </param>
        /// <param name="answer"> The answer. </param>
        /// <param name="attemptCount"> The attempt count. </param>
        /// <param name="password"> The new password </param>
        /// <returns> </returns>
        Task<AuthenticationResultDto> ResetPasswordAsync(string token, int userQuestionAnswerId, string answer, int attemptCount, string password);

        Task<AuthenticationResultDto> ResetPasswordAsync(int userQuestionAnswerId, string answer, int attempCount, string password);

        bool CheckPasswordResetUrlNotExpired(string token);

        /// <summary>
        ///     To request a password reset. This will do follow:
        ///     1. generate a reset password token
        ///     2. send a reset password email
        ///     3. log to system
        /// </summary>
        /// <param name="username"> UserName entered by user </param>
        /// <returns> </returns>
        Task<AuthenticationResultDto> RequestResetPassword(string username);

        Task<AuthenticationResultDto> RequestUsernameEmail(string email);

        // Sign in  
        Task<SignInResultDto> SignInByUserName(string userName, string password, bool isPersistent);

        /// <summary>
        ///     Set current user's additional claims, such as current organizationId, current authorityId, current programId
        /// </summary>
        /// <param name="claims"> The claims to set </param>
        void SetCurrentUserClaims(IDictionary<string, string> claims);

        void UpdateClaim(string key, string value);

        IList<Claim> GetClaims();

        //sign off
        void SignOff();

        void SetClaimsForOrgRegProgramSelection(int orgRegProgId);

        string GetClaimsValue(string claimType);
    }
}