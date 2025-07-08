using API.Common.DTO;

namespace API.Common.Interfaces
{
    /// <summary>
    /// API Interface 
    /// </summary>
    public interface IAPIHandler
    {
        /// <summary>
        /// Alle Produkte zu einer PLZ und Radius abrufen
        /// </summary>
        /// <param name="postCode"></param>
        /// <param name="radiuskm"></param>
        /// <returns></returns>
        List<Product> GetProducts(string postCode, double radiuskm);
        /// <summary>
        /// Preisalarm Job ausführen
        /// </summary>
        void PriceAlert();
        /// <summary>
        /// Abmelden vom Preisalarm
        /// </summary>
        /// <param name="priceAlert">Preisalarm</param>
        void PriceAlertDeregister(PriceAlert priceAlert);
        /// <summary>
        /// Anmelden am Preisalarm
        /// </summary>
        /// <param name="priceAlert">Preisalarm</param>
        void PriceAlertRegister(PriceAlert priceAlert);

        /// <summary>
        /// Preisalarm bestätigen
        /// </summary>
        /// <param name="id">PreisalarmId</param>
        /// <param name="email">Mailadresse</param>
        void PriceAlertConfirmed(int id, string email);

        /// <summary>
        /// Preisgrenze zum Preisalarm abholen
        /// </summary>
        /// <param name="priceAlert">Preisalarm</param>
        /// <returns>Preisalarm befüllt mit Preis</returns>
        PriceAlert GetPriceAlert(PriceAlert priceAlert);
        void ReportWrongPrice(WrongProductPrice request);
    }
}