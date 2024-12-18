using Microsoft.AspNetCore.Mvc;
using Pharm.Data;

namespace Pharmasuit.Controllers
{
    public class ProductsController : Controller
    {
        private readonly PharmContext _context;

        public ProductsController(PharmContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}
