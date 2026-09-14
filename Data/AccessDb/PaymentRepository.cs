using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using NextGenPOS.Models;

namespace NextGenPOS.Data.AccessDb
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly DbContext _db;
        public PaymentRepository(DbContext db) { _db = db; }

        public List<Payment> GetAll() => _db.Query(conn => ReadPayments(conn, "SELECT * FROM Payments"));
        public Payment GetById(int id) => _db.Query(conn => { var l = ReadPayments(conn, "SELECT * FROM Payments WHERE PaymentId=" + id); return l.Count > 0 ? l[0] : null; });
        public Payment GetByOrder(int orderId) => _db.Query(conn => { var l = ReadPayments(conn, "SELECT * FROM Payments WHERE OrderId=" + orderId); return l.Count > 0 ? l[0] : null; });
        public List<Payment> GetByDate(DateTime date) => _db.Query(conn => ReadPayments(conn, $"SELECT * FROM Payments WHERE PaidAt >= #{date:MM/dd/yyyy}# AND PaidAt < #{date.AddDays(1):MM/dd/yyyy}#"));

        private List<Payment> ReadPayments(OleDbConnection conn, string sql)
        {
            var list = new List<Payment>();
            using (var cmd = new OleDbCommand(sql, conn))
            using (var r = cmd.ExecuteReader())
                while (r.Read())
                    list.Add(new Payment
                    {
                        PaymentId = r.GetInt32(r.GetOrdinal("PaymentId")),
                        OrderId = r.GetInt32(r.GetOrdinal("OrderId")),
                        Method = r.GetString(r.GetOrdinal("Method")),
                        Amount = r.GetDecimal(r.GetOrdinal("Amount")),
                        PaidAt = r.GetDateTime(r.GetOrdinal("PaidAt")),
                        ReceiptNo = r.GetString(r.GetOrdinal("ReceiptNo"))
                    });
            return list;
        }

        public int Insert(Payment p)
        {
            return _db.Query(conn =>
            {
                using (var cmd = new OleDbCommand("INSERT INTO Payments (OrderId,Method,Amount,PaidAt,ReceiptNo) VALUES (@o,@m,@a,@pa,@rn)", conn))
                {
                    cmd.Parameters.AddWithValue("@o", p.OrderId);
                    cmd.Parameters.AddWithValue("@m", p.Method);
                    cmd.Parameters.AddWithValue("@a", p.Amount);
                    cmd.Parameters.AddWithValue("@pa", p.PaidAt == default(DateTime) ? DateTime.Now : p.PaidAt);
                    cmd.Parameters.AddWithValue("@rn", p.ReceiptNo ?? ("R" + DateTime.Now.ToString("yyyyMMddHHmmss")));
                    cmd.ExecuteNonQuery();
                }
                using (var c2 = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return (int)(decimal)c2.ExecuteScalar();
            });
        }

        public void Update(Payment p)
        {
            _db.Execute(conn =>
            {
                using (var cmd = new OleDbCommand("UPDATE Payments SET Method=@m,Amount=@a WHERE PaymentId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@m", p.Method);
                    cmd.Parameters.AddWithValue("@a", p.Amount);
                    cmd.Parameters.AddWithValue("@id", p.PaymentId);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public void Delete(int id) { _db.Execute(conn => { using (var cmd = new OleDbCommand("DELETE FROM Payments WHERE PaymentId=" + id, conn)) cmd.ExecuteNonQuery(); }); }

        public DailySales GetDailySummary(DateTime date)
        {
            var payments = GetByDate(date);
            return new DailySales
            {
                Date = date,
                TotalOrders = payments.Count,
                TotalRevenue = payments.Sum(p => p.Amount),
                CashTotal = payments.Where(p => p.Method == "Cash").Sum(p => p.Amount),
                QrTotal = payments.Where(p => p.Method == "QR").Sum(p => p.Amount)
            };
        }
    }
}
