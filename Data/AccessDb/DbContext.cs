using System;
using System.Data.OleDb;

namespace NextGenPOS.Data.AccessDb
{
    /// <summary>
    /// OLEDB connection helper for Microsoft Access .accdb files.
    /// </summary>
    public class DbContext : IDisposable
    {
        private readonly string _connectionString;

        public DbContext(string dbPath)
        {
            string fullPath = System.IO.Path.GetFullPath(dbPath);
            _connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={fullPath};Persist Security Info=False;";
        }

        public OleDbConnection CreateConnection()
        {
            return new OleDbConnection(_connectionString);
        }

        /// <summary>
        /// Opens a connection, executes the action, then closes.
        /// </summary>
        public void Execute(Action<OleDbConnection> action)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                action(conn);
            }
        }

        /// <summary>
        /// Opens a connection, executes the function, returns result.
        /// </summary>
        public T Query<T>(Func<OleDbConnection, T> func)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();
                return func(conn);
            }
        }

        public void Dispose() { /* connections are opened/closed per call */ }
    }
}
