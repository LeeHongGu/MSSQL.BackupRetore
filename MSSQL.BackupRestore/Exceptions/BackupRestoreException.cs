using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Exceptions
{
    public class BackupRestoreException : Exception
    {
        // 기본 생성자
        public BackupRestoreException() : base()
        {
        }

        // 메시지 전달 생성자
        public BackupRestoreException(string message) : base(message)
        {
        }

        // 내부 예외만 전달하는 생성자
        public BackupRestoreException(Exception innerException) 
            : base(innerException?.Message, innerException)
        {
        }

        // 메시지와 내부 예외를 전달하는 생성자
        public BackupRestoreException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

        // 특정 파라미터와 메시지를 전달하는 생성자
        public BackupRestoreException(string param, string message) 
            : base($"{param}: {message}")
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new ArgumentException("Parameter name cannot be null or empty.", nameof(param));
            }
        }

        // 특정 파라미터, 메시지, 내부 예외를 전달하는 생성자
        public BackupRestoreException(string param, string message, Exception innerException) 
            : base($"{param}: {message}", innerException)
        {
            if (string.IsNullOrWhiteSpace(param))
            {
                throw new ArgumentException("Parameter name cannot be null or empty.", nameof(param));
            }
        }
    }        
}
