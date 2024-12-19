using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Pharm.Data;

namespace Pharm.Controllers
{
    public class AdminController : Controller
    {
        private readonly PharmContext _context; // Inject the database context
        private readonly IConfiguration _configuration;

        // Inject both PharmContext and IConfiguration
        public AdminController(PharmContext context, IConfiguration configuration)
        {
            _context = context; // Assign the injected context
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Check if the admin is already authenticated
            if (User.Identity.IsAuthenticated && User.HasClaim(c => c.Type == "Admin"))
            {
                // Redirect to the dashboard section
                var orders = _context.Orders.ToList(); // Fetch orders from the database
                return View("Dashboard", orders);
            }

            // Otherwise, show the login form
            return View("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            // Retrieve admin credentials from configuration
            var adminUsername = _configuration["Admin:Username"];
            var adminPassword = _configuration["Admin:Password"];

            if (username == adminUsername && password == adminPassword)
            {
                // Create admin claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim("Admin", "true")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // Redirect back to the same page to show the dashboard
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Invalid credentials.";
            return View("Login");
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string status)
        {
            // Find the order in the database
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                // Handle case where the order is not found
                TempData["Error"] = "Order not found.";
                return RedirectToAction("Index");
            }

            // Update the order status
            order.OrderStatus = status;

            // Save changes to the database
            _context.SaveChanges();

            TempData["Success"] = "Order status updated successfully.";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }
    }
}
