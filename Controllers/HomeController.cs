using Microsoft.AspNetCore.Mvc;
using Pharm.Data;

namespace Pharm.Controllers
{
    public class HomeController : Controller
    {
        private readonly PharmContext _context;

        public HomeController(PharmContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var featuredProducts = _context.Products.Take(4).ToList(); // Show 4 featured products
            return View(featuredProducts);
        }
    }
}
