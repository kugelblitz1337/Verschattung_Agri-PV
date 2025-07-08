using ALKISService.Services;
using DuckDB.NET.Data;
using NetTopologySuite.Index.HPRtree;
using System.Data.Common;

namespace ALKISService.Repository
{
    public interface IFlurstueckRepository
    {
        void EnsureSchema();
        void BulkInsert(IEnumerable<FlurstueckEntity> flurstuecke);
        IEnumerable<FlurstueckEntity> GetAll();
        IEnumerable<FlurstueckEntity> GetByCounterNominator(string counter, string nominator, string marking);
        IEnumerable<FlurstueckEntity> GetByMarking(string marking);
        int Delete(string dateiname);
    }

    public class FlurstueckRepository : IFlurstueckRepository
    {
        private readonly IDbContext _dbContext;

        public FlurstueckRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void EnsureSchema()
        {
            string sql = @"
CREATE TABLE IF NOT EXISTS Flurstuecke (
    dateiname VARCHAR,
    zaehler VARCHAR,
    nenner VARCHAR,
    wkt VARCHAR,
    amtlicheFlaeche DOUBLE
);";
            _dbContext.Execute(sql, new { });
        }

        public void BulkInsert(IEnumerable<FlurstueckEntity> flurstuecke)
        {
            int batchSize = 1000;
            using var tx = _dbContext.Connection.BeginTransaction();

            for (int i = 0; i < flurstuecke.Count(); i += batchSize)
            {
                var batch = flurstuecke.Skip(i).Take(batchSize).ToList();
                var valueRows = new List<string>();
                var parameters = new List<DuckDBParameter>();

                foreach (var item in batch)
                {
                    valueRows.Add("(?, ?, ?, ?, ?)");
                    parameters.Add(new DuckDBParameter { Value = item.dateiname });
                    parameters.Add(new DuckDBParameter { Value = item.zaehler });
                    parameters.Add(new DuckDBParameter { Value = item.nenner });
                    parameters.Add(new DuckDBParameter { Value = item.wkt });
                    parameters.Add(new DuckDBParameter { Value = item.amtlicheFlaeche.ToString(System.Globalization.CultureInfo.InvariantCulture) });
                }

                string sql = $@"
INSERT INTO Flurstuecke (dateiname, zaehler, nenner, wkt, amtlicheFlaeche)
VALUES {string.Join(", ", valueRows)};
";
                _dbContext.Execute(sql, parameters);
            }
            tx.Commit();
        }
        public int Delete(string dateiname)
        {
            List<DuckDBParameter> parameters = new List<DuckDBParameter>();
            parameters.Add(new DuckDBParameter { Value = dateiname });

            string sql = $@"
DELETE FROM Flurstuecke WHERE dateiname = ?;
";
            return _dbContext.Execute(sql, parameters);


        }
        public IEnumerable<FlurstueckEntity> GetAll()
            => _dbContext.Query<FlurstueckEntity>("SELECT * FROM Flurstuecke");
        public IEnumerable<FlurstueckEntity> GetByMarking(string marking)
            => _dbContext.Query<FlurstueckEntity>("SELECT * FROM Flurstuecke INNER JOIN Gemarkungen ON Flurstuecke.dateiname = Gemarkungen.dateiname WHERE Gemarkung = ?", new { gemarkung = marking });

        public IEnumerable<FlurstueckEntity> GetByCounterNominator(string counter, string nominator, string marking)
            => _dbContext.Query<FlurstueckEntity>(
                "SELECT * FROM Flurstuecke INNER JOIN Gemarkungen ON Flurstuecke.dateiname = Gemarkungen.dateiname WHERE zaehler = ? AND nenner = ? AND gemarkung = ?",
                new { zaehler = counter, nenner = nominator, gemarkung = marking });
    }

    public class FlurstueckEntity
    {
        public string dateiname { get; set; }
        public string zaehler { get; set; }
        public string nenner { get; set; }
        public string wkt { get; set; }
        public double amtlicheFlaeche { get; set; }
    }

}
