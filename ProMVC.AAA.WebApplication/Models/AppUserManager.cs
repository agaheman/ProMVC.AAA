using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProMVC.AAA.WebApplication.Models
{
    public interface IAppUserManager : IDisposable
    {
        Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword);
        Task<IdentityResult> CreateAsync(AppUser user);
        Task<IdentityResult> CreateAsync(AppUser user, string password);
        Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword);
    }

    public class AppUserManager : UserManager<AppUser>, IAppUserManager
    {
        private readonly IUsedPasswordsService usedPasswordsService;
        private readonly IUserStore<AppUser> store;
        private readonly IOptions<IdentityOptions> optionsAccessor;
        private readonly IPasswordHasher<AppUser> passwordHasher;
        private readonly IEnumerable<IUserValidator<AppUser>> userValidators;
        private readonly IEnumerable<IPasswordValidator<AppUser>> passwordValidators;
        private readonly ILookupNormalizer keyNormalizer;
        private readonly IdentityErrorDescriber errors;
        private readonly IServiceProvider services;
        private readonly ILogger<UserManager<AppUser>> logger;

        public AppUserManager(
                              IUsedPasswordsService usedPasswordsService,
                              IUserStore<AppUser> store,
                              IOptions<IdentityOptions> optionsAccessor,
                              IPasswordHasher<AppUser> passwordHasher,
                              IEnumerable<IUserValidator<AppUser>> userValidators,
                              IEnumerable<IPasswordValidator<AppUser>> passwordValidators,
                              ILookupNormalizer keyNormalizer,
                              IdentityErrorDescriber errors,
                              IServiceProvider services,
                              ILogger<UserManager<AppUser>> logger) :

            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            this.usedPasswordsService = usedPasswordsService;
            this.store = store;
            this.optionsAccessor = optionsAccessor;
            this.passwordHasher = passwordHasher;
            this.userValidators = userValidators;
            this.passwordValidators = passwordValidators;
            this.keyNormalizer = keyNormalizer;
            this.errors = errors;
            this.services = services;
            this.logger = logger;
        }

        public override async Task<IdentityResult> CreateAsync(AppUser user)
        {
            var result = await base.CreateAsync(user);
            if (result.Succeeded)
            {
                await usedPasswordsService.AddToUsedPasswordsListAsync(user);
            }
            return result;
        }

        public override async Task<IdentityResult> CreateAsync(AppUser user, string password)
        {
            var result = await base.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await usedPasswordsService.AddToUsedPasswordsListAsync(user);
            }
            return result;
        }

        public override async Task<IdentityResult> UpdateAsync(AppUser user)
        {
            var oldUser = await base.FindByIdAsync(user.Id);
            var result = await base.UpdateAsync(user);

            if (result.Succeeded)
            {
                if (oldUser.PasswordHash != user.PasswordHash)
                {
                    await usedPasswordsService.AddToUsedPasswordsListAsync(user);
                }
            }

            return result;
        }

        public override async Task<IdentityResult> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
        {

            var result = await base.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                await usedPasswordsService.AddToUsedPasswordsListAsync(user);
            }
            return result;
        }

        public override async Task<IdentityResult> ResetPasswordAsync(AppUser user, string token, string newPassword)
        {
            var result = await base.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                await usedPasswordsService.AddToUsedPasswordsListAsync(user);
            }
            return result;
        }
    }
}
