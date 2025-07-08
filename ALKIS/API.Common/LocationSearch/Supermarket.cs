using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.LocationSearch
{
    /// <summary>
    /// Bildet ein Transferobjekt für die Supermärkte aus der Suche ab
    /// </summary>
    public class Supermarket
    {
        public string Street { get; set; }
        public string PostCode { get; set; }
        public string City { get; set; }
        public string Housenumber { get; set; }
        public string Title { get; set; }
        public string Country { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public double Distance { get; set; }
    }
}
