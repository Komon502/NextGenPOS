using System;
using System.Collections.Generic;
using System.Data.OleDb;
using NextGenPOS.Models;

namespace NextGenPOS.Data.AccessDb
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContext _db;
        public OrderRepository(DbContext db) { _db = db; }

        public List<Order> GetAll() => _db.Query(conn => ReadOrders(conn, "SELECT * FROM Orders"));
        public Order GetById(int id) => _db.Query(conn => { var l = ReadOrders(conn, "SELECT * FROM Orders WHERE OrderId=" + id); return l.Count > 0 ? l[0] : null; });
        public List<Order> GetByTable(int tableId) => _db.Query(conn => ReadOrders(conn, $"SELECT * FROM Orders WHERE TableId={tableId} AND Status<>'Paid' AND Status<>'Cancelled'"));
        public List<Order> GetByStatus(string status) => _db.Query(conn => ReadOrders(conn, $"SELECT * FROM Orders WHERE Status='{status}'"));
        public List<Order> GetByDate(DateTime date) => _db.Query(conn => ReadOrders(conn, $"SELECT * FROM Orders WHERE OrderTime >= #{date:MM/dd/yyyy}# AND OrderTime < #{date.AddDays(1):MM/dd/yyyy}#"));

        private List<Order> ReadOrders(OleDbConnection conn, string sql)
        {
            var list = new List<Order>();
            using (var cmd = new OleDbCommand(sql, conn))
            using (var r = cmd.ExecuteReader())
                while (r.Read())
                    list.Add(new Order
                    {
                        OrderId = r.GetInt32(r.GetOrdinal("OrderId")),
                        TableId = r.GetInt32(r.GetOrdinal("TableId")),
                        OrderTime = r.GetDateTime(r.GetOrdinal("OrderTime")),
                        Status = r.GetString(r.GetOrdinal("Status")),
                        SubTotal = r.GetDecimal(r.GetOrdinal("SubTotal")),
                        DiscountPct = r.GetDouble(r.GetOrdinal("DiscountPct")),
                        TaxAmt = r.GetDecimal(r.GetOrdinal("TaxAmt")),
                        GrandTotal = r.GetDecimal(r.GetOrdinal("GrandTotal")),
                        StaffName = r.GetString(r.GetOrdinal("StaffName"))
                    });
            return list;
        }

        public int Insert(Order o)
        {
            return _db.Query(conn =>
            {
                using (var cmd = new OleDbCommand("INSERT INTO Orders (TableId,OrderTime,Status,SubTotal,DiscountPct,TaxAmt,GrandTotal,StaffName) VALUES (@t,@ot,@s,@st,@d,@ta,@gt,@sn)", conn))
                {
                    cmd.Parameters.AddWithValue("@t", o.TableId);
                    cmd.Parameters.AddWithValue("@ot", o.OrderTime == default(DateTime) ? DateTime.Now : o.OrderTime);
                    cmd.Parameters.AddWithValue("@s", o.Status ?? "New");
                    cmd.Parameters.AddWithValue("@st", o.SubTotal);
                    cmd.Parameters.AddWithValue("@d", o.DiscountPct);
                    cmd.Parameters.AddWithValue("@ta", o.TaxAmt);
                    cmd.Parameters.AddWithValue("@gt", o.GrandTotal);
                    cmd.Parameters.AddWithValue("@sn", o.StaffName ?? "");
                    cmd.ExecuteNonQuery();
                }
                using (var c2 = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return (int)(decimal)c2.ExecuteScalar();
            });
        }

        public void Update(Order o)
        {
            _db.Execute(conn =>
            {
                using (var cmd = new OleDbCommand("UPDATE Orders SET Status=@s,SubTotal=@st,DiscountPct=@d,TaxAmt=@ta,GrandTotal=@gt WHERE OrderId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@s", o.Status);
                    cmd.Parameters.AddWithValue("@st", o.SubTotal);
                    cmd.Parameters.AddWithValue("@d", o.DiscountPct);
                    cmd.Parameters.AddWithValue("@ta", o.TaxAmt);
                    cmd.Parameters.AddWithValue("@gt", o.GrandTotal);
                    cmd.Parameters.AddWithValue("@id", o.OrderId);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public void Delete(int id) { _db.Execute(conn => { using (var cmd = new OleDbCommand("DELETE FROM Orders WHERE OrderId=" + id, conn)) cmd.ExecuteNonQuery(); }); }

        public List<OrderItem> GetOrderItems(int orderId)
        {
            return _db.Query(conn =>
            {
                var list = new List<OrderItem>();
                using (var cmd = new OleDbCommand("SELECT oi.*, m.[Name] AS ItemName FROM OrderItems oi INNER JOIN MenuItems m ON oi.ItemId=m.ItemId WHERE oi.OrderId=@oid", conn))
                {
                    cmd.Parameters.AddWithValue("@oid", orderId);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new OrderItem
                            {
                                OrderItemId = r.GetInt32(r.GetOrdinal("OrderItemId")),
                                OrderId = r.GetInt32(r.GetOrdinal("OrderId")),
                                ItemId = r.GetInt32(r.GetOrdinal("ItemId")),
                                ItemName = r.GetString(r.GetOrdinal("ItemName")),
                                Qty = r.GetInt32(r.GetOrdinal("Qty")),
                                UnitPrice = r.GetDecimal(r.GetOrdinal("UnitPrice")),
                                Notes = r.IsDBNull(r.GetOrdinal("Notes")) ? "" : r.GetString(r.GetOrdinal("Notes")),
                                Status = r.GetString(r.GetOrdinal("Status"))
                            });
                }
                return list;
            });
        }

        public int AddOrderItem(OrderItem item)
        {
            return _db.Query(conn =>
            {
                using (var cmd = new OleDbCommand("INSERT INTO OrderItems (OrderId,ItemId,Qty,UnitPrice,Notes,Status) VALUES (@o,@i,@q,@u,@n,@s)", conn))
                {
                    cmd.Parameters.AddWithValue("@o", item.OrderId);
                    cmd.Parameters.AddWithValue("@i", item.ItemId);
                    cmd.Parameters.AddWithValue("@q", item.Qty);
                    cmd.Parameters.AddWithValue("@u", item.UnitPrice);
                    cmd.Parameters.AddWithValue("@n", item.Notes ?? "");
                    cmd.Parameters.AddWithValue("@s", item.Status ?? "Pending");
                    cmd.ExecuteNonQuery();
                }
                using (var c2 = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return (int)(decimal)c2.ExecuteScalar();
            });
        }

        public void UpdateOrderItem(OrderItem item)
        {
            _db.Execute(conn =>
            {
                using (var cmd = new OleDbCommand("UPDATE OrderItems SET Qty=@q,Notes=@n,Status=@s WHERE OrderItemId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@q", item.Qty);
                    cmd.Parameters.AddWithValue("@n", item.Notes ?? "");
                    cmd.Parameters.AddWithValue("@s", item.Status ?? "Pending");
                    cmd.Parameters.AddWithValue("@id", item.OrderItemId);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public void RemoveOrderItem(int id) { _db.Execute(conn => { using (var cmd = new OleDbCommand("DELETE FROM OrderItems WHERE OrderItemId=" + id, conn)) cmd.ExecuteNonQuery(); }); }
    }
}
