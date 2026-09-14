using System;
using System.Collections.Generic;
using System.Linq;
using NextGenPOS.Data;
using NextGenPOS.Models;

namespace NextGenPOS.Data.Mock
{
    public class MockOrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = new List<Order>();
        private readonly List<OrderItem> _items = new List<OrderItem>();
        private int _nextOrderId = 1;
        private int _nextItemId = 1;

        public List<Order> GetAll() => _orders.ToList();
        public Order GetById(int id) => _orders.FirstOrDefault(o => o.OrderId == id);
        public List<Order> GetByTable(int tableId) => _orders.Where(o => o.TableId == tableId && o.Status != "Paid" && o.Status != "Cancelled").ToList();
        public List<Order> GetByStatus(string status) => _orders.Where(o => o.Status == status).ToList();
        public List<Order> GetByDate(DateTime date) => _orders.Where(o => o.OrderTime.Date == date.Date).ToList();

        public int Insert(Order o)
        {
            o.OrderId = _nextOrderId++;
            o.OrderTime = DateTime.Now;
            _orders.Add(o);
            return o.OrderId;
        }

        public void Update(Order o)
        {
            var e = GetById(o.OrderId);
            if (e == null) return;
            e.Status = o.Status;
            e.SubTotal = o.SubTotal;
            e.DiscountPct = o.DiscountPct;
            e.TaxAmt = o.TaxAmt;
            e.GrandTotal = o.GrandTotal;
        }

        public void Delete(int id) { _orders.RemoveAll(o => o.OrderId == id); _items.RemoveAll(i => i.OrderId == id); }

        public List<OrderItem> GetOrderItems(int orderId) => _items.Where(i => i.OrderId == orderId).ToList();

        public int AddOrderItem(OrderItem item)
        {
            item.OrderItemId = _nextItemId++;
            item.Status = "Pending";
            _items.Add(item);
            return item.OrderItemId;
        }

        public void UpdateOrderItem(OrderItem item)
        {
            var e = _items.FirstOrDefault(i => i.OrderItemId == item.OrderItemId);
            if (e == null) return;
            e.Qty = item.Qty;
            e.Notes = item.Notes;
            e.Status = item.Status;
        }

        public void RemoveOrderItem(int id) => _items.RemoveAll(i => i.OrderItemId == id);
    }
}
