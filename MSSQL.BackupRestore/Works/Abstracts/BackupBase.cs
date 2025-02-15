﻿using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using MSSQL.BackupRestore.Configurations;
using MSSQL.BackupRestore.Exceptions;
using MSSQL.BackupRestore.Extensions;
using MSSQL.BackupRestore.Interfaces;
using MSSQL.BackupRestore.Utils;
using MSSQL.BackupRestore.Works.Abstracts;
using MSSQL.BackupRestore.Works.BackupWorks;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Works.Abstracts
{
    /// <summary>
    /// Base class for managing SQL Server backup operations.
    /// Provides functionality for configuring backup devices, executing backups, 
    /// and handling events related to backup progress and completion.
    /// </summary>
    public abstract class BackupBase : IBackupRestore, IFileSystem, IWorkSet
    {
        /// <summary>
        /// Logger instance for recording log messages related to backup operations.
        /// </summary>
        protected readonly ILogger _logger;

        /// <summary>
        /// Represents the SQL Server backup operation being managed.
        /// </summary>
        private readonly Backup _backup;

        /// <summary>
        /// The file path where the backup will be stored.
        /// </summary>
        protected string _filePath;

        /// <summary>
        /// Event triggered when the backup operation is complete.
        /// </summary>
        public event ServerMessageEventHandler Complete
        {
            add => _backup.Complete += value;
            remove => _backup.Complete -= value;
        }

        /// <summary>
        /// Event triggered when an informational message is received during the backup operation.
        /// </summary>
        public event ServerMessageEventHandler Information
        {
            add => _backup.Information += value;
            remove => _backup.Information -= value;
        }

        /// <summary>
        /// Event triggered to report the progress of the backup operation in percentage.
        /// </summary>
        public event PercentCompleteEventHandler PercentComplete
        {
            add => _backup.PercentComplete += value;
            remove => _backup.PercentComplete -= value;
        }

        /// <summary>
        /// Gets or sets the name of the database being backed up.
        /// </summary>
        public string DatabaseName
        {
            get => _backup.Database;
            private set => _backup.Database = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupBase"/> class.
        /// </summary>
        /// <param name="logger">Logger instance for recording log messages.</param>
        /// <param name="databaseName">The name of the database to back up.</param>
        /// <param name="backup">The <see cref="Backup"/> object representing the backup operation.</param>
        /// <param name="optionDelegate">Optional delegate for configuring the backup object.</param>
        protected BackupBase(ILogger logger, string databaseName, Backup backup, Action<Backup> optionDelegate = null)
        {
            _logger = logger;
            _backup = backup ?? throw new ArgumentNullException(nameof(backup), "Backup object cannot be null.");
            DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName), "Database name cannot be null.");

            optionDelegate?.Invoke(_backup);
        }

        protected BackupBase(ILogger logger, string databaseName, Action<Backup> optionDelegate = null)
            : this(logger, databaseName, new Backup(), optionDelegate)
        {    
        }

        /// <summary>
        /// Adds a backup device by specifying the device name and type.
        /// </summary>
        /// <param name="deviceName">The name of the backup device.</param>
        /// <param name="deviceType">The type of the backup device.</param>
        public void AddDevice(string deviceName, DeviceType deviceType) => AddDevice(new BackupDeviceItem(deviceName, deviceType));

        /// <summary>
        /// Adds a backup device represented by a <see cref="BackupDeviceItem"/>.
        /// </summary>
        /// <param name="device">The backup device to add.</param>
        public void AddDevice(BackupDeviceItem device)
        {
            if (_backup?.Devices == null)
            {
                _logger.LogError("Backup devices collection is not initialized.");
                throw new InvalidOperationException("Backup devices collection is not initialized.");
            }

            var existingDevice = _backup.Devices.Find(x => x.DeviceType == device.DeviceType);

            if (!(existingDevice is null))
            {
                _backup.Devices.Remove(existingDevice);
            }

            if (existingDevice?.Name != device.Name)
            {
                _backup.Devices.AddDevice(device.Name, device.DeviceType);
                _logger.LogInformation("Backup device added: {DeviceName}", device.Name);
            }
            else
            {
                _logger.LogInformation("Backup device {DeviceName} already configured.", device.Name);
            }
        }

        /// <summary>
        /// Abstract method for configuring a backup device.
        /// Must be implemented by derived classes to specify the backup device to use.
        /// </summary>
        /// <returns>A <see cref="BackupDeviceItem"/> representing the backup device.</returns>
        protected abstract BackupDeviceItem SetDevice();


        /// <summary>
        /// Sets the default backup device for the operation.
        /// </summary>
        /// <returns>A <see cref="BackupDeviceItem"/> representing the backup device.</returns>
        public BackupDeviceItem CreateDefaultDevice() => new BackupDeviceItem(_filePath, DeviceType.File);

        /// <summary>
        /// Validates the file path for the backup operation.
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void CheckNullFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "File path cannot be null.");
        }

        /// <summary>
        /// Initializes the backup operation by setting the file path and database name.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="databaseName"></param>
        protected abstract void Initialize(string filePath, string databaseName);

        /// <summary>
        /// Generates a metadata file containing backup details.
        /// </summary>
        protected void GenerateBackupMetadata()
        {
            // BackupActionType + Incremental을 기반으로 BackupType을 결정
            var backupType = BackupTypeExtensions.DetermineBackupType(_backup);

            var metadata = new BackupMetadata
            {
                DatabaseName = DatabaseName,
                BackupType = backupType.ToString(),
                BackupFilePath = _filePath,
                CreatedAt = DateTime.Now,
                BackupDescription = $"{backupType} backup for {DatabaseName} created on {DateTime.Now}."
            };

            var metadataFilePath = Path.ChangeExtension(_filePath, ".meta.json");

            try
            {
                var json = JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(metadataFilePath, json);
                _logger?.LogInformation("Backup metadata created at {MetadataFilePath}", metadataFilePath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create backup metadata file for {DatabaseName}.", DatabaseName);
                throw new IOException("Failed to create metadata file.", ex);
            }
        }

        /// <summary>
        /// Executes the backup operation asynchronously.
        /// </summary>
        /// <param name="server">The SQL Server instance where the backup will be performed.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous backup operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the server is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the database name is not set or a backup is already in progress.</exception>
        /// <exception cref="BackupRestoreException">Thrown if the backup configuration is incomplete or invalid.</exception>
        public async Task ExecuteAsync(Server server, CancellationToken ct = default)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server), "The server parameter does not allow a null value.");

            if (string.IsNullOrWhiteSpace(DatabaseName))
                throw new InvalidOperationException("The database name is not set.");

            if (_backup.AsyncStatus.ExecutionStatus == ExecutionStatus.InProgress)
                throw new InvalidOperationException("The backup operation is already in progress.");

            if (!server.IsDatabase(DatabaseName))
                throw new BackupRestoreException(new Exception($"The database {DatabaseName} already exists."));

            AddDevice(SetDevice());

            if (_backup.Devices.Count == 0)
                throw new BackupRestoreException(new Exception("The backup proceeded without completing the backup devices configuration."));

            _logger?.LogInformation("Backup started for database {DatabaseName}", DatabaseName);

            _backup.SqlBackupAsync(server);

            await Task.Run(() => _backup.Wait(), ct);

            _logger?.LogInformation("Backup completed for database {DatabaseName}", DatabaseName);

            GenerateBackupMetadata();
        }
    }

}
