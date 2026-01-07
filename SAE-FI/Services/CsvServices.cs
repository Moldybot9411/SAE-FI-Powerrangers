using System.Globalization;
using SAE_FI.Models;
using System.IO;

namespace SAE_FI.Services
{
    public class CsvService
    {
        public List<CsvRow> Read(string path)
        {
            var rows = new List<CsvRow>();

            foreach (var line in File.ReadLines(path))
            {
                if (TryParseLine(line, out var row))
                    rows.Add(row!);
            }

            return rows;
        }

        private bool TryParseLine(string line, out CsvRow? row)
        {
            row = null;
            var parts = line.Split(',');

            if (parts.Length != 3)
                return false;

            if (!DateTime.TryParse(parts[1], CultureInfo.InvariantCulture, out var ts))
                return false;

            if (!double.TryParse(parts[2], CultureInfo.InvariantCulture, out var value))
                return false;

            row = new CsvRow(parts[0], ts, value);
            return true;
        }
    }
}
