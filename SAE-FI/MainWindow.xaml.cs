using System;
using System.Windows;
using SAE_FI.Models;
using SAE_FI.Services;

namespace SAE_FI
{
    public partial class MainWindow : Window
    {
        private readonly CsvService _csvService = new(); 

        private readonly ApplicationManager _appManager;

        public MainWindow()
        {
            InitializeComponent();
            var dbFactory = new DatabaseConnectionFactory("data.db");
            var migrationService = new MigrationService(dbFactory);
            var repository = new MeasurementRepository(dbFactory);
            var statsService = new TemperatureStatisticsService(dbFactory);
            _appManager = new ApplicationManager(migrationService, repository, statsService);
            _appManager.ApplyMigration("./Migrations/01-setup.sql");
        }

        private void ImportCSV(object sender, RoutedEventArgs e)
        {
            var rows = _csvService.Read("temps.csv");

            if (rows.Count == 0)
            {
                MessageBox.Show("Keine gültigen Daten gefunden.");
                return;
            }

            _appManager.ImportCsvData(rows);
            MessageBox.Show($"{rows.Count} Datensätze importiert.");
        }

        private void GetTemperatureStats(object sender, RoutedEventArgs e)
        {
            var stats = _appManager.GetStats();

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