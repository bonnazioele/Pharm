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

      


        public IActionResult OrderConfirmation()
        {
            return View(); // Create an OrderConfirmation.cshtml view to thank the user
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

        [HttpGet]
        public IActionResult Checkout()
        {
            var cartItems = _context.CartItems.Include(c => c.Product).ToList();
            return View(cartItems); // Pass cart items to the view
        }

        [HttpPost]
        public IActionResult ProcessPayment(string paymentMethod, string CardHolderName, string CardNumber, string ExpiryDate, string CVV)
        {
            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["ErrorMessage"] = "Please select a payment method.";
                return RedirectToAction("Checkout");
            }

            var cartItems = _context.CartItems.Include(c => c.Product).ToList();
            if (!cartItems.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Checkout");
            }


            // Process based on the selected payment method
            switch (paymentMethod)
            {
                case "CashOnDelivery":
                    ProcessCashOnDelivery(cartItems);
                    break;

                case "CardPayment":
                    if (string.IsNullOrWhiteSpace(CardHolderName) || string.IsNullOrWhiteSpace(CardNumber) ||
                        string.IsNullOrWhiteSpace(ExpiryDate) || string.IsNullOrWhiteSpace(CVV))
                    {
                        TempData["ErrorMessage"] = "Card details are required for Card Payment.";
                        return RedirectToAction("Checkout");
                    }
                    ProcessCardPayment(cartItems);
                    break;

                case "OnlinePayment":
                    ProcessOnlinePayment(cartItems);
                    break;

                default:
                    TempData["ErrorMessage"] = "Invalid payment method.";
                    return RedirectToAction("Checkout");
            }

            TempData["SuccessMessage"] = "Order placed successfully!";
            return RedirectToAction("OrderConfirmation");
        }

        private void ProcessCashOnDelivery(List<CartItem> cartItems)
        {
            CreateOrder(cartItems, "Cash on Delivery");
        }

        private void ProcessCardPayment(List<CartItem> cartItems)
        {
            CreateOrder(cartItems, "Card Payment");
        }

        private void ProcessOnlinePayment(List<CartItem> cartItems)
        {
            CreateOrder(cartItems, "Online Payment");
        }

        private void CreateOrder(List<CartItem> cartItems, string paymentMethod)
        {
            var order = new Order
            {
                UserId = User.Identity.Name,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(c => c.Quantity * c.Product.Price),
                PaymentMethod = paymentMethod,
                OrderStatus = "Pending" // Set default order status
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in cartItems)
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null && product.Stock >= item.Quantity)
                {
                    product.Stock -= item.Quantity;
                }

                var orderDetails = new OrderDetails
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };

                _context.OrderDetails.Add(orderDetails);
            }

            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();
        }



        [HttpPost]
        public async Task<IActionResult> CheckoutConfirm(IFormFile prescription)
        {
            var cartItems = _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == User.Identity.Name)
                .ToList();

            // Check if any product in the cart requires a prescription
            var requiresPrescription = cartItems.Any(item => item.Product.RequiresPrescription);

            if (requiresPrescription && prescription == null)
            {
                TempData["Error"] = "A prescription is required for some items in your cart.";
                return RedirectToAction("Checkout");
            }

            if (prescription != null)
            {
                // Save prescription to the server
                var filePath = Path.Combine("wwwroot/prescriptions", Guid.NewGuid().ToString() + Path.GetExtension(prescription.FileName));
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await prescription.CopyToAsync(stream);
                }

                // Store the prescription path for the order
                TempData["Success"] = "Prescription uploaded successfully.";
            }

            // Proceed with checkout logic
            var order = new Order
            {
                UserId = User.Identity.Name,
                OrderDate = DateTime.Now,
                TotalAmount = cartItems.Sum(item => item.Product.Price * item.Quantity),
                OrderStatus = "Pending"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cartItems)
            {
                var orderDetails = new OrderDetails
                {
                    OrderId = order.Id,
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };

                _context.OrderDetails.Add(orderDetails);
            }

            await _context.SaveChangesAsync();

            // Clear cart after checkout
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { id = order.Id });
        }


        [HttpGet]
        public IActionResult MyOrders()
        {
            var userId = User.Identity.Name;
            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        public IActionResult UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);
            if (order != null)
            {
                order.OrderStatus = newStatus;
                _context.SaveChanges();
            }

            return RedirectToAction("OrderManagement"); // Admin-specific view
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
