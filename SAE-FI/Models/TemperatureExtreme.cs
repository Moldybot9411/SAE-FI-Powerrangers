namespace SAE_FI.Models
{
    public class TemperatureExtreme
    {
        public long Id { get; }
        public double Value { get; }
        public string Sensor { get; }
        public DateTime Timestamp { get; }

        public TemperatureExtreme(long id, double value, string sensor, DateTime timestamp)
        {
            Id = id;
            Value = value;
            Sensor = sensor;
            Timestamp = timestamp;
        }

        public override string ToString() => $"{Sensor} - {Value}°C at {Timestamp}";
    }
}
