using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ALKISService.Models;
using CsvHelper;

namespace ALKISService.Services
{

    public interface ICsvImporter
    {
        List<GemarkungsRow> Import(string csvPath);
    }
    public class CsvImporter : ICsvImporter
    {
        public List<GemarkungsRow> Import(string csvPath)
        {
            using var reader = new StreamReader(csvPath);
            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,
                IgnoreBlankLines = true,
                MissingFieldFound = null, // kein Fehler bei fehlender Spalte
                PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
            };

            using var csv = new CsvReader(reader, config);
            return csv.GetRecords<GemarkungsRow>().ToList();
        }
    }

}
