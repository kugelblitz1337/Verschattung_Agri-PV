using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.Interfaces
{
    public interface IPlattformScraper
    {
        /// <summary>
        /// Ruft alle Produkte von Plattformen ab
        /// </summary>
        /// <param name="postCode">Postleitzahl anhand der gesucht wird</param>
        /// <param name="radius">Radius in km in der Supermärkte gesucht werden</param>
        /// <returns>Liste von Angeboten</returns>
        List<API.Common.DTO.Product> GetProducts(string postCode, double radius);
    }
}
