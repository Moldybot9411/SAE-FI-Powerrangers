using SAE_FI.Models;
using SAE_FI.Services;
using System;
using System.Collections.Generic;

namespace SAE_FI
{
    public class ApplicationManager
    {
        private readonly MigrationService _migrationService;
        private readonly MeasurementRepository _repository;
        private readonly TemperatureStatisticsService _statsService;

        public ApplicationManager(
            MigrationService migrationService,
            MeasurementRepository repository,
            TemperatureStatisticsService statsService)
        {
            _migrationService = migrationService;
            _repository = repository;
            _statsService = statsService;
        }

        // Migration
        public void ApplyMigration(string migrationPath)
        {
            _migrationService.ApplyMigration(migrationPath);
        }

        // CSV-Daten importieren
        public void ImportCsvData(IEnumerable<CsvRow> rows)
        {
            _repository.Insert(rows);
        }
        // Daten löschen
        public void DeleteData()
        {
            _repository.Delete();
        }

        // Stats
        public TemperatureStats GetStats(DateTime? start = null, DateTime? end = null, string? sensor = null)
        {
            return _statsService.GetTemperatureStats(start, end, sensor);
        }

        public SensorStats[] GetSensorStats(DateTime start, DateTime end)
        {
            return _statsService.GetSensorStats(start, end);
        }
    }
}
