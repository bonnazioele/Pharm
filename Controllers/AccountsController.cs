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
        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string address, string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                TempData["ErrorMessage"] = "All fields are required.";
                return View();
            }

            var existingUser = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email);
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "Email is already registered.";
                return View();
            }

            var newUser = new Account
            {
                UserName = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Address = address,
                PhoneNumber = phoneNumber // Set the phone number
            };

            _context.Accounts.Add(newUser);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, newUser.UserName),
        new Claim(ClaimTypes.Email, newUser.Email),
        new Claim(ClaimTypes.MobilePhone, newUser.PhoneNumber) // Add phone number as a claim
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["SuccessMessage"] = "Registration successful. Welcome!";
            return RedirectToAction("Index", "Home");
        }



        [HttpGet]
        public IActionResult EditProfile()
        {
            var userId = User.Identity.Name;
            var user = _context.Accounts.FirstOrDefault(u => u.UserName == userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult EditProfile(EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.Name;
                var user = _context.Accounts.FirstOrDefault(u => u.UserName == userId);

                if (user == null)
                {
                    return NotFound();
                }

                user.UserName = model.UserName;
                user.Email = model.Email;
                user.Address = model.Address;
                user.PhoneNumber = model.PhoneNumber;

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("EditProfile");
            }

            return View(model);
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
