using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharm.Data;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pharm.Models;
using Microsoft.EntityFrameworkCore;

namespace Pharma.Controllers
{
    [Authorize] // Enforces login for all actions in this controller
    public class CartsController : Controller
    {
        private readonly PharmContext _context;

        public CartsController(PharmContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var userId = User.Identity.Name;

            var cartItems = _context.CartItems
                .Where(c => c.UserId == userId) // Retrieve items for the logged-in user
                .Include(c => c.Product)
                .ToList();

            return View(cartItems);
        }

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity)
        {
            var userId = User.Identity.Name;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Please log in to add items to the cart.";
                return RedirectToAction("Login", "Accounts");
            }

            if (productId <= 0 || quantity <= 0)
            {
                TempData["Error"] = "Invalid product or quantity.";
                return RedirectToAction("Index", "Products");
            }

            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index", "Products");
            }

            var existingCartItem = _context.CartItems
                .FirstOrDefault(c => c.ProductId == productId && c.UserId == userId);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity += quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UserId = userId
                };
                _context.CartItems.Add(cartItem);
            }

            _context.SaveChanges();

            TempData["Success"] = "Item added to cart successfully!";
            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            var userId = User.Identity.Name;

            var cartItems = _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            return View(cartItems);
        }

        [HttpPost]
        public async Task<IActionResult> CheckoutConfirm()
        {
            var userId = User.Identity.Name;

            var cartItems = _context.CartItems
                .Where(c => c.UserId == userId)
                .Include(c => c.Product)
                .ToList();

            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index");
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(c => c.Quantity * c.Product.Price)
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                var orderDetails = new OrderDetails
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };

                _context.OrderDetails.Add(orderDetails);
            }

            _context.CartItems.RemoveRange(cartItems); // Clear cart items
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = User.Identity.Name;

            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
