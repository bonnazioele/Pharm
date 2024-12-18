using Microsoft.EntityFrameworkCore;
using Pharm.Models;


namespace Pharm.Data
{
    public class PharmContext : DbContext
    {
        public PharmContext(DbContextOptions<PharmContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Pharm.Models.Account> Accounts { get; set; } = default!;
        public DbSet<Pharm.Models.OrderDetails> OrderDetails { get; set; } = default!;
        public DbSet<Pharm.Models.Cart> Carts { get; set; } = default!;
        public DbSet<CartItem> CartItems { get; set; }

    }
}
