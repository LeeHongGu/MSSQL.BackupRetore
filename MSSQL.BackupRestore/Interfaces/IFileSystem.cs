using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Interfaces
{
    internal interface IFileSystem
    {
        void ValidateFilePath(string filePath);
    }
}
