using System;

namespace NextGenPOS.Models
{
    public class DailySales
    {
        public DateTime Date { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CashTotal { get; set; }
        public decimal QrTotal { get; set; }
        public decimal DiscountTotal { get; set; }
        public decimal TaxTotal { get; set; }
    }
}
