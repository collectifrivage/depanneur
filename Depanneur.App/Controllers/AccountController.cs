using System;
using Depanneur.App.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Depanneur.App.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly DepanneurContext db;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IConfiguration config;

        public AccountController(DepanneurContext db, UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration config)
        {
            this.db = db;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.config = config;
        }

        [HttpGet("login"), AllowAnonymous]
        public IActionResult Login(string returnUrl = null, string invalidDomain = null, string deleted = null)
        {
            if (!string.IsNullOrWhiteSpace(invalidDomain))
            {
                ViewData["Message"] = "L'adresse courriel choisie n'est pas autorisée à utiliser le dépanneur.";
            }
            if (!string.IsNullOrWhiteSpace(deleted))
            {
                ViewData["Message"] = "Votre adresse courriel est marquée comme supprimée.";
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet("logout"), AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpPost("external-login"), AllowAnonymous, ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { ReturnUrl = returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }
        
        [HttpGet("external-login-callback"), AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login");
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login");
            }

            if (await UserIsDeleted(info))
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", new {deleted = 1});
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);
            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            else
            {
                if (await LinkToExistingUserAndSignIn(info))
                {
                    return RedirectToLocal(returnUrl);
                }

                if (!IsAuthorizedDomain(info))
                {
                    await signInManager.SignOutAsync();
                    return RedirectToAction("Login", new {invalidDomain = 1});
                }
                
                if (await CreateAndSignInUser(info))
                {
                    return RedirectToLocal(returnUrl);
                }

                await signInManager.SignOutAsync();
                return RedirectToAction("Login");
            }
        }

        private async Task<bool> UserIsDeleted(ExternalLoginInfo info)
        {
            var user = await db.Users.FirstOrDefaultAsync(
                u => u.Logins.Any(
                    l => l.LoginProvider == info.LoginProvider &&
                         l.ProviderKey == info.ProviderKey));

            return user != null && user.IsDeleted;
        } 

        private bool IsAuthorizedDomain(ExternalLoginInfo info)
        {
            var identity = (ClaimsIdentity)info.Principal.Identity;
            var email = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value?.ToLower();

            if (string.IsNullOrWhiteSpace(email)) return false;

            var whitelistedDomains = config.GetValue<string>("WhitelistedDomains")?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
            return whitelistedDomains.Any(d => email.EndsWith($"@{d}"));
        }

        private async Task<bool> LinkToExistingUserAndSignIn(ExternalLoginInfo info)
        {
            var identity = (ClaimsIdentity)info.Principal.Identity;
            var email = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return false;

            var result = await userManager.AddLoginAsync(user, info);
            if (!result.Succeeded) return false;

            return (await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true)).Succeeded;
        }

        private async Task<bool> CreateAndSignInUser(ExternalLoginInfo info)
        {
            var isFirstUser = !await db.Users.AnyAsync();

            var identity = (ClaimsIdentity)info.Principal.Identity;
            var user = new User
            {
                Name = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                UserName = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                Email = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value
            };

            var result = await userManager.CreateAsync(user);
            if (!result.Succeeded) return false;
            
            result = await userManager.AddLoginAsync(user, info);
            if (!result.Succeeded) return false;

            if (isFirstUser)
            {
                await userManager.AddToRolesAsync(user, Roles.All);
            }

            return (await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true)).Succeeded;
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction("Index", "App");
        }
    }
}
