using HotelBooking.Web.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Account;

    public class LoginModel(AppDbContext db) : PageModel
    {
        [BindProperty] public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required] [EmailAddress]
            public string Email { get; set; } = "";

            [Required] [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            public bool  RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            // Куда перенаправлять после выхода
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == Input.Email.ToLower());

            // Специально не разделяю "нет email" или "неверный пароль" - это защта от
            // User enumeration attack
            if (user == null || !BCrypt.Net.BCrypt.Verify(Input.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid password or email");
                return Page();
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties()
                {
                    IsPersistent = Input.RememberMe,
                    ExpiresUtc = Input.RememberMe
                        ? DateTimeOffset.UtcNow.AddDays(30)
                        : DateTimeOffset.UtcNow.AddHours(8)
                });

            // LocalRedirect защищает от open redirect attack
            return  LocalRedirect(returnUrl);
        }
    }
