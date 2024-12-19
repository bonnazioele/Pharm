using Pharm.Models;

namespace Pharm.Data
{
    public static class DbInitializer
    {
        public static void Initialize(PharmContext context)
        {
            if (context.Products.Any()) return; // Database has been seeded

            var products = new Product[]
            {
                new Product { Name = "Paracetamol", Description = "Pain relief", Price = 5.99M, Stock = 100, ImageUrl = "images/paracetamol.jpg", RequiresPrescription = true },
                new Product { Name = "Ibuprofen", Description = "Anti-inflammatory", Price = 6.49M, Stock = 80, ImageUrl = "images/ibuprofen.jpg",  RequiresPrescription = false },
                new Product { Name = "Cough Syrup", Description = "For cough relief", Price = 4.99M, Stock = 50, ImageUrl = "images/coughsyrup.jpg",  RequiresPrescription = false },
                new Product { Name = "Vitamins", Description = "Boost immunity", Price = 9.99M, Stock = 70, ImageUrl = "images/vitamins.jpg", RequiresPrescription = false}
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
