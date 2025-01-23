using Microsoft.SqlServer.Management.Smo;
using MSSQL.BackupRestore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Configurations
{
    /// <summary>
    /// Wrapper class for the Microsoft.SqlServer.Management.Smo.Database object.
    /// Provides a simplified interface for interacting with a SQL Server database.
    /// </summary>
    public class DatabaseWrapper : IDatabase
    {
        /// <summary>
        /// The underlying SMO Database object being wrapped.
        /// </summary>
        private readonly Database _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseWrapper"/> class.
        /// </summary>
        /// <param name="database">The SMO Database object to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when the database parameter is null.</exception>
        public DatabaseWrapper(Database database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public string Name => _database.Name;

        /// <summary>
        /// Creates the database on the SQL Server.
        /// </summary>
        public void Create()
        {
            _database.Create();
        }

        /// <summary>
        /// Drops the database from the SQL Server.
        /// </summary>
        public void Drop()
        {
            _database.Drop();
        }

        /// <summary>
        /// Checks if the database exists on the SQL Server.
        /// </summary>
        /// <returns>True if the database exists, otherwise false.</returns>
        public bool Exists()
        {
            return _database.Parent.Databases.Contains(_database.Name);
        }

        /// <summary>
        /// Retrieves the underlying SMO Database object.
        /// </summary>
        /// <returns>The SMO Database object.</returns>
        public Database GetDatabase()
        {
            return _database;
        }
    }

}
