using System.Collections.Generic;
using System.Security.Claims;
using Linko.LinkoExchange.Services.Dto;
using System.Threading.Tasks;

namespace Linko.LinkoExchange.Services.Authentication
{
    public interface IAuthenticationService
    {
        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="userInfo">The user information</param>
        /// <param name="registrationToken">The registration token</param>
        /// <returns>The authentication result</returns>
        Task<AuthenticationResultDto> CreateUserAsync(UserDto userInfo, string registrationToken); 

        // Change or reset password
        /// <summary>
        /// Change password happens after a user login, and change his password
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="newPassword">The new password</param>
        /// <returns></returns>
        Task<AuthenticationResultDto> ChangePasswordAsync(string userId, string newPassword);

        /// <summary>
        /// Reset password happens when user request a 'reset password', and system generates a reset password token and sends to user's email
        /// And user click the link in the email to reset the password.
        /// </summary>
        /// <param name="email">User email address </param>
        /// <param name="resetPasswordToken">The reset password token</param>
        /// <param name="newPassword">The new password</param>
        /// <returns></returns>
        Task<AuthenticationResultDto> ResetPasswordAsync(string email, string resetPasswordToken, string newPassword);

        /// <summary>
        /// To request a password reset. This will do follow:
        /// 1. generate a reset password token
        /// 2. send a reset password email
        /// 3. log to system 
        /// </summary>
        /// <param name="email">The email address </param>
        /// <returns></returns>
        Task<AuthenticationResultDto> RequestResetPassword(string email); 

            // Sign in  
        Task<SignInResultDto> SignInByUserName(string userName, string password, bool isPersistent);

        IEnumerable<Claim> GetClaims();
        //sign off
        void SignOff();
    }
}
