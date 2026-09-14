using System;

namespace NextGenPOS.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public DateTime OrderTime { get; set; }
        public string Status { get; set; } // New, Preparing, Served, Paid, Cancelled
        public decimal SubTotal { get; set; }
        public double DiscountPct { get; set; }
        public decimal TaxAmt { get; set; }
        public decimal GrandTotal { get; set; }
        public string StaffName { get; set; }
    }
}
