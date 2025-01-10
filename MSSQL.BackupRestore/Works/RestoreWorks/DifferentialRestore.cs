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
    /// Represents a differential restore operation for an MSSQL database.
    /// This class provides functionality to configure and execute differential database restores.
    /// </summary>
    public class DifferentialRestore : RestoreBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentialRestore"/> class with default restore settings.
        /// </summary>
        /// <param name="databaseName">The name of the database to restore.</param>
        /// <param name="filePath">The file path of the differential backup to restore from.</param>
        /// <param name="noRecovery">Indicates whether the database should remain in a restoring state after the restore operation. Defaults to <c>true</c>.</param>
        /// <param name="loggerFactory">Optional logger factory for creating loggers.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="BackupRestoreException">Thrown if the specified file path does not exist.</exception>
        public DifferentialRestore(
            string databaseName,
            string filePath,
            bool noRecovery = true,
            ILoggerFactory loggerFactory = null)
            : base(loggerFactory?.CreateLogger<DifferentialRestore>(), databaseName, (restore) =>
            {
                restore.Action = RestoreActionType.Database;
                restore.ReplaceDatabase = false;
                restore.PercentCompleteNotification = 1;
                restore.ContinueAfterError = true;
                restore.NoRecovery = noRecovery;
            })
        {
            Initialize(filePath, databaseName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DifferentialRestore"/> class with custom restore settings.
        /// </summary>
        /// <param name="databaseName">The name of the database to restore.</param>
        /// <param name="filePath">The file path of the differential backup to restore from.</param>
        /// <param name="configureRestore">A delegate to customize the restore configuration.</param>
        /// <param name="loggerFactory">Optional logger factory for creating loggers.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="BackupRestoreException">Thrown if the specified file path does not exist.</exception>
        public DifferentialRestore(
            string databaseName,
            string filePath,
            Action<Restore> configureRestore,
            ILoggerFactory loggerFactory = null)
            : base(loggerFactory?.CreateLogger<DifferentialRestore>(), databaseName, configureRestore)
        {
            Initialize(filePath, databaseName);
        }

        /// <summary>
        /// Initializes the restore operation by validating the file path and logging the setup.
        /// </summary>
        /// <param name="filePath">The file path of the differential backup to restore from.</param>
        /// <param name="databaseName">The name of the database to restore.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="BackupRestoreException">Thrown if the specified file path does not exist.</exception>
        protected override void Initialize(string filePath, string databaseName)
        {
            ValidateFilePath(filePath);
            _filePath = filePath;
            if (!IsFileExists(filePath))
                throw new BackupRestoreException(new FileNotFoundException("The specified backup file does not exist.", filePath));
            _logger?.LogDebug("Initialized differential restore operation for database {DatabaseName} from file {FilePath}.", databaseName, filePath);
        }

        /// <summary>
        /// Configures the restore device for the differential restore operation.
        /// </summary>
        /// <returns>A <see cref="BackupDeviceItem"/> representing the device for the restore operation.</returns>
        protected override BackupDeviceItem SetDevice() => CreateDefaultDevice();
    }
}
