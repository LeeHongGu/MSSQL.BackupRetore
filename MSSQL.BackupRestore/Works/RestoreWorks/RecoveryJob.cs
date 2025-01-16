using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using MSSQL.BackupRestore.Configurations;
using MSSQL.BackupRestore.Enums;
using MSSQL.BackupRestore.Exceptions;
using MSSQL.BackupRestore.Extensions;
using MSSQL.BackupRestore.Interfaces;
using MSSQL.BackupRestore.Utils;
using MSSQL.BackupRestore.Works.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Works.RestoreWorks
{
    /// <summary>
    /// Manages and executes a series of SQL Server database restore operations.
    /// This class supports full, differential, and transaction log restores, ensuring that the restore process
    /// is performed in the correct sequence and with proper error handling.
    /// </summary>
    public class RecoveryJob : FileSystem, IBackupRestore, IRecoveryJob
    {
        /// <summary>
        /// A list of backup restore operations to be executed.
        /// </summary>
        private IList<IBackupRestore> _backupRestoreList = new List<IBackupRestore>();

        /// <summary>
        /// Logger instance for logging restore operations and errors.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Gets the name of the database associated with the recovery job.
        /// </summary>
        public string DatabaseName { get; }

        /// <summary>
        /// Event triggered when a restore operation is completed.
        /// </summary>
        public event ServerMessageEventHandler Complete;

        /// <summary>
        /// Event triggered when an informational message is received during a restore operation.
        /// </summary>
        public event ServerMessageEventHandler Information;

        /// <summary>
        /// Event triggered to report the progress of a restore operation in percentage.
        /// </summary>
        public event PercentCompleteEventHandler PercentComplete;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryJob"/> class.
        /// </summary>
        /// <param name="databaseName">The name of the database to restore.</param>
        /// <param name="logger">Optional logger for logging restore process information.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="databaseName"/> is null or empty.</exception>
        public RecoveryJob(string databaseName, ILogger logger = null)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentNullException(nameof(databaseName));
            DatabaseName = databaseName;
            _logger = logger;
        }

        /// <summary>
        /// Adds a restore operation to the recovery job.
        /// </summary>
        /// <param name="backupRestore">The restore operation to add.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="backupRestore"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the restore operation's database name does not match the recovery job's database.</exception>
        /// <exception cref="InvalidOperationException">Thrown if a duplicate restore operation is added.</exception>
        public void AddRestoreOperation(IBackupRestore backupRestore)
        {
            if (backupRestore is null)
                throw new ArgumentNullException(nameof(backupRestore));

            if (backupRestore.DatabaseName != DatabaseName)
                throw new ArgumentException("Database name must match the recovery job database name.", nameof(backupRestore));

            if (backupRestore is FullRestore && _backupRestoreList.Any(x => x is FullRestore))
                throw new InvalidOperationException("Only one full restore operation is allowed per recovery job.");

            if (backupRestore is DifferentialRestore && _backupRestoreList.Any(x => x is DifferentialRestore))
                throw new InvalidOperationException("Only one differential restore operation is allowed per recovery job.");

            backupRestore.Complete += (sender, e) => Complete?.Invoke(sender, e);
            backupRestore.Information += (sender, e) => Information?.Invoke(sender, e);
            backupRestore.PercentComplete += (sender, e) => PercentComplete?.Invoke(sender, e);

            _backupRestoreList.Add(backupRestore);
            _logger?.LogDebug("Added backup restore operation to recovery job for database {DatabaseName}.", DatabaseName);
        }

        /// <summary>
        /// Adds a full restore operation to the recovery job.
        /// </summary>
        /// <param name="filePath">The file path of the full backup.</param>
        public void FullRestore(string filePath)
        {
            ValidateFilePath(filePath);
            AddRestoreOperation(new FullRestore(DatabaseName, filePath));
        }

        /// <summary>
        /// Adds a differential restore operation to the recovery job.
        /// </summary>
        /// <param name="filePath">The file path of the differential backup.</param>
        public void DifferentialRestore(string filePath)
        {
            ValidateFilePath(filePath);
            AddRestoreOperation(new DifferentialRestore(DatabaseName, filePath));
        }

        /// <summary>
        /// Adds a transaction log restore operation to the recovery job.
        /// </summary>
        /// <param name="filePath">The file path of the transaction log backup.</param>
        public void TransactionLogRestore(string filePath)
        {
            ValidateFilePath(filePath);
            AddRestoreOperation(new TransactionLogRestore(DatabaseName, filePath));
        }

        /// <summary>
        /// Adds a restore operation by automatically determining the backup type based on the file.
        /// </summary>
        /// <param name="filePath">The backup file path.</param>
        /// <param name="server">The SQL Server instance to validate the backup type.</param>
        /// <exception cref="ArgumentException">Thrown when the file type cannot be determined.</exception>
        public void AddRestoreByFileName(string filePath, Server server)
        {
            ValidateFilePath(filePath);

            var backupType = BackupTypeExtensions.DetermineBackupType(filePath, server);

            switch (backupType)
            {
                case BackupType.Full:
                    FullRestore(filePath);
                    break;
                case BackupType.Differential:
                    DifferentialRestore(filePath);
                    break;
                case BackupType.TransactionLog:
                    TransactionLogRestore(filePath);
                    break;
                default:
                    throw new ArgumentException("Invalid file name. Unable to determine restore operation.", nameof(filePath));
            }
        }

        /// <summary>
        /// Executes all added restore operations in the correct order.
        /// </summary>
        /// <param name="server">The SQL Server instance where the restore will occur.</param>
        /// <param name="ct">A cancellation token for handling task cancellation.</param>
        /// <returns>A task that represents the asynchronous restore operation.</returns>
        /// <exception cref="BackupRestoreTaskException">Thrown if no restore operations are added or if any restore operation fails.</exception>
        public async Task ExecuteAsync(Server server, CancellationToken ct = default)
        {
            var orderedList = _backupRestoreList.OrderBy(x => x.GetType(), new RestoreComparer()).ToList();

            if (!orderedList.Any())
                throw new BackupRestoreTaskException("No restore operations have been added to the recovery job.");

            if (!server.TryGetDatabase(DatabaseName, out var database))
            {
                var newDatabase = new Database(server, DatabaseName);
                newDatabase.Create();
            }

            if (database?.Status == DatabaseStatus.Normal)
            {
                _logger?.LogDebug("Database {DatabaseName} is in normal state. Changing to single user mode.", DatabaseName);
                database.DatabaseOptions.UserAccess = DatabaseUserAccess.Single;
                database.Alter(TerminationClause.RollbackTransactionsImmediately);
            }

            foreach (var restore in orderedList)
            {
                try
                {
                    await restore.ExecuteAsync(server, ct);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Restore operation failed for {DatabaseName} during {RestoreType}.", DatabaseName, restore.GetType().Name);
                    throw new BackupRestoreTaskException($"Restore failed during {restore.GetType().Name} for {DatabaseName}.", ex);
                }
            }

            server.TryGetDatabase(DatabaseName, out var restoredDatabase);
            if (database?.Status == DatabaseStatus.Normal)
            {
                _logger?.LogDebug("Database {DatabaseName} restored successfully. Changing to multi user mode.", DatabaseName);
                restoredDatabase.DatabaseOptions.UserAccess = DatabaseUserAccess.Multiple;
                restoredDatabase.Alter(TerminationClause.RollbackTransactionsImmediately);
            }
        }
    }
}
