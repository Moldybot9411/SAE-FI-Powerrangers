using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;

namespace SAE_FI;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum wvJsonMessageTypes
{
    SensorData,
    TempSettings
}

public record wvJsonMessage
(
    wvJsonMessageTypes Type,
    JsonElement Data
);

public partial class HeatmapPage : Page
{
    public HeatmapPage()
    {
        InitializeComponent();

        InitializeAsync();

        StartDatePicker.Date = new DateTime(2024, 1, 1);
        EndDatePicker.Date = new DateTime(2024, 12, 31);

        UpdateButton.Click += (e, a) =>
        {
            if (!StartDatePicker.Date.HasValue || !EndDatePicker.Date.HasValue)
            {
                MessageBox.Show("Please provide a valid Start and End Date.");
                return;
            }

            SendData(
                StartDatePicker.Date.Value,
                EndDatePicker.Date.Value,
                ColdSlider.Value,
                WarmSlider.Value
            );
        };
    }

    private async void InitializeAsync()
    {
        await webView.EnsureCoreWebView2Async();

        string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "src-heatmap", "dist");

        if (!Directory.Exists(localPath))
        {
            System.Diagnostics.Debug.WriteLine($"WARNUNG: Ordner nicht gefunden: {localPath}");
        }

        webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "app.assets",
            localPath,
            CoreWebView2HostResourceAccessKind.Allow
        );

        webView.NavigationCompleted += (s, e) =>
        {
            if (e.IsSuccess)
            {
                SendData(
                    DateTime.MinValue,
                    DateTime.MaxValue,
                    ColdSlider.Value,
                    WarmSlider.Value);
            }
        };

        webView.CoreWebView2.Navigate("https://app.assets/index.html");
    }

    private void SendData(DateTime start, DateTime end, double coldTemp, double warmTemp)
    {
        SensorStats[] data = MainWindow._appManager.GetSensorStats(start, end);

        if (data.Length == 0)
        {
            MessageBox.Show("The given Timeframe doesn't have any Sensor Data.");
            return;
        }

        var sensorDataJson = JsonSerializer.Serialize(
            new wvJsonMessage(
                wvJsonMessageTypes.SensorData,
                JsonSerializer.SerializeToElement(data)
            )
        );

        string tempSettingsJson = JsonSerializer.Serialize(
            new wvJsonMessage(
                wvJsonMessageTypes.TempSettings,
                JsonSerializer.SerializeToElement(
                    new { coldTemp, warmTemp }
                )
            )
        );

        webView.CoreWebView2.PostWebMessageAsJson(sensorDataJson);
        webView.CoreWebView2.PostWebMessageAsJson(tempSettingsJson);
    }
}