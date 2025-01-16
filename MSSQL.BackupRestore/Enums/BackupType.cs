using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Enums
{
    public enum BackupType
    {
        Unknown = 0,
        Full = 1,
        Differential = 2,
        TransactionLog = 3
    }
}
