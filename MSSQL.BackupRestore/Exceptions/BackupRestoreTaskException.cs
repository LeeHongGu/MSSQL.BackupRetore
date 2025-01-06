using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Exceptions
{
    public class BackupRestoreTaskException : Exception
    {
        public BackupRestoreTaskException() : base() { }
        public BackupRestoreTaskException(string message) : base(message) { }
        public BackupRestoreTaskException(string message, Exception innerException) : base(message, innerException) { }
        public BackupRestoreTaskException(Exception innerException) : base(innerException.Message, innerException) { }
        public BackupRestoreTaskException(Exception innerException, string message) : base(message, innerException) { }
        public BackupRestoreTaskException(string param, string message) : base($"{param}: {message}")
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new ArgumentException("Parameter name cannot be null or empty.", nameof(param));
            }
        }
        public BackupRestoreTaskException(string param, string message, Exception innerException) : base($"{param}: {message}", innerException)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new ArgumentException("Parameter name cannot be null or empty.", nameof(param));
            }
        }
    }
}
