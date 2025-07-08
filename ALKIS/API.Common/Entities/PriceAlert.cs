using API.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.Entities
{
    /// <summary>
    /// Entität von Preisalarm
    /// </summary>
    public class PriceAlert : IEntity
    {
        [Key]
        public int Id { get; set; }
        public string EMail { get; set; }
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public double Radius { get; set; }
        public string PostCode { get; set; }
        public DateTime LastMailSend { get; set; }
        public bool Confirmed { get; set; }
    }
}
