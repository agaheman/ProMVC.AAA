using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace ProMVC.AAA.WebApplication.Models
{
    public interface IUsedPasswordsService
    {
        Task AddToUsedPasswordsListAsync(AppUser user);
        Task<bool> IsPreviouslyUsedPasswordAsync(AppUser user, string newPassword);
    }

    public class UsedPasswordsService : IUsedPasswordsService
    {
        private readonly int _notAllowedPreviouslyUsedPasswords;
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly IdentityDbContext<AppUser> _dbContext;
        private readonly DbSet<UserUsedPassword> _userUsedPasswords;
        public UsedPasswordsService(
           IdentityDbContext<AppUser> identityDbContext,
           IPasswordHasher<AppUser> passwordHasher,
           IOptionsSnapshot<SiteSettings> configuration)
        {
            _dbContext = identityDbContext;

            _userUsedPasswords = _dbContext.Set<UserUsedPassword>();

            _passwordHasher = passwordHasher;

            _notAllowedPreviouslyUsedPasswords = configuration.Value.NotAllowedPreviouslyUsedPasswords;
        }
        public async Task AddToUsedPasswordsListAsync(AppUser user)
        {
            await _userUsedPasswords.AddAsync(new UserUsedPassword
            {
                UserId = user.Id,
                HashedPassword = user.PasswordHash
            });
            await _dbContext.SaveChangesAsync();
        }


        public Task<bool> IsPreviouslyUsedPasswordAsync(AppUser user, string newPassword)
        {
            var userId = user.Id;
            return
                _userUsedPasswords
                    .AsNoTracking()
                    .Where(userUsedPassword => userUsedPassword.UserId == userId)
                    .OrderByDescending(userUsedPassword => userUsedPassword.Id)
                    .Select(userUsedPassword => userUsedPassword.HashedPassword)
                    .Take(_notAllowedPreviouslyUsedPasswords)
                    .AnyAsync(hashedPassword => _passwordHasher.VerifyHashedPassword(user, hashedPassword, newPassword) != PasswordVerificationResult.Failed);
        }


    }
}
