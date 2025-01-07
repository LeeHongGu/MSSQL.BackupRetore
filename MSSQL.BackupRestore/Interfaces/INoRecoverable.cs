using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Interfaces
{
    public interface INoRecoverable
    {
        bool NoRecovery { get; set; }
    }
}
