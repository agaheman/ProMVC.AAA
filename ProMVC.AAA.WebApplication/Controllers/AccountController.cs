using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProMVC.AAA.WebApplication.Models;

namespace ProMVC.AAA.WebApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly IUserValidator<AppUser> userValidator;
        private readonly IPasswordValidator<AppUser> passwordValidator;
        private readonly IPasswordHasher<AppUser> passwordHasher;

        public AccountController(UserManager<AppUser> userManager,SignInManager<AppUser> signInManager,
                                      IUserValidator<AppUser> userValidator,
                                      IPasswordValidator<AppUser> passwordValidator,
                                      IPasswordHasher<AppUser> passwordHasher)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.userValidator = userValidator;
            this.passwordValidator = passwordValidator;
            this.passwordHasher = passwordHasher;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            return View(userManager.Users.ToList());
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create(UserCreateModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };

                //var code = userManager.GenerateEmailConfirmationTokenAsync(user).Result;
                //var callbackUrl = Url.Page(
                //    "/Account/ConfirmEmail",
                //    pageHandler: null,
                //    values: new { userId = user.Id, code = code },
                //    protocol: Request.Scheme);

                //await emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");



                IdentityResult result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        public ViewResult Create() => View();



        [BindProperty]
        public UserSignInModel UserSignInModel { get; set; }
        public string ReturnUrl { get; set; }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> LoginPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (ModelState.IsValid)
            {
                AppUser user = await userManager.FindByEmailAsync(UserSignInModel.Email);
                if (user != null)
                {
                    await signInManager.SignOutAsync();

                    var result = await signInManager.PasswordSignInAsync(
                    UserSignInModel.UserName, UserSignInModel.Password, UserSignInModel.RememberMe, lockoutOnFailure: true);
                    if (result.Succeeded)
                    {
                        bool asd = User.Identity.IsAuthenticated;
                        return Redirect(returnUrl);
                    }

                    if (result.IsLockedOut)
                    {
                        return RedirectToPage("./Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                        return View(UserSignInModel);
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(UserSignInModel.Email),"Invalid user or password");
                }
            }

            return View();
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl=null)
        {
            ReturnUrl = returnUrl;
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Edit(string id)
        {
            AppUser user = await userManager.FindByIdAsync(id);
           
            if (user != null)
            {
                UserEditModel userEditModel = new UserEditModel
                {
                    Id = user.Id,
                    NewEmail = user.Email,

                };

                return View(userEditModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(UserEditModel userEditModel)
        {
            AppUser user = await userManager.FindByIdAsync(userEditModel.Id);
            if (user != null)
            {
                user.Email = userEditModel.NewEmail;
                IdentityResult validEmail = await userValidator.ValidateAsync(userManager, user);
                if (!validEmail.Succeeded)
                {
                    AddErrorsFromResult(validEmail);
                }
                IdentityResult validPass = null;

                if (!string.IsNullOrEmpty(userEditModel.NewPassword))
                {
                    validPass = await passwordValidator.ValidateAsync(userManager, user, userEditModel.NewPassword);
                    if (validPass.Succeeded)
                    {
                        user.PasswordHash = passwordHasher.HashPassword(user,userEditModel.NewPassword);
                    }
                    else
                    {
                        AddErrorsFromResult(validPass);
                    }
                }
                if ((validEmail.Succeeded && validPass == null) || (validEmail.Succeeded && userEditModel.NewPassword != string.Empty && validPass.Succeeded))
                {
                    IdentityResult result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View(userEditModel);
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }


    }
}