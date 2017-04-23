using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using EmployabilityWebApp.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace EmployabilityWebApp.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationSignInManager signInManager;
        private readonly ApplicationUserManager userManager;
        private readonly EmployabilityDbContext dbContext;

        public AccountService(EmployabilityDbContext dbContext, 
            ApplicationUserManager userManager, 
            ApplicationSignInManager signInManager)
        {
            this.dbContext = dbContext;
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<SignInStatus> PasswordSignInAsync(string userName, string password, bool isPersistent, bool shouldLockout)
        {
            return await
                    signInManager.PasswordSignInAsync(userName, password, isPersistent,
                        shouldLockout: false);
        }


        public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
        {
            return await userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            return await userManager.CreateAsync(user);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            return await userManager.GenerateEmailConfirmationTokenAsync(userId);
        }

        public async Task SendEmailAsync(string userId, string subject, string body)
        {
             await userManager.SendEmailAsync(userId, subject, body);
        }


        public async Task ConfirmEmailBasicUser(string userId, string token)
        {
            var result = await userManager.ConfirmEmailAsync(userId, token);

            if (result.Succeeded)
            {
                userManager.AddToRole(userId, BasicUser.Role);
                dbContext.SaveChanges();
                var user = await userManager.FindByIdAsync(userId);
                await signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
        }

        public async Task ConfirmEmailOrganisationUser(string userId, string token)
        {
            var result = await userManager.ConfirmEmailAsync(userId, token);

            if (result.Succeeded)
            {
                userManager.AddToRole(userId, OrganisationUser.Role);

                var organistion = new Organisation();
                var user = await userManager.FindByIdAsync(userId);
                dbContext.OrganisationUsers.Attach((OrganisationUser)user);
                organistion.OrganisationUser = (OrganisationUser)user;
                dbContext.Organisations.Add(organistion);
                dbContext.SaveChanges();

                await signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
        }

        public async Task<bool> HasBeenVerifiedAsync()
        {
            return await signInManager.HasBeenVerifiedAsync();
        }

        public async Task<SignInStatus> TwoFactorSignInAsync(string provider, string code, bool isPersistent, bool rememberBrowser)
        {
            return await signInManager.TwoFactorSignInAsync(provider, code, isPersistent: isPersistent,
                         rememberBrowser: rememberBrowser);
        }

        public async  Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return await userManager.FindByEmailAsync(email);
        }

        public async  Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo login)
        {
            return await userManager.AddLoginAsync(userId, login);
        }

        public async Task SignInAsync(ApplicationUser user, bool isPersistent, bool rememberBrowser)
        {
            await signInManager.SignInAsync(user, isPersistent, rememberBrowser);
        }

        public async Task<string> GetVerifiedUserIdAsync()
        {
            return await signInManager.GetVerifiedUserIdAsync();
        }

        public async Task<IList<string>> GetValidTwoFactorProvidersAsync(string userId)
        {
            return await userManager.GetValidTwoFactorProvidersAsync(userId);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            return await userManager.ConfirmEmailAsync(userId, token);
        }

        public async Task<bool> IsEmailConfirmedAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            return await userManager.IsEmailConfirmedAsync(user.Id);
        }

        public async Task<IdentityResult> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            return await userManager.ResetPasswordAsync(userId, token, newPassword);
        }

        public async Task<SignInStatus> ExternalSignInAsync(ExternalLoginInfo loginInfo, bool isPersistent)
        {
            return await signInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
        }

        public async  Task<bool> SendTwoFactorCodeAsync(string provider)
        {
            return await signInManager.SendTwoFactorCodeAsync(provider);
        }
    }
}