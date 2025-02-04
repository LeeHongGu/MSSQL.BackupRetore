using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Prototype.Interfaces
{
    internal interface IBackupJob
    {
        Task ExecuteAsync();
    }
}
