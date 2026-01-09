using System.Windows;
using SAE_FI.Services;

namespace SAE_FI
{
    public partial class MainWindow : Window
    {
        private readonly CsvService _csvService = new();
        private readonly SqliteService _dbService = new("data.db");

        public MainWindow()
        {
            InitializeComponent();
            _dbService.ApplyMigration("./Migration/01-setup.sql");
        }

        private void ImportCSV(object sender, RoutedEventArgs e)
        {
            var rows = _csvService.Read("temps.csv");

            if (rows.Count == 0)
            {
                MessageBox.Show("Keine gültigen Daten gefunden.");
                return;
            }

            _dbService.Insert(rows);
            MessageBox.Show($"{rows.Count} Datensätze importiert.");
        }

        private void GetTemperatureStats(object sender, RoutedEventArgs e)
        {
            var stats = _dbService.GetTemperatureStats();
/*             var stats = _dbService.GetTemperatureStats(
                new DateTime(2024, 1, 1),
                new DateTime(2024, 1, 2)
            ); */

            MessageBox.Show(
                $"From {stats.StartDate:d} to {stats.EndDate:d}\n\n" +

                $"Min:\n" +
                $"  ID: {stats.Min.Id}\n" +
                $"  Temp: {stats.Min.Value:F1} °C\n" +
                $"  Sensor: {stats.Min.Sensor}\n" +
                $"  Time: {stats.Min.Timestamp}\n\n" +

                $"Max:\n" +
                $"  ID: {stats.Max.Id}\n" +
                $"  Temp: {stats.Max.Value:F1} °C\n" +
                $"  Sensor: {stats.Max.Sensor}\n" +
                $"  Time: {stats.Max.Timestamp}\n\n" +

                $"Avg: {stats.Average.Value:F1} °C"
            );
        }
    }
}