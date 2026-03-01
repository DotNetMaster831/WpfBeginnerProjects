using ExpenseBudgetManager.Services;
using Serilog;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace ExpenseBudgetManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ILoggerService _logger;

        protected override void OnStartup(StartupEventArgs e)
        {
            //step -1 configure serilog
            


            base.OnStartup(e);
        }

        /// <summary>
        /// Serilog Configuration
        /// </summary>
        private static void ConfigureSerilog()
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", "app-.log");


            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentName()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}--{Exception}")
                .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] (Thread:{ThreadId}) {Message:lj}{NewLine}{Exception}").CreateLogger();
        }

        private void WireExceptionHandlers()
        {
            //Handler-1 UI thread exception
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            //Handler-2 Background thread
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            //Handler -3 Async task exceptions
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.LogError("UI thread exception", e.Exception);

            ShowErrorDailog("An unexpected error occured.", e.Exception.Message);

            e.Handled = true;
            
        }

        private void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            _logger?.LogError("Fatal background thread exception", ex);

            ShowErrorDailog("A fatal error occured.", ex!.Message ?? "Unknown error");

        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger?.LogError("Unobserved async task exception", e.Exception);
            e.SetObserved();
        }

        private static void ShowErrorDailog(string title, string message)
        {
            MessageBox.Show($"{message}\n\nDeatils have been saved to the log file", $"Expense Manager - {title}", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

}
