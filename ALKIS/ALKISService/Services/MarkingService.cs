using ALKISService.Entities;
using ALKISService.Models;
using ALKISService.Repository;
using System.Globalization;

namespace ALKISService.Services
{
    public interface IMarkingService
    {
        void ImportGemarkungenFromCsv(string csvPath);
        IEnumerable<MarkingEntity> GetAll();
    }

    public class MarkingService : IMarkingService
    {
        private readonly IMarkingRepository _repo;

        public MarkingService(IMarkingRepository repo)
        {
            _repo = repo;
        }

        public void ImportGemarkungenFromCsv(string csvPath)
        {
            // Beispielhaft, mit CsvHelper (kannst du natürlich nach deinem Bedarf anpassen)
            using var reader = new StreamReader(csvPath);
            //using var csv = new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);

            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,
                IgnoreBlankLines = true,
                PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
            };
            using var csv = new CsvHelper.CsvReader(reader, config);
            var records = csv.GetRecords<GemarkungsRow>().ToList();
            _repo.BulkInsert(records);
        }

        public IEnumerable<MarkingEntity> GetAll() => _repo.GetAll();
    }

}
