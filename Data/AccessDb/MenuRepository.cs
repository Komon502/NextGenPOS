using System.Collections.Generic;
using System.Data.OleDb;
using NextGenPOS.Models;

namespace NextGenPOS.Data.AccessDb
{
    public class MenuRepository : IMenuRepository
    {
        private readonly DbContext _db;
        public MenuRepository(DbContext db) { _db = db; }

        public List<Category> GetCategories()
        {
            return _db.Query(conn =>
            {
                var list = new List<Category>();
                using (var cmd = new OleDbCommand("SELECT CategoryId, [Name], SortOrder, IsActive FROM Categories WHERE IsActive=True ORDER BY SortOrder", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new Category { CategoryId = r.GetInt32(0), Name = r.GetString(1), SortOrder = r.GetInt32(2), IsActive = r.GetBoolean(3) });
                return list;
            });
        }

        public List<MenuItem> GetByCategory(int catId)
        {
            return _db.Query(conn =>
            {
                var list = new List<MenuItem>();
                using (var cmd = new OleDbCommand("SELECT ItemId, CategoryId, [Name], Price, IsActive FROM MenuItems WHERE CategoryId=@c AND IsActive=True", conn))
                {
                    cmd.Parameters.AddWithValue("@c", catId);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            list.Add(new MenuItem { ItemId = r.GetInt32(0), CategoryId = r.GetInt32(1), Name = r.GetString(2), Price = r.GetDecimal(3), IsActive = r.GetBoolean(4) });
                }
                return list;
            });
        }

        public List<MenuItem> GetAll()
        {
            return _db.Query(conn =>
            {
                var list = new List<MenuItem>();
                using (var cmd = new OleDbCommand("SELECT ItemId, CategoryId, [Name], Price, IsActive FROM MenuItems WHERE IsActive=True", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        list.Add(new MenuItem { ItemId = r.GetInt32(0), CategoryId = r.GetInt32(1), Name = r.GetString(2), Price = r.GetDecimal(3), IsActive = r.GetBoolean(4) });
                return list;
            });
        }

        public MenuItem GetById(int id)
        {
            return _db.Query(conn =>
            {
                using (var cmd = new OleDbCommand("SELECT ItemId, CategoryId, [Name], Price, IsActive FROM MenuItems WHERE ItemId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        if (r.Read()) return new MenuItem { ItemId = r.GetInt32(0), CategoryId = r.GetInt32(1), Name = r.GetString(2), Price = r.GetDecimal(3), IsActive = r.GetBoolean(4) };
                }
                return null;
            });
        }

        public int Insert(MenuItem i)
        {
            return _db.Query(conn =>
            {
                using (var cmd = new OleDbCommand("INSERT INTO MenuItems (CategoryId,[Name],Price,IsActive) VALUES (@c,@n,@p,@a)", conn))
                {
                    cmd.Parameters.AddWithValue("@c", i.CategoryId);
                    cmd.Parameters.AddWithValue("@n", i.Name);
                    cmd.Parameters.AddWithValue("@p", i.Price);
                    cmd.Parameters.AddWithValue("@a", i.IsActive);
                    cmd.ExecuteNonQuery();
                }
                using (var c2 = new OleDbCommand("SELECT @@IDENTITY", conn))
                    return (int)(decimal)c2.ExecuteScalar();
            });
        }

        public void Update(MenuItem i)
        {
            _db.Execute(conn =>
            {
                using (var cmd = new OleDbCommand("UPDATE MenuItems SET CategoryId=@c,[Name]=@n,Price=@p,IsActive=@a WHERE ItemId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@c", i.CategoryId);
                    cmd.Parameters.AddWithValue("@n", i.Name);
                    cmd.Parameters.AddWithValue("@p", i.Price);
                    cmd.Parameters.AddWithValue("@a", i.IsActive);
                    cmd.Parameters.AddWithValue("@id", i.ItemId);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        public void Delete(int id)
        {
            _db.Execute(conn =>
            {
                using (var cmd = new OleDbCommand("UPDATE MenuItems SET IsActive=False WHERE ItemId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                }
            });
        }
    }
}
