using API.Common.DTO;

namespace API.Common.Interfaces
{
    /// <summary>
    /// Interface zum MailService zum versenden von Mails
    /// </summary>
    public interface IEMailService
    {

        /// <summary>
        /// Versand von E-Mails
        /// </summary>
        /// <param name="from">Absendermail</param>
        /// <param name="to">Empfängermail</param>
        /// <param name="subject">Betreff</param>
        /// <param name="body">Inhalt</param>
        /// <param name="html">IstHTML ja = true, nein = false</param>
        void SendEmail(string from, string to, string subject, string body, bool html);

        /// <summary>
        /// Versand der Bestätigungsmail
        /// </summary>
        /// <param name="priceAlert">Preisalarm</param>
        /// <param name="registeredPriceAlerts">registrierte Preisalarme</param>
        void SendConfirmationMail(PriceAlert priceAlert, List<PriceAlert> registeredPriceAlerts);
        /// <summary>
        /// Falsche Preismeldung per Mail versenden
        /// </summary>
        /// <param name="product">falscher Produktpreis</param>
        void SendReportWrongPriceMail(WrongProductPrice product);
    }
}