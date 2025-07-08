using API.Common.Enum;
using API.Common.LocationSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.DTO
{
    /// <summary>
    /// Wrong ProductPrice
    /// </summary>
    public class WrongProductPrice
    {
        public string Name { get; set; }
        public decimal NewPrice { get; set; }
        public string Supermarket { get; set; }
        public string EMail { get; set; }

    }

}
