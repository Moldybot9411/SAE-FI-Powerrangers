namespace SAE_FI.Models
{
    public class TemperatureAverage
    {
        public double Average { get; }

        public TemperatureAverage(double average)
        {
            Average = average;
        }

        public override string ToString() => Average.ToString("F2");
    }
}
