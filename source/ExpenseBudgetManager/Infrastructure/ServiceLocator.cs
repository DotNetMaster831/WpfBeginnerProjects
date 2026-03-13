using ExpenseBudgetManager.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseBudgetManager.Infrastructure
{
    /// <summary>
    /// Simple service locator — wires dependencies without a DI container.
    /// Project 5 will replace this with Microsoft.Extensions.DI properly.
    /// </summary>
    public static class ServiceLocator
    {
        private static ILoggerService? _logger;
        private static IStorageService? _storage;

        public static ILoggerService Logger
            => _logger ??= new SerilogLoggerService();

        public static IStorageService Storage
            => _storage ??= new JsonStorageService(Logger);

        public static void Initialize()
        {
            // Force creation and log startup
            Logger.LogInformation("ServiceLocator initialized.");
            Logger.LogInformation($"Storage ready: {Storage.GetType().Name}");
        }
    }
}
