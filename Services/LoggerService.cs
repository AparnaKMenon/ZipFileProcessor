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

        public void LogInformation(string message, [CallerMemberName] string methodName = "")
        {
            _logger.LogInformation($"[{methodName}] {message}");
        }

        public void LogWarning(string message, [CallerMemberName] string methodName = "")
        {
            _logger.LogWarning($"[{methodName}] {message}");
        }

        public void LogError(string message, Exception ex = null, [CallerMemberName] string methodName = "")
        {
            _logger.LogError(ex, $"[{methodName}] {message}");
        }
    }
}
