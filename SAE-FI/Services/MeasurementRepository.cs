using Microsoft.Data.Sqlite;
using SAE_FI.Models;
using System.Collections.Generic;

namespace SAE_FI.Services
{
    public class MeasurementRepository
    {
        private readonly DatabaseConnectionFactory _connectionFactory;

        public MeasurementRepository(DatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void Delete()
        {
            using var conn = _connectionFactory.CreateConnection();
            using var tx = conn.BeginTransaction();


            using var clearCmd = conn.CreateCommand();
            clearCmd.CommandText = "DELETE FROM Measurements;";
            clearCmd.ExecuteNonQuery();
            tx.Commit();
        }

        public void Insert(IEnumerable<CsvRow> rows)
        {
            using var conn = _connectionFactory.CreateConnection();
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
