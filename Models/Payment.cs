using System;

namespace NextGenPOS.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string Method { get; set; } // Cash, QR, Split
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }
        public string ReceiptNo { get; set; }
    }
}
