using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using MSSQL.BackupRestore.Exceptions;
using MSSQL.BackupRestore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Configurations
{
    /// <summary>
    /// Factory class for creating and managing SQL Server connections.
    /// </summary>
    public static class ServerFactory
    {
        /// <summary>
        /// Default timeout for SQL statements in seconds.
        /// This can be overridden by setting the environment variable "STATEMENT_TIMEOUT".
        /// https://docs.microsoft.com/en-us/sql/t-sql/statements/set-statements-azure-sql-database?view=sql-server-ver15#set-statement-timeout
        /// </summary>
        private const int STATEMENT_TIMEOUT = 600;

        /// <summary>
        /// Creates a new SQL Server instance and configures the connection settings.
        /// </summary>
        /// <param name="serverName">The name of the SQL Server.</param>
        /// <param name="serverId">The user ID to log into the server.</param>
        /// <param name="serverPassword">The password for the user ID.</param>
        /// <param name="statementTimeout">The timeout for SQL statements in seconds. Defaults to STATEMENT_TIMEOUT.</param>
        /// <param name="serverMessageHandler">An optional handler for server messages.</param>
        /// <param name="loggerFactory">An optional logger factory for logging server events.</param>
        /// <returns>A configured <see cref="Server"/> object.</returns>
        /// <exception cref="ArgumentException">Thrown when any of the required parameters are null or invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the connection context could not be initialized.</exception>
        public static Server CreateServer(
            string serverName,
            string serverId,
            string serverPassword,
            int? statementTimeout = null,
            Action<ServerMessageEventArgs> serverMessageHandler = null,
            ILogger logger = null)
        {
            if (!ValidateServerName(serverName))
                throw new ArgumentException("Server name cannot be null or empty.", nameof(serverName));
            if (!ValidateServerId(serverId))
                throw new ArgumentException("Server ID cannot be null or empty.", nameof(serverId));
            if (!ValidateServerPassword(serverPassword))
                throw new ArgumentException("Server password cannot be null or empty.", nameof(serverPassword));

            statementTimeout = statementTimeout ?? STATEMENT_TIMEOUT;

            logger = logger ?? NullLogger.Instance;

            var server = new Server(serverName);
            server.ConnectionContext.LoginSecure = false;
            server.ConnectionContext.Login = serverId;
            server.ConnectionContext.Password = serverPassword;
            server.ConnectionContext.StatementTimeout = statementTimeout.Value;
            server.ConnectionContext.ServerMessage += (sender, e) =>
            {
                serverMessageHandler?.Invoke(e);
                logger.LogInformation("Server message: {Message}, Severity: {State}", e.Error.Message, e.Error.State);
            };

            logger.LogDebug("Server {serverName} created.", serverName);

            return new ServerWrapper(server).GetServer();
        }

        /// <summary>
        /// Validates the server name by ensuring it is not null, empty, or contains invalid characters.
        /// </summary>
        /// <param name="serverName">The server name to validate.</param>
        /// <returns>True if the server name is valid; otherwise, false.</returns>
        private static bool ValidateServerName(string serverName)
        {
            // Disable the warning.
#pragma warning disable SYSLIB1045
            return !(string.IsNullOrWhiteSpace(serverName) || !new Regex(@"^[a-zA-Z0-9._-]+$").IsMatch(serverName));
#pragma warning restore SYSLIB1045
        }

        /// <summary>
        /// Validates the server ID by ensuring it is not null or empty.
        /// </summary>
        /// <param name="serverId">The server ID to validate.</param>
        /// <returns>True if the server ID is valid; otherwise, false.</returns>
        private static bool ValidateServerId(string serverId)
        {
            return !string.IsNullOrWhiteSpace(serverId);
        }

        /// <summary>
        /// Validates the server password by ensuring it is not null or empty.
        /// </summary>
        /// <param name="serverPassword">The server password to validate.</param>
        /// <returns>True if the server password is valid; otherwise, false.</returns>
        private static bool ValidateServerPassword(string serverPassword)
        {
            return !string.IsNullOrWhiteSpace(serverPassword);
        }

        /// <summary>
        /// Checks if a database exists on the server.
        /// </summary>
        /// <param name="server">The SQL Server instance to check.</param>
        /// <param name="databaseName">The name of the database to check for.</param>
        /// <returns>True if the database exists; otherwise, false.</returns>
        internal static bool IsDatabase(this Server server, string databaseName)
        {
            return server?.Databases.Contains(databaseName) ?? false;
        }

        /// <summary>
        /// Attempts to retrieve a database from the server.
        /// </summary>
        /// <param name="server">The SQL Server instance to retrieve the database from.</param>
        /// <param name="databaseName">The name of the database to retrieve.</param>
        /// <param name="database">The retrieved database object, if found.</param>
        /// <returns>True if the database was found and retrieved; otherwise, false.</returns>
        internal static bool TryGetDatabase(this Server server, string databaseName, out Database database)
        {
            database = default;

            if (server == null)
                throw new DatabaseConfigurationException(new ArgumentNullException(nameof(server)), "The server is null.");
            if (string.IsNullOrEmpty(databaseName))
                throw new DatabaseConfigurationException(new ArgumentNullException(nameof(databaseName)), "The database name is null or empty.");

            database = server.Databases[databaseName];
            return database != null;
        }
    }
}
