using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using MSSQL.BackupRestore.Configurations;
using MSSQL.BackupRestore.Exceptions;
using MSSQL.BackupRestore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Works.Abstracts
{
    /// <summary>
    /// Base class for database restore operations in MSSQL.
    /// Provides common functionality for configuring and executing restore tasks.
    /// </summary>
    public abstract class RestoreBase : IBackupRestore, INoRecoverable
    {
        /// <summary>
        /// Logger instance for recording restore operation events and debugging information.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// MSSQL Restore object used to configure and execute the restore operation.
        /// </summary>
        private readonly Restore _restore;

        /// <summary>
        /// Gets or sets the name of the database to be restored.
        /// </summary>
        public string DatabaseName
        {
            get => _restore.Database;
            private set => _restore.Database = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the database should remain in a restoring state after the restore operation.
        /// </summary>
        public bool NoRecovery
        {
            get => _restore.NoRecovery;
            set => _restore.NoRecovery = value;
        }

        /// <summary>
        /// Event triggered when the restore operation completes.
        /// </summary>
        public event ServerMessageEventHandler Complete
        {
            add => _restore.Complete += value;
            remove => _restore.Complete -= value;
        }

        /// <summary>
        /// Event triggered when informational messages are generated during the restore operation.
        /// </summary>
        public event ServerMessageEventHandler Information
        {
            add => _restore.Information += value;
            remove => _restore.Information -= value;
        }

        /// <summary>
        /// Event triggered to report the progress of the restore operation in percentage.
        /// </summary>
        public event PercentCompleteEventHandler PercentComplete
        {
            add => _restore.PercentComplete += value;
            remove => _restore.PercentComplete -= value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestoreBase"/> class with a custom <see cref="Restore"/> object.
        /// </summary>
        /// <param name="logger">Logger instance for recording events.</param>
        /// <param name="databaseName">The name of the database to restore.</param>
        /// <param name="restore">The <see cref="Restore"/> object used for the restore operation.</param>
        /// <param name="optionDelegate">Optional delegate to configure the restore operation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="databaseName"/> or <paramref name="restore"/> is null.</exception>
        protected RestoreBase(ILogger logger, string databaseName, Restore restore, Action<Restore> optionDelegate = null)
        {
            _logger = logger;
            DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName), "Database name cannot be null.");
            _restore = restore ?? throw new ArgumentNullException(nameof(restore), "Restore object cannot be null.");

            optionDelegate?.Invoke(_restore);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestoreBase"/> class with a default <see cref="Restore"/> object.
        /// </summary>
        /// <param name="logger">Logger instance for recording events.</param>
        /// <param name="databaseName">The name of the database to restore.</param>
        /// <param name="optionDelegate">Optional delegate to configure the restore operation.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="databaseName"/> is null.</exception>
        protected RestoreBase(ILogger logger, string databaseName, Action<Restore> optionDelegate = null)
            : this(logger, databaseName, new Restore(), optionDelegate)
        {
        }

        /// <summary>
        /// Configures the restore device for the operation.
        /// Derived classes must implement this method to specify the restore device.
        /// </summary>
        /// <returns>A <see cref="BackupDeviceItem"/> representing the restore device.</returns>
        protected abstract BackupDeviceItem SetDevice();

        /// <summary>
        /// Adds a restore device to the restore operation.
        /// </summary>
        /// <param name="backupDeviceItem">The backup device item to add.</param>
        public void AddDevice(BackupDeviceItem backupDeviceItem)
        {
            _restore.Devices.AddDevice(backupDeviceItem.Name, backupDeviceItem.DeviceType);
            _logger?.LogDebug("Restore device '{DeviceName}' of type '{DeviceType}' added for database '{DatabaseName}'.", backupDeviceItem.Name, backupDeviceItem.DeviceType, DatabaseName);
        }

        /// <summary>
        /// Adds a restore device to the restore operation by specifying the device name and type.
        /// </summary>
        /// <param name="deviceName">The name of the restore device.</param>
        /// <param name="deviceType">The type of the restore device.</param>
        public void AddDevice(string deviceName, DeviceType deviceType) => AddDevice(new BackupDeviceItem(deviceName, deviceType));

        /// <summary>
        /// Executes the restore operation asynchronously on the specified SQL Server instance.
        /// </summary>
        /// <param name="server">The SQL Server instance where the restore operation will be executed.</param>
        /// <param name="ct">A cancellation token to cancel the operation if needed.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="server"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the restore operation is already in progress or the database name is not set.</exception>
        /// <exception cref="BackupRestoreException">Thrown if the restore devices are not properly configured or the database already exists.</exception>
        public virtual async Task ExecuteAsync(Server server, CancellationToken ct = default)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server), "The server parameter does not allow a null value.");

            if (string.IsNullOrWhiteSpace(DatabaseName))
                throw new InvalidOperationException("The database name is not set.");

            if (!server.IsDatabase(DatabaseName))
                throw new BackupRestoreException(new Exception($"The database {DatabaseName} already exists."));

            if (_restore.AsyncStatus.ExecutionStatus == ExecutionStatus.InProgress)
                throw new InvalidOperationException("The restore operation is already in progress.");

            AddDevice(SetDevice());

            if (_restore.Devices.Count == 0)
                throw new BackupRestoreException(new Exception("The restore proceeded without completing the restore devices configuration."));

            _logger?.LogInformation("Starting restore for database '{DatabaseName}'...", DatabaseName);
            _restore.SqlRestoreAsync(server);

            try
            {
                await Task.Run(() => _restore.Wait(), ct);
            }
            catch (OperationCanceledException)
            {
                _logger?.LogWarning("Restore operation for database '{DatabaseName}' was canceled.", DatabaseName);
                throw;
            }

            _logger?.LogInformation("Restore completed for database '{DatabaseName}'.", DatabaseName);
        }
    }

}
