using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Smo;
using MSSQL.BackupRestore.Works.Abstracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Works.BackupWorks
{
    /// <summary>
    /// Represents a transaction log backup operation for a specific SQL Server database.
    /// This class provides functionality to configure and execute transaction log backups.
    /// </summary>
    public class TransactionLogBackup : BackupBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLogBackup"/> class with default settings.
        /// </summary>
        /// <param name="databaseName">The name of the database to back up.</param>
        /// <param name="filePath">The file path where the backup will be saved.</param>
        /// <param name="loggerFactory">An optional logger factory to create logging instances.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="filePath"/> is null or empty.</exception>
        public TransactionLogBackup(string databaseName, string filePath, ILoggerFactory loggerFactory = null)
            : base(loggerFactory?.CreateLogger<FullBackup>(), databaseName, (backup) =>
            {
                backup.Action = BackupActionType.Log;
                backup.BackupSetName = $"{databaseName} Transaction Log Backup";
                backup.BackupSetDescription = $"{databaseName} Transaction Log Backup";
                backup.Initialize = false;
                backup.Checksum = true;
                backup.PercentCompleteNotification = 1;
                backup.ContinueAfterError = true;
            })
        {
            Initialize(filePath, databaseName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionLogBackup"/> class with custom settings.
        /// </summary>
        /// <param name="databaseName">The name of the database to back up.</param>
        /// <param name="filePath">The file path where the backup will be saved.</param>
        /// <param name="configureBackup">An action to customize the backup configuration.</param>
        /// <param name="loggerFactory">An optional logger factory to create logging instances.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="filePath"/> is null or empty.</exception>
        public TransactionLogBackup(string databaseName, string filePath, Action<Backup> configureBackup, ILoggerFactory loggerFactory = null)
            : base(loggerFactory?.CreateLogger<FullBackup>(), databaseName, configureBackup)
        {
            Initialize(filePath, databaseName);
        }

        /// <summary>
        /// Initializes the backup operation by validating the file path and logging the setup.
        /// </summary>
        /// <param name="filePath">The file path where the backup will be saved.</param>
        /// <param name="databaseName">The name of the database to back up.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="filePath"/> is null or empty.</exception>
        protected override void Initialize(string filePath, string databaseName)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath), "File path cannot be null");
            _logger?.LogDebug("Initialized transaction log backup for database {DatabaseName} with file path {filePath}", databaseName, _filePath);
        }

        /// <summary>
        /// Configures the backup device for the operation.
        /// </summary>
        /// <returns>A <see cref="BackupDeviceItem"/> representing the file device for the backup.</returns>
        protected override BackupDeviceItem SetDevice()
        {
            return new BackupDeviceItem(_filePath, DeviceType.File);
        }
    }
}
