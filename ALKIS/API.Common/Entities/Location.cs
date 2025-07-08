using API.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.Entities
{
    /// <summary>
    /// Entität von Lokalitäten
    /// </summary>
    public class Location : IEntity
    {
        [Key]
        public int Id { get; set; }
        public string plz_code { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
    }
}
