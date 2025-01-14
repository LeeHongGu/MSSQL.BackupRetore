using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using MSSQL.BackupRestore.Exceptions;
using MSSQL.BackupRestore.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Works.RestoreWorks
{
    public class RecoveryJob : IBackupRestore, IRecoveryJob
    {
        private const string BACKUP_EXTENSION = ".bak";
        private const string LOG_EXTENSION = ".trn";

        private IList<IBackupRestore> _backupRestoreList = new List<IBackupRestore>();
        private readonly ILogger _logger;

        public string DatabaseName { get; }

        public event ServerMessageEventHandler Complete;
        public event ServerMessageEventHandler Information;
        public event PercentCompleteEventHandler PercentComplete;

        public void AddBackupRestore(IBackupRestore backupRestore)
        {
            if (backupRestore is null)
                throw new ArgumentNullException(nameof(backupRestore));

            if (backupRestore.DatabaseName != DatabaseName)
                throw new ArgumentException("Database name must match the recovery job database name.", nameof(backupRestore));

            if (backupRestore is FullRestore fullRestore)
            {
                if (_backupRestoreList.Any(x => x is FullRestore))
                    throw new InvalidOperationException("Only one full restore operation is allowed per recovery job.");
            }

            if (backupRestore is DifferentialRestore differentialRestore)
            {
                if (_backupRestoreList.Any(x => x is DifferentialRestore))
                    throw new InvalidOperationException("Only one differential restore operation is allowed per recovery job.");
            }

            backupRestore.Complete += (sender, e) => Complete?.Invoke(sender, e);
            backupRestore.Information += (sender, e) => Information?.Invoke(sender, e);
            backupRestore.PercentComplete += (sender, e) => PercentComplete?.Invoke(sender, e);

            _backupRestoreList.Add(backupRestore);
        }

        public void FullRestore(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Backup file not found.", filePath);
            if (!IsBackupFile(filePath))
                throw new BackupRestoreException(filePath, "Invalid backup file extension.");

            AddBackupRestore(new FullRestore(DatabaseName, filePath));
        }

        private static bool IsBackupFile(string filePath)
        {
            return Path.GetExtension(filePath) == BACKUP_EXTENSION || Path.GetExtension(filePath) == LOG_EXTENSION;
        }

        public Task ExecuteAsync(Server server, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
