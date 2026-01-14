namespace SAE_FI.Models
{
    public class TemperatureStats
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public TemperatureAverage Average { get; }
        public TemperatureExtreme Min { get; }
        public TemperatureExtreme Max { get; }

        public TemperatureStats(
            DateTime startDate,
            DateTime endDate,
            TemperatureAverage average,
            TemperatureExtreme min,
            TemperatureExtreme max)
        {
            StartDate = startDate;
            EndDate = endDate;
            Average = average;
            Min = min;
            Max = max;
        }

        public override string ToString() =>
            $"From {StartDate} to {EndDate}: Avg={Average}, Min={Min}, Max={Max}";
    }
}
