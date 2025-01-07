using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Enums
{
    [Flags]
    public enum RecoveryOptions
    {
        None = 0,
        FullRestore = 1,
        DifferentialRestore = 2,
        TransactionLogRestore = 4
    }
}
