
using System.Collections.Generic;
using ALKISService.Entities;
using ALKISService.Models;
using ALKISService.Services;

namespace ALKISService.Repository
{
    public class MarkingRepository : IMarkingRepository
    {
        private readonly IDbContext _dbContext;

        public MarkingRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
            CreateTableIfNotExists();
        }

        public IEnumerable<MarkingEntity> GetAll()
        {
            string sql = "SELECT Gemarkungsschluessel, Gemarkung,Dateiname, Checksum, Error FROM Gemarkungen";
            return _dbContext.Query<MarkingEntity>(sql);
        }
        private void CreateTableIfNotExists()
        {
            string sql = @"
CREATE TABLE IF NOT EXISTS Gemarkungen (
    Gemarkungsschluessel VARCHAR,
    Gemarkung VARCHAR,
    Dateiname VARCHAR,
    Checksum VARCHAR,
    Error VARCHAR
);";

            _dbContext.Execute(sql, new {});
        }

        public void InsertCsvDataIfNotExists(IEnumerable<GemarkungsRow> rows)
        {
            foreach (var row in rows)
            {
                string sql = @"INSERT INTO Gemarkungen (Gemarkungsschluessel, Gemarkung)  
                                 SELECT ?, ?  
                                 WHERE NOT EXISTS (  
                                     SELECT 1 FROM Gemarkungen WHERE Gemarkungsschluessel = ? AND Gemarkung = ?  
                                 )";

                _dbContext.Execute(sql, new
                {
                    Gemarkungsschluessel = row.Gemarkungsschluessel,
                    Gemarkung = row.Gemarkung,
                    where1 = row.Gemarkungsschluessel, // für WHERE NOT EXISTS
                    where2 = row.Gemarkung              // für WHERE NOT EXISTS
                });
            }
        }

        public void BulkInsert(IEnumerable<GemarkungsRow> rows)
        {
            using var tx = _dbContext.Connection.BeginTransaction();
            foreach (var row in rows)
            {
                string sql = @"
INSERT INTO Gemarkungen (Gemarkungsschluessel, Gemarkung, Checksum, dateiname)
SELECT ?, ?, '', ''
WHERE NOT EXISTS (
    SELECT 1 FROM Gemarkungen WHERE Gemarkungsschluessel = ? AND Gemarkung = ?
)";
                _dbContext.Execute(sql, new {
    row.Gemarkungsschluessel,
    row.Gemarkung,
    where1 = row.Gemarkungsschluessel, // für WHERE NOT EXISTS
    where2 = row.Gemarkung             // für WHERE NOT EXISTS
});
            }
            tx.Commit();
        }
        public bool ChecksumExists(string checksum, string dateiname)
        {
            string sql = "SELECT COUNT(*) FROM Gemarkungen WHERE Checksum = ? AND Dateiname = ?";
            var checksumexists = _dbContext.ExecuteScalar<long>(sql, new { Checksum = checksum, Dateiname = dateiname });
            return checksumexists > 0;
        }

        public void InsertChecksum(string checksum, string schluessel, string gemarkung, string dateiname)
        {
            string sql = @"UPDATE Gemarkungen   
                             SET Checksum = ?, dateiname = ?  
                             WHERE Gemarkungsschluessel = ? AND Gemarkung = ?";

            _dbContext.Execute(sql, new
            {
                Checksum = checksum,
                Dateiname = dateiname,
                Gemarkungsschluessel = schluessel,
                Gemarkung = gemarkung
            });
        }
        public void SetError(string schluessel, string gemarkung, string error)
        {
            string sql = @"UPDATE Gemarkungen 
                   SET Error = ? 
                   WHERE Gemarkungsschluessel = ? AND Gemarkung = ?";
            _dbContext.Execute(sql, new
            {
                Error = error,
                Gemarkungsschluessel = schluessel,
                Gemarkung = gemarkung
            });
        }

    }

    public interface IMarkingRepository
    {
        void InsertCsvDataIfNotExists(IEnumerable<GemarkungsRow> rows);
        bool ChecksumExists(string checksum, string dateiname);
        void InsertChecksum(string checksum, string schluessel, string gemarkung, string dateiname);
        void BulkInsert(IEnumerable<GemarkungsRow> rows);
        IEnumerable<MarkingEntity> GetAll();
        void SetError(string schluessel, string gemarkung, string error);
    }
}

