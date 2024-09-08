using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipFileProcessor.Services
{
    public interface ILoggerService
    {
        void LogInformation(string message="");
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
    }
}
