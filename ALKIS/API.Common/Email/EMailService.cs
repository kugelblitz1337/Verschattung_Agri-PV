using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using API.Common.DTO;
using API.Common.Enum;
using API.Common.Interfaces;
using Org.BouncyCastle.Utilities.IO;

namespace API.Common.Email
{
    /// <summary>
    /// 
    /// </summary>
    public class EMailService : IEMailService
    {
        private readonly SmtpClient _smtpClient;
        private readonly ResourceManager _resourceManager;
        private readonly ILogger _logger;

        /// <summary>
        /// Instanzieren des SmtpClients
        /// </summary>
        /// <param name="resourceManager">Ressourcenmanager</param>
        public EMailService(ResourceManager resourceManager, ILogger logger)
        {
            _resourceManager = resourceManager;
            _smtpClient = new SmtpClient(_resourceManager.GetString("SMTPHost"), Convert.ToInt32(_resourceManager.GetString("SMTPPort")))
            {
                Credentials = new NetworkCredential(_resourceManager.GetString("SMTPUser"), _resourceManager.GetString("SMTPUserPW")),
                EnableSsl = true
            };
            _logger = logger;
        }

        /// <summary>
        /// Versand von E-Mails
        /// </summary>
        /// <param name="from">Absendermail</param>
        /// <param name="to">Empfängermail</param>
        /// <param name="subject">Betreff</param>
        /// <param name="body">Inhalt</param>
        /// <param name="html">IstHTML ja = true, nein = false</param>
        public void SendEmail(string from, string to, string subject, string body, bool html)
        {
            try
            {
                var message = new MailMessage(from, to, subject, body);
                message.IsBodyHtml = html;
                _smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, APITypes.Common, "EmailService", "SendEmail", ex);
            }
        }

        /// <summary>
        /// Template für Bestätigungsmail abrufen
        /// </summary>
        /// <param name="confirmationLink">Bestätigungslink</param>
        /// <param name="priceAlerts">Preisalarme die mit ausgegeben werden</param>
        /// <returns>fertiges Mail HTML Template</returns>
        private string GetConfirmationHTMLMailTemplate(string confirmationLink, List<PriceAlert> priceAlerts)
        {
            StringBuilder emailContent = new StringBuilder();
            emailContent.Append("<html>");
            emailContent.Append("<head></head>");
            emailContent.Append("<body>");
            emailContent.Append("<p>Vielen Dank für dein Interesse am Bierpreiswecker!</p>");
            emailContent.Append("<p>Um sicherzustellen, dass du Zugriff auf unseren Service hast, bitten wir dich, deine E-Mail-Adresse zu bestätigen, indem du auf den untenstehenden Link klickst:</p>");
            emailContent.AppendFormat("<a href=\"{0}\">Bestätigungslink</a>", confirmationLink);
            emailContent.Append("<p>Nachdem du deine E-Mail-Adresse bestätigt hast, wirst du benachrichtigt, sobald der von dir gewählte Bierpreis den gewünschten Betrag erreicht oder unterschreitet.</p></br>");
            if (priceAlerts.Count() > 0)
            {
                emailContent.Append(GetRegisteredPriceAlertsHTMLMailTemplate(priceAlerts));
            }
            emailContent.Append("<p>Prost!</p>");
            emailContent.Append("</body>");
            emailContent.Append("</html>");

            return emailContent.ToString();
        }

        /// <summary>
        /// Alle registrierten Preisalarme zurückgeben
        /// </summary>
        /// <param name="priceAlerts">Preisalarme</param>
        /// <returns>fertiges HTML Template für Preisalarme</returns>
        private string GetRegisteredPriceAlertsHTMLMailTemplate(List<PriceAlert> priceAlerts)
        {
            StringBuilder emailContent = new StringBuilder();
            emailContent.Append(@"<style>
                        table {
                            width: 100%;
                            border-collapse: collapse;
                        }
                        th, td {
                            border: 1px solid #dddddd;
                            text-align: left;
                            padding: 8px;
                        }
                        th {
                            background-color: #f2f2f2;
                        }
                    </style>");

            emailContent.Append("<p>Du bist aktuell für folgende Preiswecker registriert:</p>");
            emailContent.Append("<table>");
            emailContent.Append("<tr><th>Produktname</th><th>Preis</th><th>Radius</th><th>Postleitzahl</th></tr>");

            foreach (var alert in priceAlerts)
            {
                emailContent.Append("<tr>");
                emailContent.Append($"<td>{alert.ProductName}</td>");
                emailContent.Append($"<td>{alert.Price}</td>");
                emailContent.Append($"<td>{alert.Radius}</td>");
                emailContent.Append($"<td>{alert.PostCode}</td>");
                emailContent.Append("</tr>");
            }

            emailContent.Append("</table></br>");

            return emailContent.ToString();

        }

        /// <summary>
        /// Versand der Bestätigungsmail
        /// </summary>
        /// <param name="priceAlert">Preisalarm</param>
        /// <param name="registeredPriceAlerts">registrierte Preisalarme</param>
        public void SendConfirmationMail(PriceAlert priceAlert, List<PriceAlert> registeredPriceAlerts)
        {
            SendEmail(_resourceManager.GetString("FromEmail"), priceAlert.EMail, $"Bestätige deinen Zugang zum Bierpreiswecker!"
                , GetConfirmationHTMLMailTemplate($"https://hopfenhunter.saufipedia.de:7777/Hopfenhunter/ConfirmPriceAlert?id={priceAlert.Id}&email={priceAlert.EMail}", registeredPriceAlerts), true);
        }

        public void SendReportWrongPriceMail(WrongProductPrice product)
        {
            SendEmail(_resourceManager.GetString("FromEmail"), _resourceManager.GetString("SupportEmail") + ", " + product.EMail, $"Preismeldung über fehlerhaften Preis!"
            , GetReportWrongPriceHTMLMailTemplate(product), true);

        }

        /// <summary>
        /// Template für das melden von falschen Preisen
        /// </summary>
        /// <param name="product"> befüllte Produktdaten</param>
        /// <returns>fertiges Mail HTML Template</returns>
        private string GetReportWrongPriceHTMLMailTemplate(WrongProductPrice product)
        {
            StringBuilder emailContent = new StringBuilder();
            emailContent.Append("<html>");
            emailContent.Append("<head>");
            emailContent.Append("<style>");
            emailContent.Append("body { font-family: Arial, sans-serif; }");
            emailContent.Append(".message { margin-bottom: 15px; }");
            emailContent.Append("</style>");
            emailContent.Append("</head>");
            emailContent.Append("<body>");
            emailContent.Append("<p class=\"message\">Vielen Dank für deine Rückmeldung!</p>");
            emailContent.Append($"<p class=\"message\">Du hast den Preis des Produkts <strong>{product.Name}</strong> aus dem Markt <strong>{product.Supermarket}</strong> als nicht aktuell gemeldet.</p>");
            emailContent.Append($"<p class=\"message\">Ein Mitglied des Hopfenhunter-Teams wird sich diese Abweichung ansehen und sie gegebenenfalls aktualisieren.</strong></p>");
            emailContent.Append("<p class=\"message\">Weiterhin viel Spaß beim Bierpreisvergleich und Prost!</p>");
            emailContent.Append("</body>");
            emailContent.Append("</html>");

            return emailContent.ToString();
        }
    }
}
