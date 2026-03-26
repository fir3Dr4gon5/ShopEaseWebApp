namespace ShopEaseWebApp.Models
{
    // Models/Order.cs
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string City { get; set; }
        public string PostCode { get; set; }
        public string Status { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }
}
