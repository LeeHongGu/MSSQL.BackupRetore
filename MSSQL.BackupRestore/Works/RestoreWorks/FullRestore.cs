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

namespace MSSQL.BackupRestore.Works.RestoreWorks
{
    /// <summary>
    /// Represents a full restore operation for an MSSQL database.
    /// This class provides functionality to configure and execute a complete database restore.
    /// </summary>
    public class FullRestore : RestoreBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FullRestore"/> class with default restore settings.
        /// </summary>
        /// <param name="databaseName">The name of the database to restore.</param>
        /// <param name="filePath">The file path of the backup to restore from.</param>
        /// <param name="noRecovery">Indicates whether the database should remain in a restoring state after the restore operation. Defaults to <c>true</c>.</param>
        /// <param name="loggerFactory">Optional logger factory for creating loggers.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> is null.</exception>
        public FullRestore(
            string databaseName,
            string filePath,
            bool noRecovery = true,
            ILoggerFactory loggerFactory = null)
            : base(loggerFactory?.CreateLogger<FullRestore>(), databaseName, (restore) =>
            {
                restore.Action = RestoreActionType.Database;
                restore.ReplaceDatabase = true;
                restore.PercentCompleteNotification = 1;
                restore.ContinueAfterError = true;
                restore.NoRecovery = noRecovery;
            })
        {
            Initialize(filePath, databaseName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FullRestore"/> class with custom restore settings.
        /// </summary>
        /// <param name="databaseName">The name of the database to restore.</param>
        /// <param name="filePath">The file path of the backup to restore from.</param>
        /// <param name="configureRestore">A delegate to customize the restore configuration.</param>
        /// <param name="loggerFactory">Optional logger factory for creating loggers.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> is null.</exception>
        public FullRestore(
            string databaseName,
            string filePath,
            Action<Restore> configureRestore,
            ILoggerFactory loggerFactory = null)
            : base(loggerFactory?.CreateLogger<FullRestore>(), databaseName, configureRestore)
        {
            Initialize(filePath, databaseName);
        }

        /// <summary>
        /// Initializes the restore operation by validating the file path and logging the setup.
        /// </summary>
        /// <param name="filePath">The file path of the backup to restore from.</param>
        /// <param name="databaseName">The name of the database to restore.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> is null.</exception>
        protected override void Initialize(string filePath, string databaseName)
        {
            ValidateFilePath(filePath);
            _filePath = filePath;
            if (!IsFileExists(filePath))
                throw new BackupRestoreException(new FileNotFoundException("The backup file does not exist.", filePath));
            _logger?.LogDebug("Initializing full restore for database {databaseName} with file path {filePath}", databaseName, filePath);
        }

        /// <summary>
        /// Configures the restore device for the operation.
        /// </summary>
        /// <returns>A <see cref="BackupDeviceItem"/> representing the file device for the restore operation.</returns>
        protected override BackupDeviceItem SetDevice()
        {
            return new BackupDeviceItem(_filePath, DeviceType.File);
        }
    }

}
