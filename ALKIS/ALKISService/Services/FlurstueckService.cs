using System.Diagnostics;
using ALKISService.Repository;

namespace ALKISService.Services
{
    public interface IFlurstueckService
    {
        Task ImportFlurstueckeAsync(string xmlPath);
        Task ImportFlurstueckFileAsync(string xmlPath);
        IEnumerable<FlurstueckEntity> GetByCounterNominator(string counter, string nominator, string marking);
        FlurstueckEntity GetByCoordinate(double x, double y, string marking);
    }

    public class FlurstueckService : IFlurstueckService
    {
        private readonly IFlurstueckRepository _repo;
        private readonly IGeometryService _geometryService;
        private readonly CoordinateTransformService _coordinateTransformService;

        public FlurstueckService(
            IFlurstueckRepository repo,
            IGeometryService geometryService,
            CoordinateTransformService coordinateTransformService)
        {
            _repo = repo;
            _geometryService = geometryService;
            _coordinateTransformService = coordinateTransformService;
        }

        public async Task ImportFlurstueckeAsync(string xmlPath)
        {
            _repo.EnsureSchema();

            foreach (var datei in Directory.EnumerateFiles(xmlPath, "*.xml"))
            {
                var batch = _geometryService.ParseFlurstueckeFast(datei);
                _repo.BulkInsert(batch);
                File.Delete(datei);
            }
        }
        public async Task ImportFlurstueckFileAsync(string xmlPath)
        {
            if (!File.Exists(xmlPath))
            {
                Console.WriteLine($"Die angegebene Datei '{xmlPath}' existiert nicht.");
                return;
            }

            _repo.EnsureSchema();
            var stopwatch = Stopwatch.StartNew();
            var batch = _geometryService.ParseFlurstueckeFast(xmlPath);
            stopwatch.Stop();

            Console.WriteLine($"Import für Datei '{xmlPath}' hat {stopwatch.Elapsed.TotalSeconds:F2} Sekunden gedauert.");

            _repo.BulkInsert(batch);
            //GC.Collect();
            //File.Delete(xmlPath);
            // Datei asynchron löschen, Fehler ggf. loggen
            Task.Run(() =>
            {
                try
                {
                    File.Delete(xmlPath);
                }
                catch (Exception ex)
                {
                    // Logging oder andere Behandlung, falls nötig
                    Console.WriteLine($"Fehler beim Löschen: {ex.Message}");
                }
            });
        }


        public IEnumerable<FlurstueckEntity> GetByCounterNominator(string counter, string nominator, string marking)
            => _repo.GetByCounterNominator(counter, nominator, marking);

        public FlurstueckEntity GetByCoordinate(double x, double y, string marking)
        {
            var transformedInput = _coordinateTransformService.TransformToUtm(x, y);
            var utmPoint = new NetTopologySuite.Geometries.Point(transformedInput[0], transformedInput[1]);
            var wktReader = new NetTopologySuite.IO.WKTReader();

            foreach (var f in _repo.GetByMarking(marking))
            {
                var polygon = wktReader.Read(f.wkt) as NetTopologySuite.Geometries.Polygon;
                if (polygon != null && polygon.Contains(utmPoint))
                    return f;
            }
            return null;
        }
    }

}
