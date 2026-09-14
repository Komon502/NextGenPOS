using System;
using System.Collections.Generic;
using System.Linq;
using NextGenPOS.Data;
using NextGenPOS.Models;

namespace NextGenPOS.Data.Mock
{
    public class MockPaymentRepository : IPaymentRepository
    {
        private readonly List<Payment> _payments = new List<Payment>();
        private int _nextId = 1;

        public List<Payment> GetAll() => _payments.ToList();
        public Payment GetById(int id) => _payments.FirstOrDefault(p => p.PaymentId == id);
        public Payment GetByOrder(int orderId) => _payments.FirstOrDefault(p => p.OrderId == orderId);
        public List<Payment> GetByDate(DateTime date) => _payments.Where(p => p.PaidAt.Date == date.Date).ToList();

        public int Insert(Payment p)
        {
            p.PaymentId = _nextId++;
            p.PaidAt = DateTime.Now;
            p.ReceiptNo = "R" + DateTime.Now.ToString("yyyyMMdd") + "-" + p.PaymentId.ToString("D4");
            _payments.Add(p);
            return p.PaymentId;
        }

        public void Update(Payment p)
        {
            var e = GetById(p.PaymentId);
            if (e == null) return;
            e.Method = p.Method; e.Amount = p.Amount;
        }

        public void Delete(int id) => _payments.RemoveAll(p => p.PaymentId == id);

        public DailySales GetDailySummary(DateTime date)
        {
            var dayPayments = GetByDate(date);
            return new DailySales
            {
                Date = date,
                TotalOrders = dayPayments.Count,
                TotalRevenue = dayPayments.Sum(p => p.Amount),
                CashTotal = dayPayments.Where(p => p.Method == "Cash").Sum(p => p.Amount),
                QrTotal = dayPayments.Where(p => p.Method == "QR").Sum(p => p.Amount),
            };
        }
    }
}
