using API.Common.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.Interfaces
{
    /// <summary>
    /// Logger zum loggen von Hinweisen, Error oder Debugmessages
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Hinweise protokollieren
        /// </summary>
        /// <param name="message">Nachricht</param>
        /// <param name="module">Modul (z.B. Common)</param>
        /// <param name="source">Klassenname</param>
        /// <param name="reference">Methodenname</param>
        void LogHint(string message, APITypes module, string source = "", string reference = "");
        /// <summary>
        /// Debugmeldungen protokollieren
        /// </summary>
        /// <param name="message">Nachricht</param>
        /// <param name="module">Modul (z.B. Common)</param>
        /// <param name="source">Klassenname</param>
        /// <param name="reference">Methodenname</param>
        void LogDebug(string message, APITypes module, string source = "", string reference = "");
        /// <summary>
        /// Errormeldungen protokollieren
        /// </summary>
        /// <param name="message">Nachricht</param>
        /// <param name="module">Modul (z.B. Common)</param>
        /// <param name="source">Klassenname</param>
        /// <param name="reference">Methodenname</param>
        void LogError(string message, APITypes module, string source = "", string reference = "", Exception ex = null);
    }
}
