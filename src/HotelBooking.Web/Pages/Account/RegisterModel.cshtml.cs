using HotelBooking.Web.Data;
using HotelBooking.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace HotelBooking.Web.Pages.Account
{
    public class RegisterModel(AppDbContext context) : PageModel
    {
        // [BindProperty] - binding properties of form with this oject on POST
        [BindProperty] public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "The Name {0} is required")]
            [StringLength(50)]
            public string FirstName { get; set; } = "";
            [Required(ErrorMessage = "The LastName {0} is required")]
            [StringLength(50)]
            public string LastName { get; set; } = "";
            [Required(ErrorMessage = "The Email {0} is required")]
            [EmailAddress(ErrorMessage = "Invalid Email Address")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "The Password {0} is required")]
            [DataType(DataType.Password)]
            [MinLength(6,  ErrorMessage = "The Password must be at least 6 characters")]
            public string Password { get; set; } = "";
            [Required(ErrorMessage = "The Confirm Password {0} is required")]
            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The Password and Confirm Password do not match")]
            public string ConfirmPassword { get; set; } = "";

        }

        public void OnGet()
        { }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (false == ModelState.IsValid)
            {
                return Page();
            }

            // Check unique Email
            /*
             * Почему это плохо: Когда вы вызываете ToLower() для колонки базы данных (x.Email),
             * база данных не может использовать обычный индекс по этому полю.
             * Ей приходится сканировать всю таблицу (Table Scan),
             * переводя каждый email в нижний регистр перед сравнением.
             *
             * Использовать EF.Functions.Like
             * Если вам нужно гарантировать поиск без учета регистра
             * вне зависимости от настроек базы:var emailExists = await context.Users
             *     .AnyAsync(x => EF.Functions.Like(x.Email, Input.Email), ct);
             */
             var emailExists = await context.Users
                 .AnyAsync(x => x.Email == Input.Email, ct);
             if (emailExists)
             {
                 /*
                  * В Razor Pages, когда вы используете свойство с атрибутом
                  * [BindProperty], например Input, ASP.NET Core ожидает,
                  * что ключ в словаре ошибок будет включать имя этого свойства.
                  */
                 ModelState.AddModelError("Input.Email", "Email already exists");
                 return Page();
             }

             // BCrypt automate add salt, work factor =12 (2^12 iteration of hash)
             var user = new User
             {
                 FirstName = Input.FirstName,
                 LastName = Input.LastName,
                 Email = Input.Email,
                 PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password, workFactor: 12)
             };

             context.Users.Add(user);
             await  context.SaveChangesAsync(ct);

             // After registration always login
             await SignInUserAsync(user);
             return RedirectToPage("/Index");

        }

        private async Task SignInUserAsync(User user)
        {
            // Claims - that set claims about user intro the token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()), // used to [Authorize(Roles="Admin")]
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // cipher principal and write into cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties {IsPersistent = true});
        }
    }
}
