using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.Management.Common;
using MSSQL.BackupRestore.Configurations;
using MSSQL.BackupRestore.Interfaces;
using MSSQL.BackupRestore.Prototype.Interfaces;
using MSSQL.BackupRestore.Prototype.Options;
using MSSQL.BackupRestore.Works.BackupWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Prototype.Jobs
{
    public class BackupJob : IBackupJob
    {
        private readonly BackupOptions _backupOptions;
        private readonly SqlServerOptions _sqlServerOptions;
        private readonly ILogger<BackupJob> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public BackupJob(
            IOptions<BackupOptions> backupOptions,
            IOptions<SqlServerOptions> sqlServerOptions,
            ILogger<BackupJob> logger,
            ILoggerFactory loggerFactory)
        {
            _backupOptions = backupOptions.Value;
            _sqlServerOptions = sqlServerOptions.Value;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public async Task ExecuteAsync()
        {
            string fullBackupPath = _backupOptions.FullBackupPath;

            // 백업 경로 유효성 검사 (디렉터리 존재 여부)
            string? backupDirectory = Path.GetDirectoryName(fullBackupPath);
            if (string.IsNullOrEmpty(backupDirectory) || !Directory.Exists(backupDirectory))
            {
                _logger.LogError("Invalid backup path: {FullBackupPath}", fullBackupPath);
                return;
            }

            // FullBackup 인스턴스 생성 (백업 라이브러리의 클래스)
            IBackupRestore fullBackup = new FullBackup(_backupOptions.DatabaseName, fullBackupPath, _loggerFactory);

            // 백업 진행률 이벤트 구독
            fullBackup.PercentComplete += (sender, e) =>
            {
                Console.WriteLine($"Full Backup Progress: {e.Percent}%");
                _logger.LogDebug("Full Backup Progress: {Percent}%", e.Percent);
            };

            try
            {
                // SQL 서버 생성 (라이브러리에서 제공하는 ServerFactory 사용)
                var server = ServerFactory.CreateServer(
                    _sqlServerOptions.Address,
                    _sqlServerOptions.UserId,
                    _sqlServerOptions.Password,
                    serverMessageHandler: ServerMessageHandler
                );

                // 백업 작업 실행
                await fullBackup.ExecuteAsync(server);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during backup execution.");
            }
        }

        // 서버 메시지 핸들러 (SQL 서버에서 전달하는 메시지 처리)
        private void ServerMessageHandler(ServerMessageEventArgs e)
        {
            Console.WriteLine($"Server Message: {e.Error.Message}");
        }
    }
}
