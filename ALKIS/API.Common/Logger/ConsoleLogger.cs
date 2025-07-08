using API.Common.Enum;
using API.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Common.Logger
{
    /// <summary>
    /// Logger für Konsole
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        public void LogHint( string message, APITypes module, string source = "", string reference = "")
        {
            Console.WriteLine($"{module} [HINT] {message} {source} {reference} ".Trim());
        }

        public void LogDebug( string message, APITypes module, string source = "", string reference = "")
        {
            Console.WriteLine($"{module} [DEBUG] {message} {source} {reference} ".Trim());
        }

        public void LogError( string message, APITypes module, string source = "", string reference = "", Exception ex = null)
        {
            string exceptionMessage = string.Empty;
            if (ex != null)
            {
                exceptionMessage = $"{ex.Message} {ex.StackTrace}";
            }
            Console.WriteLine($"{module} [ERROR] {message} {source} {reference} {exceptionMessage}".Trim());
        }
    }
}
