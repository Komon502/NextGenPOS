namespace NextGenPOS.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } // Denormalized for display
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; } // Pending, Cooking, Done
    }
}
