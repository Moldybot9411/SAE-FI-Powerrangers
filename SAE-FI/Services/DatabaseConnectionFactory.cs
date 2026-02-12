using System.Windows;
using Microsoft.Data.Sqlite;

namespace SAE_FI.Services
{
    public class DatabaseConnectionFactory
    {
        private readonly string _dbPath;

        public DatabaseConnectionFactory(string dbPath)
        {
            _dbPath = dbPath;
        }

        public SqliteConnection CreateConnection()
        {
            var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            return conn;
        }
    }
}
