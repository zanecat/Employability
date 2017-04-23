using System.Collections.Generic;
using System.Threading.Tasks;
using EmployabilityWebApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace EmployabilityWebApp.Services
{
    /// <summary>
    /// An interface defines methods that could be used by presentation layer.
    /// </summary>
    public interface IAccountService
    {

        /// <summary>
        ///  Use password to sign in
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="password">password</param>
        /// <param name="isPersistent">is Persistent or not</param>
        /// <param name="shouldLockout">lock out or not</param>
        /// <returns>Sign In status</returns>
        Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout);

        /// <summary>
        ///  Create new user with password.
        /// </summary>
        /// <param name="user">a new user</param>
        /// <param name="password">password</param>
        /// <returns>Identity result</returns>
        Task<IdentityResult> CreateAsync(ApplicationUser user, string password);

        /// <summary>
        /// Create a new user.
        /// </summary>
        /// <param name="user">a new users</param>
        /// <returns>Identity result</returns>
        Task<IdentityResult> CreateAsync(ApplicationUser user);

        /// <summary>
        /// check token against a user with userId.
        /// </summary>
        /// <param name="userId">the id of a user</param>
        /// <param name="token">token</param>
        /// <returns>Identity result</returns>
        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);

        /// <summary>
        /// Check if email is confirmed or not
        /// </summary>
        /// <param name="email">the email to be checked</param>
        /// <returns>true if the email has been confirmed, otherwise false</returns>
        Task<bool> IsEmailConfirmedAsync(string email);

        /// <summary>
        /// Reset password.
        /// </summary>
        /// <param name="userId">id of a user</param>
        /// <param name="token">token</param>
        /// <param name="newPassword">new password</param>
        /// <returns>Identity result</returns>
        Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword);

        /// <summary>
        /// External sign in.
        /// </summary>
        /// <param name="loginInfo">info about log in</param>
        /// <param name="isPersistent">is persistent or not</param>
        /// <returns>Sign in status</returns>
        Task<SignInStatus> ExternalSignInAsync(ExternalLoginInfo loginInfo, bool isPersistent);

        /// <summary>
        /// Send two factor code.
        /// </summary>
        /// <param name="provider">provider of two factor service</param>
        /// <returns>true if success, otherwise false</returns>
        Task<bool> SendTwoFactorCodeAsync(string provider);

        /// <summary>
        /// Generate token by token provider.
        /// </summary>
        /// <param name="userId">id of a user</param>
        /// <returns>a new token</returns>
        Task<string> GenerateEmailConfirmationTokenAsync(string userId);

        /// <summary>
        /// Send email to a user.
        /// </summary>
        /// <param name="userId">id of a user</param>
        /// <param name="subject">subject of the email</param>
        /// <param name="body">body of the email</param>
        /// <returns>void</returns>
        Task SendEmailAsync(string userId, string subject, string body);

        /// <summary>
        /// Confirm user id and token
        /// </summary>
        /// <param name="userId">id of a user</param>
        /// <param name="token">confirmation token</param>
        /// <returns>void</returns>
        Task ConfirmEmailBasicUser(string userId, string token);

        /// <summary>
        /// Confirm organisation user id and token
        /// </summary>
        /// <param name="userId">id of a user</param>
        /// <param name="token">token</param>
        /// <returns>void</returns>
        Task ConfirmEmailOrganisationUser(string userId, string token);

        /// <summary>
        /// check if has been verified or not
        /// </summary>
        /// <returns>true if verified.</returns>
        Task<bool> HasBeenVerifiedAsync();

        /// <summary>
        /// Using two factors sign in.
        /// </summary>
        /// <param name="provider">two factors provider</param>
        /// <param name="code">security code</param>
        /// <param name="isPersistent">is persistent or not</param>
        /// <param name="rememberBrowser">remember or not</param>
        /// <returns>Sign in status</returns>
        Task<SignInStatus> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberBrowser);

        /// <summary>
        /// Find a user by email.
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>return the found user</returns>
        Task<ApplicationUser> FindByEmailAsync(string email);

        /// <summary>
        /// Add login
        /// </summary>
        /// <param name="userId">id of a user</param>
        /// <param name="login">login info</param>
        /// <returns>Identity result</returns>
        Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo login);

        /// <summary>
        /// Sign in with some specific requirements.
        /// </summary>
        /// <param name="user">id of a user</param>
        /// <param name="isPersistent">persistent or not</param>
        /// <param name="rememberBrowser">remember or not</param>
        /// <returns>void</returns>
        Task SignInAsync(ApplicationUser user, bool isPersistent, bool rememberBrowser);

        /// <summary>
        /// Get verified user id.
        /// </summary>
        /// <returns>user id</returns>
        Task<string> GetVerifiedUserIdAsync();

        /// <summary>
        ///  Get Valid providers for two factor sign in.
        /// </summary>
        /// <param name="userId">id of a user</param>
        /// <returns>providers</returns>
        Task<IList<string>>  GetValidTwoFactorProvidersAsync(string userId);
    }

}