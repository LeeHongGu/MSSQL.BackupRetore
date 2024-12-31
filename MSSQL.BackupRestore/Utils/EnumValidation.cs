using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Utils
{
    public static class EnumValidation
    {
        public static bool IsValid<T>(T value) where T : struct
        {
            return Enum.IsDefined(typeof(T), value);
        }
    }
}
