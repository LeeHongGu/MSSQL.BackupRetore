using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Interfaces
{
    public interface IServer
    {
        /// <summary>
        /// Checks if a database exists on the server.
        /// </summary>
        /// <param name="databaseName">The name of the database to check for.</param>
        /// <returns>True if the database exists; otherwise, false.</returns>
        bool ContainsDatabase(string databaseName);

        /// <summary>
        /// Retrieves a database from the server by name.
        /// </summary>
        /// <param name="databaseName">The name of the database to retrieve.</param>
        /// <returns>The database object, if found.</returns>
        Database GetDatabase(string databaseName);

        /// <summary>
        /// Adds a handler for server messages.
        /// </summary>
        void AddServerMessageHandler(Action<ServerMessageEventArgs> handler);
    }
}
