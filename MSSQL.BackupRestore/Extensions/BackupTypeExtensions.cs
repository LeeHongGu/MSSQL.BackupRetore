using Microsoft.SqlServer.Management.Smo;
using MSSQL.BackupRestore.Enums;
using MSSQL.BackupRestore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Extensions
{
    /// <summary>
    /// Provides extension methods for converting and identifying SQL Server backup types.
    /// This class simplifies the handling of backup types across various operations such as metadata reading,
    /// file naming conventions, and SQL Server backup metadata analysis.
    /// </summary>
    public static class BackupTypeExtensions
    {
        /// <summary>
        /// Converts a <see cref="BackupType"/> enum to its string representation.
        /// </summary>
        /// <param name="backupType">The backup type to convert.</param>
        /// <returns>A string representing the backup type.</returns>
        public static string ToBackupTypeString(this BackupType backupType)
        {
            switch (backupType)
            {
                case BackupType.Full:
                    return "Full";
                case BackupType.Differential:
                    return "Differential";
                case BackupType.TransactionLog:
                    return "Transaction Log";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Converts a string representation of a backup type to its corresponding <see cref="BackupType"/> enum.
        /// </summary>
        /// <param name="backupTypeString">The string representation of the backup type.</param>
        /// <returns>The corresponding <see cref="BackupType"/>.</returns>
        public static BackupType ToBackupType(this string backupTypeString)
        {
            switch (backupTypeString.ToLower())
            {
                case "full":
                    return BackupType.Full;
                case "differential":
                    return BackupType.Differential;
                case "transaction log":
                    return BackupType.TransactionLog;
                default:
                    return BackupType.Unknown;
            }
        }

        /// <summary>
        /// Maps the SQL Server backup type code to the corresponding <see cref="BackupType"/> enum.
        /// </summary>
        /// <param name="backupTypeCode">The numeric backup type code from SQL Server metadata.</param>
        /// <returns>The corresponding <see cref="BackupType"/>.</returns>
        public static BackupType GetBackupType(int backupTypeCode)
        {
            switch (backupTypeCode)
            {
                case 1:
                    return BackupType.Full;
                case 2:
                    return BackupType.Differential;
                case 3:
                    return BackupType.TransactionLog;
                default:
                    return BackupType.Unknown;
            }
        }

        /// <summary>
        /// Converts a <see cref="BackupType"/> to its corresponding SQL Server backup type code.
        /// </summary>
        /// <param name="backupType">The backup type to convert.</param>
        /// <returns>The numeric backup type code.</returns>
        public static int GetBackupTypeCode(this BackupType backupType)
        {
            return (int)backupType;
        }

        /// <summary>
        /// Determines the backup type based on file name conventions.
        /// </summary>
        /// <param name="filePath">The backup file path.</param>
        /// <returns>The identified <see cref="BackupType"/>.</returns>
        public static BackupType GetBackupTypeByFileName(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath).ToLower();

            if (fileName.Contains("full")) return BackupType.Full;
            if (fileName.Contains("diff")) return BackupType.Differential;
            if (fileName.Contains("log")) return BackupType.TransactionLog;

            return BackupType.Unknown;
        }

        /// <summary>
        /// Determines the backup type by analyzing the SQL Server backup header metadata.
        /// </summary>
        /// <param name="filePath">The backup file path.</param>
        /// <param name="server">The SQL Server instance.</param>
        /// <returns>The identified <see cref="BackupType"/>.</returns>
        public static BackupType GetBackupTypeByFileName(string filePath, Server server)
        {
            var sql = $"RESTORE HEADERONLY FROM DISK = N'{filePath}'";
            var dataTable = server.ConnectionContext.ExecuteWithResults(sql).Tables[0];
            int backupTypeCode = Convert.ToInt32(dataTable.Rows[0]["BackupType"]);
            return GetBackupType(backupTypeCode);
        }

        /// <summary>
        /// Determines the backup type by reading the associated metadata file (.meta.json).
        /// </summary>
        /// <param name="filePath">The backup file path.</param>
        /// <returns>The identified <see cref="BackupType"/>.</returns>
        public static BackupType GetBackupTypeFromMetadata(string filePath)
        {
            var metaFilePath = Path.ChangeExtension(filePath, ".meta.json");

            if (!File.Exists(metaFilePath))
                return BackupType.Unknown;

            var jsonContent = File.ReadAllText(metaFilePath);
            var metadata = JsonSerializer.Deserialize<BackupMetadata>(jsonContent);

            return Enum.TryParse(metadata.BackupType, out BackupType type) ? type : BackupType.Unknown;
        }

        /// <summary>
        /// Determines the backup type using a multi-step strategy: SQL metadata, filename, and metadata file.
        /// </summary>
        /// <param name="filePath">The backup file path.</param>
        /// <param name="server">The SQL Server instance.</param>
        /// <returns>The identified <see cref="BackupType"/>.</returns>
        public static BackupType DetermineBackupType(string filePath, Server server)
        {
            // 1차: SQL 메타데이터로 확인
            var type = GetBackupTypeByFileName(filePath, server);
            if (type != BackupType.Unknown) return type;

            // 2차: 파일명 규칙으로 확인
            type = GetBackupTypeByFileName(filePath);
            if (type != BackupType.Unknown) return type;

            // 3차: 메타데이터 파일로 확인
            return GetBackupTypeFromMetadata(filePath);
        }

        /// <summary>
        /// Determines the backup type based on the <see cref="BackupActionType"/> and <see cref="Backup.Incremental"/> flag.
        /// </summary>
        /// <param name="backup">The backup operation object.</param>
        /// <returns>The corresponding <see cref="BackupType"/>.</returns>
        public static BackupType DetermineBackupType(Backup backup)
        {
            switch (backup.Action)
            {
                case BackupActionType.Database when backup.Incremental == false:
                    return BackupType.Full;
                case BackupActionType.Database when backup.Incremental == true:
                    return BackupType.Differential;
                case BackupActionType.Log:
                    return BackupType.TransactionLog;
                default:
                    return BackupType.Unknown;
            }
        }
    }
}
