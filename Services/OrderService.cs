using System;
using System.Collections.Generic;
using System.Linq;
using NextGenPOS.Data;
using NextGenPOS.Models;
using NextGenPOS.Services;

namespace NextGenPOS.Services
{
    /// <summary>
    /// Business logic for order operations: create, add items, calculate totals.
    /// </summary>
    public class OrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ITableRepository _tableRepo;
        private readonly decimal _taxRate;

        public OrderService(IOrderRepository orderRepo, ITableRepository tableRepo, ConfigService config)
        {
            _orderRepo = orderRepo;
            _tableRepo = tableRepo;
            _taxRate = config.GetDecimal("TaxRate", 7m) / 100m;
        }

        public Order CreateOrder(int tableId, string staffName)
        {
            var order = new Order
            {
                TableId = tableId,
                Status = "New",
                StaffName = staffName,
                OrderTime = DateTime.Now
            };
            order.OrderId = _orderRepo.Insert(order);
            _tableRepo.UpdateStatus(tableId, "Occupied");
            return order;
        }

        public Order GetOrCreateActiveOrder(int tableId, string staffName)
        {
            var existing = _orderRepo.GetByTable(tableId).FirstOrDefault(o => o.Status != "Paid" && o.Status != "Cancelled");
            return existing ?? CreateOrder(tableId, staffName);
        }

        public void AddItem(int orderId, MenuItem menuItem, int qty, string notes)
        {
            _orderRepo.AddOrderItem(new OrderItem
            {
                OrderId = orderId,
                ItemId = menuItem.ItemId,
                ItemName = menuItem.Name,
                Qty = qty,
                UnitPrice = menuItem.Price,
                Notes = notes ?? "",
                Status = "Pending"
            });
        }

        public void RecalcTotals(Order order)
        {
            var items = _orderRepo.GetOrderItems(order.OrderId);
            order.SubTotal = items.Sum(i => i.UnitPrice * i.Qty);
            decimal discountAmt = order.SubTotal * (decimal)(order.DiscountPct / 100.0);
            decimal afterDiscount = order.SubTotal - discountAmt;
            order.TaxAmt = Math.Round(afterDiscount * _taxRate, 2);
            order.GrandTotal = afterDiscount + order.TaxAmt;
            _orderRepo.Update(order);
        }

        public void SendToKitchen(Order order)
        {
            order.Status = "Preparing";
            _orderRepo.Update(order);
        }
    }
}
