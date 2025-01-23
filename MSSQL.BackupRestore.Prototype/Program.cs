using Microsoft.Extensions.Logging;
using MSSQL.BackupRestore.Works.RestoreWorks;

namespace MSSQL.BackupRestore.Prototype
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var consoleLog = new ConsoleLoggerFactory();
            var log = consoleLog.CreateLogger<RecoveryJob>();
        }
    }

    public class ConsoleLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ConsoleLogger();
        }

        public void Dispose()
        {
        }
    }

    public class ConsoleLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null!;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine(formatter(state, exception));
        }
    }
}
