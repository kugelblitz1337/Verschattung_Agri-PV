using API.Common.LocationSearch;

namespace API.Common.Interfaces
{
    public interface ISupermarketSearch
    {
        /// <summary>
        /// Alle Supermärkte im Umkreis finden
        /// </summary>
        /// <param name="qryList">Suchbegriffe</param>
        /// <param name="postCode">Postleitzahl</param>
        /// <param name="radiuskm">Radius in Kilometer</param>
        /// <returns></returns>
        List<Supermarket> RadiusSearchAddressQry(List<string> qryList, string postCode, double radiuskm);
    }
}