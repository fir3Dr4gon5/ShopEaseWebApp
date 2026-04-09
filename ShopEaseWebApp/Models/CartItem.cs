using Microsoft.AspNetCore.Identity;

namespace ShopEaseWebApp.Models
{
    // Models/CartItem.cs
    public class CartItem
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        public IdentityUser User { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
