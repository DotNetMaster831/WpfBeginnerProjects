using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseBudgetManager.Services
{
    public interface ILoggerService
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message, Exception? ex = null);
        void LogDebug(string message);
    }
}
