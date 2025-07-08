using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace API.Common
{
    /// <summary>
    /// Die Klasse stellt Funktionalitäten zum Abruf von API Daten zur Verfügung 
    /// </summary>
    public class CommonWebClient : IDisposable
    {

        private CommonWebClient() { }
        // Statische Instanz der Klasse
        private static CommonWebClient _webClient;
        public static CommonWebClient WebClient
        {
            get
            {
                // Wenn die Instanz noch nicht erstellt wurde, erstelle sie
                if (_webClient == null)
                {
                    _webClient = new CommonWebClient();
                }

                return _webClient;
            }
        }

        // Statische Instanz des ChromeDrivers
        private static ChromeDriver _chromeDriver;
        // Wird verwendet um DDOS Schutz von Rewe zu umgehen ansonsten können keine Anfragen an API gemacht werden
        public string GetHTMLCodeBypassCloudflare(string url, string regexSearchPattern)
        {
            // Wenn die Instanz noch nicht erstellt wurde dann machen
            if (_chromeDriver == null)
            {
                ChromeOptions chromeOptions = new ChromeOptions();
                chromeOptions.AddArgument("--headless"); // Headless-Modus, um den Browser nicht sichtbar zu machen
                chromeOptions.AddArgument("--disable-gpu"); // DisableGPU-Modus, um den Browser nicht sichtbar zu machen
                chromeOptions.AddArgument("--no-sandbox"); // DisableGPU-Modus, um den Browser nicht sichtbar zu machen
                chromeOptions.AddArgument("--enable-cookies"); // Cookies aktivieren

                _chromeDriver = new ChromeDriver(chromeOptions);
            }

            _chromeDriver.Navigate().GoToUrl(url);

            // Regex-Muster zum Extrahieren des JSON aus dem <pre>-Element
            Match match = Regex.Match(_chromeDriver.PageSource, regexSearchPattern, RegexOptions.Singleline);

            if (match.Success && !String.IsNullOrWhiteSpace(match.Groups[0].Value))
            {
                return match.Groups[1].Value;
            }
            else if(!String.IsNullOrWhiteSpace(_chromeDriver.PageSource))
            {
                return _chromeDriver.PageSource;
            }
            else
            {
                Console.WriteLine("Kein übereinstimmendes <pre>-Element gefunden.");
                return String.Empty;
            }
        }

        /// <summary>
        /// Ruft den HTML Code einer Webseite ab anhand einer HTTPRequestMessage
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="httpRequestMessage">befüllte RequestMessage</param>
        /// <returns>HTML Code zur Webseite</returns>
        public string GetHTMLCode(string url, HttpRequestMessage httpRequestMessage = null)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; AcmeInc/1.0)");
                // Webseite herunterladen
                if (httpRequestMessage != null)
                {
                    var response = client.SendAsync(httpRequestMessage).Result;
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    return client.GetStringAsync(url).Result;
                }
            }
        }

        public void Dispose()
        {
            _chromeDriver.Dispose();
        }
    }
}
