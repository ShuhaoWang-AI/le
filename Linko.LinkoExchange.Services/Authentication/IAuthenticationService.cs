﻿using System.Collections.Generic;
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
        Task<RegistrationResultDto> Register(UserDto userInfo, string registrationToken); 

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
        Task<AuthenticationResultDto> ResetPasswordAsync(string token, int userQuestionAnswerId, string answer, int attempCount, string password);

        /// <summary>
        /// To request a password reset. This will do follow:
        /// 1. generate a reset password token
        /// 2. send a reset password email
        /// 3. log to system 
        /// </summary>
        /// <param name="username">UserName entered by user</param>
        /// <returns></returns>
        Task<AuthenticationResultDto> RequestResetPassword(string username);

        Task<AuthenticationResultDto> RequestUsernameEmail(string email);

        // Sign in  
        Task<SignInResultDto> SignInByUserName(string userName, string password, bool isPersistent);

        /// <summary>
        /// Set current user's additional claims, such as current organizationId, current authorityId, current programId
        /// </summary>
        /// <param name="claims">The claims to set</param>
        void SetCurrentUserClaims(IDictionary<string,string> claims);
        IList<Claim> GetClaims();
        //sign off
        void SignOff();

        void SetClaimsForOrgRegProgramSelection(int orgRegProgId);

        string GetClaimsValue(string claimType);
    }
}
