using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Prototype.Options
{
    // appsettings.json의 SqlServer 섹션과 매핑
    public class SqlServerOptions
    {
        public string Address { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
