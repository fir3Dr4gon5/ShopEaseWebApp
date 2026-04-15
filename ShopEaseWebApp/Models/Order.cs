using Microsoft.AspNetCore.Identity;

namespace ShopEaseWebApp.Models
{
    // Models/Order.cs
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = null!;
        public string City { get; set; } = null!;
        public string PostCode { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? StripeCheckoutSessionId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public string? PaymentStatus { get; set; }

        public IdentityUser User { get; set; } = null!;
        public List<OrderItem> OrderItems { get; set; } = null!;
    }
}
