using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Exceptions
{
    public class DatabaseConfigurationException : Exception
    {
        public DatabaseConfigurationException() : base() { }

        public DatabaseConfigurationException(string message) : base(message) { }

        public DatabaseConfigurationException(Exception innerException) : base(innerException?.Message, innerException) { }

        public DatabaseConfigurationException(string message, Exception innerException) : base(message, innerException) { }
        
        public DatabaseConfigurationException(Exception innerException, string message) : base(message, innerException) { }

        public DatabaseConfigurationException(string param, string message) : base($"{param}: {message}")
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new ArgumentException("Parameter name cannot be null or empty.", nameof(param));
            }
        }


        public DatabaseConfigurationException(string param, string message, Exception innerException) : base($"{param}: {message}", innerException)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new ArgumentException("Parameter name cannot be null or empty.", nameof(param));
            }
        }
    }
}
