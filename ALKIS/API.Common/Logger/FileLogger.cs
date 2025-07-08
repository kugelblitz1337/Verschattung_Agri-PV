using API.Common.Enum;
using API.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.Logger
{
    public class FileLogger : ILogger
    {
        private String filePath;

        public FileLogger(String filePath)
        {
            this.filePath = filePath;
        }

        public void LogHint(string message, APITypes module, string source = "", string reference = "")
        {
            logToFile($"{module} [HINT] {message} {source} {reference}".Trim());
        }

        public void LogDebug(string message, APITypes module, string source = "", string reference = "")
        {
            logToFile($"{module} [DEBUG] {message} {source} {reference}".Trim());
        }

        public void LogError(string message, APITypes module, string source = "", string reference = "", Exception ex = null)
        {
            string exceptionMessage = string.Empty;
            if (ex != null) 
            {
                exceptionMessage = $"{ex.Message} {ex.StackTrace}";   
            }
            logToFile($"{module} [ERROR] {message} {source} {reference} ".Trim());
        }

        private void logToFile(String message)
        {

            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    // Schreibe die formatierte Nachricht in die Datei
                    writer.WriteLine(FormatLogMessage(message));
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Fehler beim Schreiben in die Datei: " + e.Message);
            }
        }

        private string FormatLogMessage(string message)
        {
            // Hier kannst du die Log-Nachricht nach Bedarf formatieren
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        }

    }
}
