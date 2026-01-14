using Microsoft.Data.Sqlite;
using System.IO;

namespace SAE_FI.Services
{
    public class MigrationService
    {
        private readonly DatabaseConnectionFactory _connectionFactory;

        public MigrationService(DatabaseConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void ApplyMigration(string migrationPath)
        {
            using var conn = _connectionFactory.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = File.ReadAllText(migrationPath);
            cmd.ExecuteNonQuery();
        }
    }
}
