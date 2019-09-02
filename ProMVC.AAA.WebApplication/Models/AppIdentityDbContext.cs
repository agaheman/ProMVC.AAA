using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ProMVC.AAA.WebApplication.Models
{
    public class AppIdentityDbContext : IdentityDbContext<AppUser>
    {
        public AppIdentityDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<UserUsedPassword> UserUsedPasswords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserUsedPassword>(modelBuilder =>
            {
                modelBuilder.ToTable("AppUserUsedPasswords");
                modelBuilder.Property(applicationUserUsedPassword => applicationUserUsedPassword.HashedPassword)
                    .HasMaxLength(450)
                    .IsRequired();
                modelBuilder.HasOne(applicationUserUsedPassword => applicationUserUsedPassword.User)
                    .WithMany(applicationUser => applicationUser.UserUsedPasswords);
            });
        }
    }
}
