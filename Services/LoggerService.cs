using Microsoft.Extensions.Logging;

namespace ZipFileProcessor.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly ILogger<LoggerService> _logger;

        public LoggerService(ILogger<LoggerService> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation($"{message}");
            Console.WriteLine("Information : " + message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning($"{message}");
            Console.WriteLine("Warning : " + message);
        }

        public void LogError(string message, Exception ex = null)
        {
            _logger.LogError(ex, $"{message}");
            Console.WriteLine("Error : " + message);
        }
    }
}
