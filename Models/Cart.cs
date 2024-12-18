namespace Pharm.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public virtual ICollection<CartItem> Items { get; set; }
    }


    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } // Navigation property
        public int Quantity { get; set; }
        public string UserId { get; set; }

        // Ensure this is populated in the controller
        public decimal Price => Product?.Price ?? 0;
    }


}
