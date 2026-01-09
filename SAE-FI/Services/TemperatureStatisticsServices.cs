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

        public TemperatureAverage GetAverage(DateTime? startDate, DateTime? endDate, string? sensor)
        {
            using var conn = _connectionFactory.CreateConnection();
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

        private TemperatureExtreme GetExtreme(string order, DateTime? startDate, DateTime? endDate, string? sensor)
        {
            using var conn = _connectionFactory.CreateConnection();
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

        public TemperatureExtreme GetMin(DateTime? startDate, DateTime? endDate, string? sensor)
            => GetExtreme("ASC", startDate, endDate, sensor);

        public TemperatureExtreme GetMax(DateTime? startDate, DateTime? endDate, string? sensor)
            => GetExtreme("DESC", startDate, endDate, sensor);

        public TemperatureStats GetTemperatureStats(DateTime? startDate = null, DateTime? endDate = null, string? sensor = null)
        {
            var avg = GetAverage(startDate, endDate, sensor);
            var min = GetMin(startDate, endDate, sensor);
            var max = GetMax(startDate, endDate, sensor);

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
