using System.Collections.Generic;
using System.Data.OleDb;
using NextGenPOS.Models;

namespace NextGenPOS.Data.AccessDb
{
    public class TableRepository : ITableRepository
    {
        private readonly DbContext _db;
        public TableRepository(DbContext db) { _db = db; }

        public List<Table> GetAll()
        {
            return _db.Query(conn =>
            {
                var list = new List<Table>();
                using (var cmd = new OleDbCommand("SELECT TableId, TableName, Seats, Status, ZoneArea FROM [Tables]", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Table { TableId = r.GetInt32(0), TableName = r.GetString(1), Seats = r.GetInt32(2), Status = r.GetString(3), ZoneArea = r.GetString(4) });
                return list;
            });
        }

        public Table GetById(int id)
        {
            return _db.Query(conn =>
            {
                using (var cmd = new OleDbCommand("SELECT TableId, TableName, Seats, Status, ZoneArea FROM [Tables] WHERE TableId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        if (r.Read()) return new Table { TableId = r.GetInt32(0), TableName = r.GetString(1), Seats = r.GetInt32(2), Status = r.GetString(3), ZoneArea = r.GetString(4) };
                }
                return null;
            });
        }

        public int Insert(Table t)
        {
            return _db.Query(conn =>
            {
                using (var cmd = new OleDbCommand("INSERT INTO [Tables] (TableName, Seats, Status, ZoneArea) VALUES (@n,@s,@st,@z)", conn))
                {
                    cmd.Parameters.AddWithValue("@n", t.TableName);
                    cmd.Parameters.AddWithValue("@s", t.Seats);
                    cmd.Parameters.AddWithValue("@st", t.Status);
                    cmd.Parameters.AddWithValue("@z", t.ZoneArea);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd2 = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return (int)(decimal)cmd2.ExecuteScalar();
            });
        }

        public void Update(Table t)
        {
            _db.Execute(conn =>
            {
                using (var cmd = new OleDbCommand("UPDATE [Tables] SET TableName=@n, Seats=@s, Status=@st, ZoneArea=@z WHERE TableId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@n", t.TableName);
                    cmd.Parameters.AddWithValue("@s", t.Seats);
                    cmd.Parameters.AddWithValue("@st", t.Status);
                    cmd.Parameters.AddWithValue("@z", t.ZoneArea);
                    cmd.Parameters.AddWithValue("@id", t.TableId);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public void Delete(int id)
        {
            _db.Execute(conn =>
            {
                using (var cmd = new OleDbCommand("DELETE FROM [Tables] WHERE TableId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public void UpdateStatus(int tableId, string status)
        {
            _db.Execute(conn =>
            {
                using (var cmd = new OleDbCommand("UPDATE [Tables] SET Status=@s WHERE TableId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@s", status);
                    cmd.Parameters.AddWithValue("@id", tableId);
                    cmd.ExecuteNonQuery();
                }
            });
        }
    }
}
