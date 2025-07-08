using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.DTO
{
    /// <summary>
    /// Preisalarm Transferklasse
    /// </summary>
    public class PriceAlert
    {
        public string EMail { get; set; }
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public double Radius { get; set; }
        public string PostCode { get; set; }
        public DateTime LastMailSend { get; set; }
        public bool Confirmed { get; set; }
    }
}
