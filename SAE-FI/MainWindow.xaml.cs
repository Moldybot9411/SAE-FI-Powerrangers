using System.Windows;
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

        private void ImportCSV(string csvDatei)
        {
            var rows = _csvService.Read(csvDatei);

            if (rows.Count == 0)
            {
                MessageBox.Show("Keine gültigen Daten gefunden.");
                return;
            }

            _appManager.ImportCsvData(rows);
            MessageBox.Show($"{rows.Count} Datensätze importiert.");
        }
        private void DatenLöschen(object sender, RoutedEventArgs e)
        {
            _appManager.DeleteData();
            MessageBox.Show($"Datensätze gelöscht.");
        }
    }
}