using MSSQL.BackupRestore.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Works.Abstracts
{
    /// <summary>
    /// Base class for managing file system operations.
    /// </summary>
    public abstract class FileSystem : IFileSystem
    {
        private const string BACKUP_EXTENSION = ".bak";
        private const string LOG_EXTENSION = ".trn";

        /// <summary>
        /// Validates the file path to ensure it is not null or empty.
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void CheckNullFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "File path cannot be null.");
        }

        /// <summary>
        /// Checks if the file exists at the specified path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected virtual bool IsFileExists(string filePath) => System.IO.File.Exists(filePath);


        protected virtual bool ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "File path cannot be null.");
            if (!IsFileExists(filePath))
                throw new System.IO.FileNotFoundException("File not found.", filePath);
            if (!IsBackupFile(filePath))
                throw new ArgumentException("Invalid file type. Only backup files are supported.", nameof(filePath));
            return true;
        }

        private static bool IsBackupFile(string filePath) => Path.GetExtension(filePath).Equals(BACKUP_EXTENSION, StringComparison.OrdinalIgnoreCase) || Path.GetExtension(filePath).Equals(LOG_EXTENSION, StringComparison.OrdinalIgnoreCase);
    }
}
