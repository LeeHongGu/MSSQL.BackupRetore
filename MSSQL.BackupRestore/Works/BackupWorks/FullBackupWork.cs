using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Smo;
using MSSQL.BackupRestore.Exceptions;
using MSSQL.BackupRestore.Works.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Works.BackupWorks
{
    public class FullBackupWork : BackupBase
    {
        private readonly string _filePath;

        public FullBackupWork(string databaseName, string filePath, ILoggerFactory loggerFactory = null)
            : base(loggerFactory?.CreateLogger<FullBackupWork>(), databaseName, (backup) =>
            {
                backup.Action = BackupActionType.Database;
                backup.BackupSetName = $"{databaseName} Full Backup";
                backup.BackupSetDescription = $"{databaseName} Full Backup";
                backup.Initialize = true;
                backup.Incremental = false;
                backup.Checksum = true;
                backup.LogTruncation = BackupTruncateLogType.Truncate;
                backup.PercentCompleteNotification = 1;
                backup.ContinueAfterError = true;
            })
        {
            _filePath = filePath ?? throw new BackupRestoreException(new FileNotFoundException("File path cannot be null"));

            _logger?.LogDebug("Full backup created for database {databaseName} with file path {_filePath}", databaseName, _filePath);
        }

        protected override BackupDeviceItem SetDevice()
        {
            return new BackupDeviceItem(_filePath, DeviceType.File);
        }
    }
}
