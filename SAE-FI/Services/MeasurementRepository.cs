using Microsoft.Data.Sqlite;
using SAE_FI.Models;
using System.Collections.Generic;
using System.Text;
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

        public void Insert(IEnumerable<Measurement> rows)
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

        /// <summary>
        /// Ruft eine Liste aller eindeutigen Sensor-Namen aus der Datenbank ab.
        /// </summary>
        public List<string> GetDistinctSensors()
        {
            var sensors = new List<string>();
            using var conn = _connectionFactory.CreateConnection();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT Sensor FROM Measurements ORDER BY Sensor;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                sensors.Add(reader.GetString(0));
            }
            return sensors;
        }

        /// <summary>
        /// Ruft alle Messwerte ab, optional gefiltert.
        /// Dies ist die Hauptmethode, die von außen aufgerufen wird.
        /// </summary>
        public List<Measurement> GetMeasurementsForSensor(string sensorName, DateTime? startTime = null, DateTime? endTime = null)
        {
            var measurements = new List<Measurement>();
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();

            var sqlBuilder = new StringBuilder("SELECT Timestamp, Value FROM Measurements WHERE Sensor = $sensor");

            if (startTime.HasValue)
            {
                sqlBuilder.Append(" AND Timestamp >= $startTime");
                cmd.Parameters.AddWithValue("$startTime", startTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            if (endTime.HasValue)
            {
                sqlBuilder.Append(" AND Timestamp <= $endTime");
                cmd.Parameters.AddWithValue("$endTime", endTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            sqlBuilder.Append(" ORDER BY Timestamp;");
            
            cmd.CommandText = sqlBuilder.ToString();
            cmd.Parameters.AddWithValue("$sensor", sensorName); 
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var timestamp = reader.GetDateTime(0);
                var value = reader.GetDouble(1);
                
                measurements.Add(new Measurement
                {
                    Sensor = sensorName,
                    Timestamp = timestamp, 
                    Value = value
                });
            }
            return measurements;
        }
    }
}
