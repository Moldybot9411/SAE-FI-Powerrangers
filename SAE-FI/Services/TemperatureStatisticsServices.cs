using Microsoft.Data.Sqlite;
using SAE_FI.Models;
using System;

namespace SAE_FI.Services
{
    public class TemperatureStatisticsService
    {
        private readonly DatabaseConnectionFactory _connectionFactory;

        public TemperatureStatisticsService(DatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private static void AddCommonFilters(SqliteCommand cmd, DateTime? startDate, DateTime? endDate, string? sensor)
        {
            cmd.Parameters.AddWithValue("$sensor", (object?)sensor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$start", (object?)startDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$end", (object?)endDate ?? DBNull.Value);
        }

        public TemperatureAverage GetAverage(SqliteConnection conn, DateTime? startDate, DateTime? endDate, string? sensor)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
            """
            SELECT AVG(Value)
            FROM Measurements
            WHERE ($sensor IS NULL OR Sensor = $sensor)
            AND ($start IS NULL OR Timestamp >= $start)
            AND ($end IS NULL OR Timestamp <= $end)
            """;

            AddCommonFilters(cmd, startDate, endDate, sensor);

            var result = cmd.ExecuteScalar();
            if (result == DBNull.Value)
                throw new InvalidOperationException("Keine Daten im Zeitraum.");

            return new TemperatureAverage(Convert.ToDouble(result));
        }

        private TemperatureExtreme GetExtreme(SqliteConnection conn, string order, DateTime? startDate, DateTime? endDate, string? sensor)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"""
            SELECT Id, Sensor, Timestamp, Value
            FROM Measurements
            WHERE ($sensor IS NULL OR Sensor = $sensor)
            AND ($start IS NULL OR Timestamp >= $start)
            AND ($end IS NULL OR Timestamp <= $end)
            ORDER BY Value {order}, Timestamp {order}, Id {order}
            LIMIT 1
            """;

            AddCommonFilters(cmd, startDate, endDate, sensor);

            using var r = cmd.ExecuteReader();
            if (!r.Read())
                throw new InvalidOperationException("Keine Daten im Zeitraum.");

            return new TemperatureExtreme(
                r.GetInt64(0),
                r.GetDouble(3),
                r.GetString(1),
                r.GetDateTime(2)
            );
        }

        public TemperatureExtreme GetMin(SqliteConnection conn, DateTime? startDate, DateTime? endDate, string? sensor)
            => GetExtreme(conn, "ASC", startDate, endDate, sensor);

        public TemperatureExtreme GetMax(SqliteConnection conn, DateTime? startDate, DateTime? endDate, string? sensor)
            => GetExtreme(conn, "DESC", startDate, endDate, sensor);

        public TemperatureStats GetTemperatureStats(DateTime? startDate = null, DateTime? endDate = null, string? sensor = null)
        {
            using var conn = _connectionFactory.CreateConnection();

            if (!startDate.HasValue || !endDate.HasValue)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText =
                """
                SELECT MIN(Timestamp), MAX(Timestamp)
                FROM Measurements
                WHERE ($sensor IS NULL OR Sensor = $sensor)
                """;
                cmd.Parameters.AddWithValue("$sensor", (object?)sensor ?? DBNull.Value);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    if (!startDate.HasValue && reader[0] != DBNull.Value)
                        startDate = reader.GetDateTime(0);

                    if (!endDate.HasValue && reader[1] != DBNull.Value)
                        endDate = reader.GetDateTime(1);
                }
                else
                {
                    throw new InvalidOperationException("Keine Daten in der Datenbank gefunden.");
                }
            }

            var avg = GetAverage(conn, startDate, endDate, sensor);
            var min = GetMin(conn, startDate, endDate, sensor);
            var max = GetMax(conn, startDate, endDate, sensor);

            return new TemperatureStats(
                startDate!.Value,
                endDate!.Value,
                avg,
                min,
                max
            );
        }

    }
}
