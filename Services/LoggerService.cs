using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

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
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning($"{message}");
        }

        public void LogError(string message, Exception ex = null)
        {
            _logger.LogError(ex, $"{message}");
        }
    }
}
