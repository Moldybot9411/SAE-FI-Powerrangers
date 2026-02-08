using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;

namespace SAE_FI;

public partial class HeatmapPage : Page
{
    public HeatmapPage()
    {
        InitializeComponent();

        InitializeAsync();
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
        SensorStats[] data = MainWindow._appManager.GetSensorStats(DateTime.MinValue, DateTime.MaxValue);

        string jsonString = JsonSerializer.Serialize(data);

        webView.CoreWebView2.PostWebMessageAsJson(jsonString);
    }
}