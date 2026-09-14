using System;
using NextGenPOS.Data;
using NextGenPOS.Models;

namespace NextGenPOS.Services
{
    /// <summary>
    /// Handles payment processing, receipt number generation, and table status reset.
    /// </summary>
    public class PaymentService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ITableRepository _tableRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly ConfigService _config;

        public PaymentService(IOrderRepository orderRepo, ITableRepository tableRepo,
            IPaymentRepository paymentRepo, ConfigService config)
        {
            _orderRepo = orderRepo;
            _tableRepo = tableRepo;
            _paymentRepo = paymentRepo;
            _config = config;
        }

        public Payment ProcessPayment(Order order, string method, decimal amountTendered)
        {
            var payment = new Payment
            {
                OrderId = order.OrderId,
                Method = method,
                Amount = order.GrandTotal,
                PaidAt = DateTime.Now
            };

            if (_paymentRepo != null)
                payment.PaymentId = _paymentRepo.Insert(payment);
            else
                payment.ReceiptNo = "R" + DateTime.Now.ToString("yyyyMMddHHmmss");

            // Mark order as paid
            order.Status = "Paid";
            _orderRepo.Update(order);

            // Free the table
            _tableRepo.UpdateStatus(order.TableId, "Available");

            return payment;
        }

        public decimal CalculateChange(decimal grandTotal, decimal tendered)
        {
            return Math.Max(0, tendered - grandTotal);
        }
    }
}
