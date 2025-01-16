using MSSQL.BackupRestore.Works.RestoreWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Utils
{
    public class RestoreComparer : IComparer<Type>
    {
        private readonly List<Type> _restoreOrder = new List<Type>
        {
            typeof(FullRestore),
            typeof(DifferentialRestore),
            typeof(TransactionLogRestore)
        };

        public int Compare(Type x, Type y)
        {
            if (x is null || y is null)
                throw new ArgumentNullException("Types cannot be null.");
            return _restoreOrder.FindIndex(t => t == x).CompareTo(_restoreOrder.FindIndex(t => t == y));
        }
    }
}
