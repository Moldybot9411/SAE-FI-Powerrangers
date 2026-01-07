using Microsoft.Data.Sqlite;
using SAE_FI.Models;
using System.IO;

namespace SAE_FI.Services
{
    public class SqliteService
    {
        private readonly string _dbPath;

        public SqliteService(string dbPath)
        {
            _dbPath = dbPath;
        }

        private SqliteConnection Open()
        {
            var conn = new SqliteConnection($"Data Source={_dbPath}");
            conn.Open();
            return conn;
        }

        public void ApplyMigration(string migrationPath)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = File.ReadAllText(migrationPath);
            cmd.ExecuteNonQuery();
        }

        public void Insert(IEnumerable<CsvRow> rows)
        {
            using var conn = Open();
            using var tx = conn.BeginTransaction();

            foreach (var r in rows)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText =
                """
                INSERT INTO Measurements (Sensor, Timestamp, Value)
                VALUES ($sensor, $timestamp, $value)
                """;

                cmd.Parameters.AddWithValue("$sensor", r.Sensor);
                cmd.Parameters.AddWithValue("$timestamp", r.Timestamp);
                cmd.Parameters.AddWithValue("$value", r.Value);

                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }
    }
}
