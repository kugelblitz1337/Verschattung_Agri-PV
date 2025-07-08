using API.Common.DTO;
using API.Common.Enum;

namespace API.Common.Interfaces
{
    public interface ICachingEngine
    {
        /// <summary>
        /// Produkte zum Cache hinzufügen
        /// </summary>
        /// <param name="apiType">APIType</param>
        /// <param name="product">Produkte</param>
        /// <returns>true=erfolgreich, false=nicht erfolgreich</returns>
        bool AddProductsToCache(APITypes apiType, Product product);
        /// <summary>
        /// Produkte aus dem Cache abrufen
        /// </summary>
        /// <param name="apiType">API Typ</param>
        /// <returns>Liste von Produkten</returns>
        List<Product> GetProductsFromCache(APITypes apiType);
        /// <summary>
        /// Cache zurücksetzen
        /// </summary>
        void DeleteProductsCache();
    }
}