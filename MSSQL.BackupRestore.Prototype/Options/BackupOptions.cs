using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Prototype.Options
{
    // appsettings.json의 Backup 섹션과 매핑
    public class BackupOptions
    {
        public string FullBackupPath { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
    }
}
