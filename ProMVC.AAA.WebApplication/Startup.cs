using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProMVC.AAA.WebApplication.Identity;
using ProMVC.AAA.WebApplication.Models;
using System;

namespace ProMVC.AAA.WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<SiteSettings>(options => Configuration.Bind(options));

            services.AddTransient<IPasswordValidator<AppUser>, CheckLastNPasswordsValidator>();

            services.AddDbContext<AppIdentityDbContext>(d => d.UseSqlServer(Configuration.GetConnectionString("AppIdentityConnection")));

            //services.AddDefaultIdentity<AppUser>()
            //    .AddDefaultUI(UIFramework.Bootstrap4)
            //    .AddEntityFrameworkStores<AppIdentityDbContext>();


            //services.BuildServiceProvider().GetService<IOptionsSnapshot<SiteSettings>>().Value

            services.AddIdentity<AppUser, IdentityRole>()
                .AddUserManager<AppUserManager>()
                .AddDefaultTokenProviders()
                .AddEntityFrameworkStores<AppIdentityDbContext>();

            services.AddScoped<IUsedPasswordsService, UsedPasswordsService>();
            services.AddScoped<IAppUserManager, AppUserManager>();

            services.AddScoped<IdentityDbContext<AppUser>, AppIdentityDbContext>();
            services.AddScoped<UserManager<AppUser>, AppUserManager>();

            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.SlidingExpiration = true;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDatabaseErrorPage();
            app.UseStaticFiles();
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();

        }
    }
}
