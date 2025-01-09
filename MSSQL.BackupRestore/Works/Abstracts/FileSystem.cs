using MSSQL.BackupRestore.Interfaces;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Validates the file path to ensure it is not null or empty.
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void ValidateFilePath(string filePath)
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
    }
}
