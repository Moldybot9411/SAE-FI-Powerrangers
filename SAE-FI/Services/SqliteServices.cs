using Microsoft.Data.Sqlite;
using SAE_FI.Models;
using System.IO;
using System.Windows;

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

        public TemperatureStats GetTemperatureStats(
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? sensor = null)
        {
            using var conn = Open();

            // AVG
            using var avgCmd = conn.CreateCommand();
            avgCmd.CommandText =
            """
            SELECT AVG(Value)
            FROM Measurements
            WHERE ($sensor IS NULL OR Sensor = $sensor)
            AND ($start IS NULL OR Timestamp >= $start)
            AND ($end IS NULL OR Timestamp <= $end)
            """;

            avgCmd.Parameters.AddWithValue("$sensor", (object?)sensor ?? DBNull.Value);
            avgCmd.Parameters.AddWithValue("$start", (object?)startDate ?? DBNull.Value);
            avgCmd.Parameters.AddWithValue("$end", (object?)endDate ?? DBNull.Value);

            var avgValue = avgCmd.ExecuteScalar();
            if (avgValue == DBNull.Value)
                throw new InvalidOperationException("Keine Daten im Zeitraum.");

            var avg = new TemperatureAverage(Convert.ToDouble(avgValue));

            // MIN
            using var minCmd = conn.CreateCommand();
            minCmd.CommandText =
            """
            SELECT Id, Sensor, Timestamp, Value
            FROM Measurements
            WHERE ($sensor IS NULL OR Sensor = $sensor)
            AND ($start IS NULL OR Timestamp >= $start)
            AND ($end IS NULL OR Timestamp <= $end)
            ORDER BY Value ASC, Timestamp ASC, Id ASC
            LIMIT 1
            """;

            minCmd.Parameters.AddWithValue("$sensor", (object?)sensor ?? DBNull.Value);
            minCmd.Parameters.AddWithValue("$start", (object?)startDate ?? DBNull.Value);
            minCmd.Parameters.AddWithValue("$end", (object?)endDate ?? DBNull.Value);

            using var minReader = minCmd.ExecuteReader();
            minReader.Read();

            var min = new TemperatureExtreme(
                minReader.GetInt64(0),
                minReader.GetDouble(3),
                minReader.GetString(1),
                minReader.GetDateTime(2)
            );

            // MAX
            using var maxCmd = conn.CreateCommand();
            maxCmd.CommandText =
            """
            SELECT Id, Sensor, Timestamp, Value
            FROM Measurements
            WHERE ($sensor IS NULL OR Sensor = $sensor)
            AND ($start IS NULL OR Timestamp >= $start)
            AND ($end IS NULL OR Timestamp <= $end)
            ORDER BY Value DESC, Timestamp DESC, Id DESC
            LIMIT 1
            """;

            maxCmd.Parameters.AddWithValue("$sensor", (object?)sensor ?? DBNull.Value);
            maxCmd.Parameters.AddWithValue("$start", (object?)startDate ?? DBNull.Value);
            maxCmd.Parameters.AddWithValue("$end", (object?)endDate ?? DBNull.Value);

            using var maxReader = maxCmd.ExecuteReader();
            maxReader.Read();

            var max = new TemperatureExtreme(
                maxReader.GetInt64(0),
                maxReader.GetDouble(3),
                maxReader.GetString(1),
                maxReader.GetDateTime(2)
            );

            return new TemperatureStats(
                startDate ?? min.Timestamp,
                endDate ?? max.Timestamp,
                avg,
                min,
                max
            );
        }
    }
}
