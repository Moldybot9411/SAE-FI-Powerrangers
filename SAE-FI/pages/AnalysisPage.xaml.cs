using System.IO;
using System.Windows;
using System.Windows.Controls;
using SAE_FI.Services;
using Newtonsoft.Json;
using System.Windows.Media;

namespace SAE_FI
{
    public partial class AnalysisPage : Page
    {
        static DatabaseConnectionFactory dbFactory = new DatabaseConnectionFactory("data.db");
        static MeasurementRepository _measurementRepository = new MeasurementRepository(dbFactory);

        public AnalysisPage()
        {
            InitializeComponent();
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            InitializeWebView();
        }

        private async void WebView_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                webView.CoreWebView2.AddHostObjectToScript("backend", new BackendApi(_measurementRepository));
                webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            }
            else
            {
                MessageBox.Show($"WebView2 Initialization failed: {e.InitializationException.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CoreWebView2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                webView.CoreWebView2.NavigationCompleted -= CoreWebView2_NavigationCompleted;
                string backgroundColorHex = "#202020";
                if (Application.Current.Resources["ApplicationBackgroundBrush"] is SolidColorBrush backgroundBrush)
                {
                    Color backgroundColor = backgroundBrush.Color;
                    backgroundColorHex = $"#{backgroundColor.R:X2}{backgroundColor.G:X2}{backgroundColor.B:X2}";
                }
                
                string styleScript = $"applyHostStyles('{backgroundColorHex}');";
                await webView.CoreWebView2.ExecuteScriptAsync(styleScript);
                await LoadAndInjectSensorData();
                
                //webView.CoreWebView2.OpenDevToolsWindow();
            }
        }


        private async void InitializeWebView()
        {
            await webView.EnsureCoreWebView2Async(null);

            string projectDirectory = Environment.CurrentDirectory;
            string folderPath = Path.Combine(projectDirectory, "wwwroot");

            if (Directory.Exists(folderPath))
                {
                    webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        "app.local", 
                        folderPath, 
                        Microsoft.Web.WebView2.Core.CoreWebView2HostResourceAccessKind.Allow
                    );

                    webView.CoreWebView2.Navigate("https://app.local/index.html");
                }
                else
                {
                    MessageBox.Show($"Der Ordner 'wwwroot' wurde unter '{folderPath}' nicht gefunden.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        private async Task LoadAndInjectSensorData()
        {
            if (webView.CoreWebView2 == null) return;

            var distinctSensors = _measurementRepository.GetDistinctSensors();
            var sensorsJson = JsonConvert.SerializeObject(distinctSensors);

            string initialSensorDataJson = "[]";
            string initialSelectedSensor = "null";

            if (distinctSensors.Any())
            {
                initialSelectedSensor = JsonConvert.SerializeObject(distinctSensors.First());
                var firstSensor = distinctSensors.First();
                
                var measurements = _measurementRepository.GetMeasurementsForSensor(firstSensor);
                var chartData = measurements.Select(m => new { m.Timestamp, m.Value }).ToList();
                var jsonSettings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
                };
                initialSensorDataJson = JsonConvert.SerializeObject(chartData, jsonSettings);
            }

            string scriptToExecute = $"setupDashboard({sensorsJson}, {initialSensorDataJson}, {initialSelectedSensor});";
            await webView.CoreWebView2.ExecuteScriptAsync(scriptToExecute);
        }
    }
}