using Microsoft.SqlServer.Management.Common;
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
    /// Provides a wrapper around the SQL Server <see cref="Server"/> object to simplify database operations
    /// and improve testability through the <see cref="IServer"/> interface.
    /// </summary>
    public class ServerWrapper : IServer
    {
        /// <summary>
        /// The internal SQL Server instance being wrapped.
        /// </summary>
        private readonly Server _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerWrapper"/> class.
        /// </summary>
        /// <param name="server">The SQL Server instance to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="server"/> is null.</exception>
        public ServerWrapper(Server server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        /// <summary>
        /// Checks if a database with the specified name exists on the SQL Server instance.
        /// </summary>
        /// <param name="databaseName">The name of the database to check.</param>
        /// <returns><c>true</c> if the database exists; otherwise, <c>false</c>.</returns>
        public bool ContainsDatabase(string databaseName)
        {
            return _server.Databases.Contains(databaseName);
        }

        /// <summary>
        /// Retrieves the database with the specified name from the SQL Server instance.
        /// </summary>
        /// <param name="databaseName">The name of the database to retrieve.</param>
        /// <returns>The <see cref="Database"/> object if found; otherwise, <c>null</c>.</returns>
        public Database GetDatabase(string databaseName)
        {
            return _server.Databases[databaseName];
        }

        /// <summary>
        /// Registers a handler for SQL Server messages generated during operations.
        /// </summary>
        /// <param name="handler">The action to execute when a server message is received.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="handler"/> is null.</exception>
        public void AddServerMessageHandler(Action<ServerMessageEventArgs> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _server.ConnectionContext.ServerMessage += (sender, e) => handler(e);
        }
    }

}
