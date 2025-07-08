using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace ALKISService.Models
{
    public class GemarkungsRow
    {
        public string Gemarkungsschluessel { get; set; }
        public string Gemarkung { get; set; }
        [Ignore]
        public string Dateiname { get; set; }
    }
}
