using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Common;
using MSSQL.BackupRestore.Configurations;
using MSSQL.BackupRestore.Interfaces;
using MSSQL.BackupRestore.Works.BackupWorks;
using MSSQL.BackupRestore.Works.RestoreWorks;

namespace MSSQL.BackupRestore.Prototype
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var consoleLog = new ConsoleLoggerFactory();
            var log = consoleLog.CreateLogger<RecoveryJob>();

            var fullBackupPath = @"R:\DBBackup\2025.01\GS425_IEE_DB_BACKUP_250124.bak";

            // 백업 경로 유효성 검사
            if (!Directory.Exists(Path.GetDirectoryName(fullBackupPath)))
            {
                log.LogError("Invalid backup path: {fullBackupPath}", fullBackupPath);
                return;
            }

            IBackupRestore fullBackup = new FullBackup("GS425_IEE_DB", fullBackupPath, consoleLog);
            fullBackup.PercentComplete += (sender, e) =>
            {
                Console.WriteLine($"Full Backup Progress: {e.Percent}%");
                log.LogDebug("Full Backup Progress: {percent}%", e.Percent);
            };

            try
            {
                var server = ServerFactory.CreateServer(
                    "SQL_SERVER_ADDRESS",
                    "SQL_SERVER_ID",
                    "SQL_SERVER_PASSWORD",
                    serverMessageHandler: ServerMessageHandler
                );

                await fullBackup.ExecuteAsync(server);
            }
            catch (Exception ex)
            {
                log.LogError("An error occurred: {message}", ex.Message);
            }
        }

        static void ServerMessageHandler(ServerMessageEventArgs e)
        {
            Console.WriteLine($"Server Message: {e.Error.Message}");
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
            return logLevel >= LogLevel.Information; // 로그 레벨 필터
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine($"{logLevel}: {formatter(state, exception)}");
        }
    }
}
