using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pharm.Data;
using Pharm.Models;
using System.Linq;

namespace Pharm.Controllers
{
    public class OrdersController : Controller
    {
        private readonly PharmContext _context;

        public OrdersController(PharmContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult MyOrders()
        {
            // Retrieve the logged-in user's username or identifier
            var userId = User.Identity.Name;

            // Query orders for the logged-in user
            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product) // Include product details for each order
                .ToList();

            // Pass the orders to the view
            return View(orders);
        }

        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            // Fetch order details by order ID
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

    }
}
