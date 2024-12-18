using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharm.Models;
using Pharm.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Pharm.Controllers
{
    public class AccountsController : Controller
    {
        private readonly PharmContext _context;

        public AccountsController(PharmContext context)
        {
            _context = context;
        }

        // ========================= REGISTER =========================

        // GET: Register Page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            // Check if the email already exists
            if (await _context.Accounts.AnyAsync(a => a.Email == email))
            {
                ModelState.AddModelError("", "An account with this email already exists.");
                return View();
            }

            // Hash the password
            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Account>();
            var account = new Account
            {
                UserName = username,
                Email = email
            };
            account.PasswordHash = passwordHasher.HashPassword(account, password);

            // Save the account to the database
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            // Log in the user
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, account.UserName),
        new Claim(ClaimTypes.Email, account.Email)
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }


        // ========================= LOGIN =========================

        // GET: Login Page
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Handle Login Form Submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View();
            }

            // Find the user by email
            var user = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            // Verify the password
            var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<Account>();
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            // Log in the user
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email)
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }


        // ========================= LOGOUT =========================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
