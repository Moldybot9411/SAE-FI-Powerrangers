using Microsoft.Data.Sqlite;
using SAE_FI.Models;
using System.Collections.Generic;
using System.Windows;

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

            using var cmd = conn.CreateCommand();
            cmd.CommandText =
            """
            INSERT OR IGNORE INTO Measurements (Sensor, Timestamp, Value)
            VALUES ($sensor, $timestamp, $value);
            """;

            var sensorParam = cmd.CreateParameter();
            sensorParam.ParameterName = "$sensor";
            cmd.Parameters.Add(sensorParam);

            var timestampParam = cmd.CreateParameter();
            timestampParam.ParameterName = "$timestamp";
            cmd.Parameters.Add(timestampParam);

            var valueParam = cmd.CreateParameter();
            valueParam.ParameterName = "$value";
            cmd.Parameters.Add(valueParam);

            foreach (var r in rows)
            {
                sensorParam.Value = r.Sensor;
                timestampParam.Value = r.Timestamp;
                valueParam.Value = r.Value;

                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }

    }
}
