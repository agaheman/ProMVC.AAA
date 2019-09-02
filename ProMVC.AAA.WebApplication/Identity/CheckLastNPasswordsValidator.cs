using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ProMVC.AAA.WebApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProMVC.AAA.WebApplication.Identity
{
    public class CheckLastNPasswordsValidator:PasswordValidator<AppUser>
    {
        private readonly IUsedPasswordsService usedPasswordsService;
        private readonly IOptionsSnapshot<SiteSettings> configuration;
        private readonly HashSet<string> passwordsBanList;

        public CheckLastNPasswordsValidator(IUsedPasswordsService usedPasswordsService, IOptionsSnapshot<SiteSettings> configuration)
        {
            this.usedPasswordsService = usedPasswordsService;
            this.configuration = configuration;
            passwordsBanList = new HashSet<string>(configuration.Value.PasswordsBanList);
        }
        public override async Task<IdentityResult> ValidateAsync(UserManager<AppUser> userManager, AppUser user, string password)
        {
            IdentityResult result = await base.ValidateAsync(userManager, user, password);

            List<IdentityError> errors = result.Succeeded ?
                new List<IdentityError>() : result.Errors.ToList();


            if (await usedPasswordsService.IsPreviouslyUsedPasswordAsync(user, password))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordIsPreviouslyUsed",
                    Description = "این کلمه‌ی عبور پیشتر توسط شما استفاده شده‌است و تکراری می‌باشد."
                });
            }


            if (IsBanPassword(password))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordIsNotSafe",
                    Description = "کلمه‌ی عبور وارد شده به سادگی قابل حدس زدن است."
                });
            }

            return errors.Count == 0 ? IdentityResult.Success
            : IdentityResult.Failed(errors.ToArray());
        }

        private bool IsBanPassword(string data)
        {
            if (passwordsBanList.Contains(data.ToLowerInvariant())) return true;
            else return false;
        }
    }
}
