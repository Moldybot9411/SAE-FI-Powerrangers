using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;

namespace SAE_FI;

public struct SensorData
{
    public string SensorId { get; set; }
    public double Temperature { get; set; }
}

public partial class HeatmapPage : Page
{
    public HeatmapPage()
    {
        InitializeComponent();

        InitializeAsync();

        SensorData[] data = {
            new SensorData { SensorId = "S1", Temperature = 26.5 },
            new SensorData { SensorId = "S2", Temperature = 80.0 }
        };

        string jsonString = JsonSerializer.Serialize(data);

        if (webView.CoreWebView2 != null)
        {
            Console.WriteLine("Sending Data to WebView.");
            webView.CoreWebView2.PostWebMessageAsJson(jsonString);
        }
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
                SendData();
            }
        };

        webView.CoreWebView2.Navigate("https://app.assets/index.html");
    }

    private void SendData()
    {
        SensorData[] data = {
            new SensorData { SensorId = "S1", Temperature = 90.0 },
            new SensorData { SensorId = "S2", Temperature = 80.0 },
            new SensorData { SensorId = "S3", Temperature = 50.0 },
            new SensorData { SensorId = "S4", Temperature = 60.0 },
            new SensorData { SensorId = "SD", Temperature = 30.0 },
            new SensorData { SensorId = "SB", Temperature = 40.0 },
        };

        string jsonString = JsonSerializer.Serialize(data);

        System.Diagnostics.Debug.WriteLine("Sende Daten an WebView...");
        webView.CoreWebView2.PostWebMessageAsJson(jsonString);
    }
}