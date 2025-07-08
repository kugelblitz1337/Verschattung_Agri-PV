using API.Common.Enum;
using API.Common.Interfaces;
using System.Reflection;
using System.Resources;

namespace API.Common.Logger
{
    public class MailLogger : ILogger
    {
        private readonly IEMailService _emailService;
        private readonly ResourceManager _resourceManager;
        private const string subject = "Hopfenhunter Logging Mail";

        public MailLogger(IEMailService eMailService, ResourceManager resourceManager)
        {
            _emailService = eMailService;
            _resourceManager = resourceManager;
        }
        public void LogHint(string message, APITypes module, string source = "", string reference = "")
        {
           var loggingMessage = $"{module} [HINT] {message} {source} {reference} ".Trim();
            _emailService.SendEmail(_resourceManager.GetString("FromEmail"), _resourceManager.GetString("LoggingMail"), subject, loggingMessage, false);
        }

        public void LogDebug(string message, APITypes module, string source = "", string reference = "")
        {
            var loggingMessage = $"{module} [DEBUG] {message} {source} {reference} ".Trim();
            _emailService.SendEmail(_resourceManager.GetString("FromEmail"), _resourceManager.GetString("LoggingMail"), subject, loggingMessage, false);
        }

        public void LogError(string message, APITypes module, string source = "", string reference = "", Exception ex = null)
        {
            string exceptionMessage = string.Empty;
            if (ex != null)
            {
                exceptionMessage = $"{ex.Message} {ex.StackTrace}";
            }
            var loggingMessage = $"{module} [ERROR] {message} {source} {reference} {exceptionMessage}".Trim();
            _emailService.SendEmail(_resourceManager.GetString("FromEmail"), _resourceManager.GetString("LoggingMail"), subject, loggingMessage, false);
        }
    }
}
