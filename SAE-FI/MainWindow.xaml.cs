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
            _dbService.ApplyMigration("./Migration/migration.sql");
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
    }
}