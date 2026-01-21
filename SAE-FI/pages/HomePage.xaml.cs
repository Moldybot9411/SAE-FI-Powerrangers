using System.Windows;
using System.Windows.Controls;
using SAE_FI.Services;

namespace SAE_FI;

public partial class HomePage : Page
{

    public HomePage()
    {
        InitializeComponent();
    }

    private void GetTemperatureStats(object sender, RoutedEventArgs e)
    {
        var stats = MainWindow._appManager.GetStats();
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