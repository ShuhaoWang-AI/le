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
        Task<AuthenticationResultDto> ChangePasswordAsync(string userId, string newPassword);
        Task<AuthenticationResultDto> ResetPasswordAsync(string userId, string changePasswordToken, string newPassword);

        // Sign in  
        Task<SignInResultDto> SignInByUserName(string userName, string password, bool isPersistent);

        IEnumerable<Claim> GetClaims();
        //sign off
        void SignOff();
    }
}
