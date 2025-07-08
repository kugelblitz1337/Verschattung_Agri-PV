using API.Common.DTO;

namespace API.Common.Interfaces
{
    public interface IPriceAlertRepository
    {
        /// <summary>
        /// Alle Preisalarme abrufen aus Datenbank
        /// </summary>
        /// <returns>Preisalarme</returns>
        List<PriceAlert> GetPriceAlerts();
        /// <summary>
        /// Alle Preisalarme einer Mail abrufen aus Datenbank
        /// </summary>
        /// <returns>Preisalarme</returns>
        List<PriceAlert> GetPriceAlertsByMail(string email);
        /// <summary>
        /// Preisalarme löschen aus Datenbank
        /// </summary>
        /// <returns>Erfolg = true ansonsten false</returns>
        bool DeletePriceAlert(PriceAlert priceAlert);
        /// <summary>
        /// Preisalarme einfügen in Datenbank
        /// </summary>
        /// <returns>Erfolg</returns>
        bool InsertPriceAlert(PriceAlert priceAlert);
        /// <summary>
        /// Aktualisieren Preisalarm wann letzte Mail gesendet wurde
        /// </summary>
        /// <returns>Erfolg = true ansonsten false</returns>
        bool UpdatePriceAlertLastMailSend(PriceAlert priceAlert);
        /// <summary>
        /// Aktualisieren Preisalarm mit Bestätigungsflag
        /// </summary>
        /// <param name="id">PreisalarmId</param>
        /// <param name="email">Mail des Preisalarminhabers</param>
        /// <returns>Erfolg = true ansonsten false</returns>
        bool UpdatePriceAlertConfirmed(int id, string email);
        /// <summary>
        /// Preisalarmpreis aus Datenbank abholen
        /// </summary>
        /// <param name="priceAlert">Preisalarm der befüllt werden soll</param>
        /// <returns>Preisalarm befüllt mit Preis</returns>
        PriceAlert GetPriceAlert(PriceAlert priceAlert);
    }
}