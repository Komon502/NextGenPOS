using System;
using System.Collections.Generic;
using NextGenPOS.Models;

namespace NextGenPOS.Data
{
    public interface IOrderRepository : IRepository<Order>
    {
        List<Order> GetByTable(int tableId);
        List<Order> GetByStatus(string status);
        List<Order> GetByDate(DateTime date);
        List<OrderItem> GetOrderItems(int orderId);
        int AddOrderItem(OrderItem item);
        void UpdateOrderItem(OrderItem item);
        void RemoveOrderItem(int orderItemId);
    }
}
