using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Utils
{
    public class BackupMetadata
    {
        public string DatabaseName { get; set; }
        public string BackupType { get; set; }
        public string BackupFilePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public string BackupDescription { get; set; }
    }
}
