namespace SAE_FI.Models
{
    public class Measurement
    {
        public string Sensor { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }

        public Measurement() { }

        public Measurement(string sensor, DateTime timestamp, double value)
        {
            Sensor = sensor;
            Timestamp = timestamp;
            Value = value;
        }
    }
}

