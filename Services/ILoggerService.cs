﻿namespace ZipFileProcessor.Services
{
    public interface ILoggerService
    {
        void LogInformation(string message="");
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
    }
}
