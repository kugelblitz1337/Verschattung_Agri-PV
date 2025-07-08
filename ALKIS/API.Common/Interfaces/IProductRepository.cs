using API.Common.DTO;

namespace API.Common.Interfaces
{
    public interface IProductRepository
    {
        /// <summary>
        /// Einfügen von Produkten in die Datenbank
        /// </summary>
        /// <param name="products">Produkte</param>
        /// <returns>True = Erfolg, False = Fehlgeschlagen</returns>
        bool InsertProduct(List<Product> products);
        /// <summary>
        /// einzelnes Produkt anhand des Produktnamen abholen
        /// </summary>
        /// <param name="productName">Produktname</param>
        /// <returns>befülltes Produkt</returns>
        DTO.Product GetProduct(string productName);
    }
}