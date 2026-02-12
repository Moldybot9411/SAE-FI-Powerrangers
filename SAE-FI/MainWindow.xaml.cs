using System.IO;
using System.Windows;
using Microsoft.Win32;
using SAE_FI.Services;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;

namespace SAE_FI
{
    public partial class MainWindow : FluentWindow
    {
        private readonly CsvService _csvService = new();

        static DatabaseConnectionFactory dbFactory = new DatabaseConnectionFactory("data.db");
        static MigrationService migrationService = new MigrationService(dbFactory);
        static MeasurementRepository repository = new MeasurementRepository(dbFactory);
        static TemperatureStatisticsService statsService = new TemperatureStatisticsService(dbFactory);
        public static readonly ApplicationManager _appManager = new ApplicationManager(migrationService, repository, statsService);

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (s, e) => RootNavigation.Navigate(typeof(HomePage));
            _appManager.ApplyMigration("./Migrations/01-setup.sql");
        }

        private void ImportCSV(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // 2. Set filters (optional) - e.g., show only text files
            openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // 3. Show the dialog and check if the user clicked "Open"
            // ShowDialog() returns a nullable bool (bool?). It is true if a file was selected.
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 4. Read the file content
                    string filePath = openFileDialog.FileName;
                    
                    var rows = _csvService.Read(filePath);
        
                    if (rows.Count == 0)
                    {
                        MessageBox.Show("Keine gültigen Daten gefunden.");
                        return;
                    }
        
                    _appManager.ImportCsvData(rows);
                    MessageBox.Show($"{rows.Count} Datensätze importiert.");
                

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading file: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void DatenLöschen(object sender, RoutedEventArgs e)
        {
            _appManager.DeleteData();
            MessageBox.Show($"Datensätze gelöscht.");
        }
    }
}