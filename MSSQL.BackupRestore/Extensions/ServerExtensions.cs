using Microsoft.SqlServer.Management.Smo;
using MSSQL.BackupRestore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Extensions
{
    public static class ServerExtensions
    {
        /// <summary>
        /// Checks if a database exists on the server.
        /// </summary>
        /// <param name="server">The <see cref="IServer"/> implementation.</param>
        /// <param name="databaseName">The name of the database to check for.</param>
        /// <returns>True if the database exists; otherwise, false.</returns>
        public static bool IsDatabase(this IServer server, string databaseName)
        {
            if (server == null)
                throw new ArgumentNullException(nameof(server));
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name cannot be null or empty.", nameof(databaseName));

            return server.ContainsDatabase(databaseName);
        }

        /// <summary>
        /// Attempts to retrieve a database from the server.
        /// </summary>
        /// <param name="server">The <see cref="IServer"/> implementation.</param>
        /// <param name="databaseName">The name of the database to retrieve.</param>
        /// <param name="database">The retrieved database object, if found.</param>
        /// <returns>True if the database was found and retrieved; otherwise, false.</returns>
        public static bool TryGetDatabase(this IServer server, string databaseName, out Database database)
        {
            database = default;

            if (server == null)
                throw new ArgumentNullException(nameof(server));
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name cannot be null or empty.", nameof(databaseName));

            database = server.GetDatabase(databaseName);
            return database != null;
        }
    }
}
