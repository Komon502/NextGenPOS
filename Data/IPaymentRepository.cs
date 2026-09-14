using System;
using System.Collections.Generic;
using NextGenPOS.Models;

namespace NextGenPOS.Data
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        List<Payment> GetByDate(DateTime date);
        Payment GetByOrder(int orderId);
        DailySales GetDailySummary(DateTime date);
    }
}
