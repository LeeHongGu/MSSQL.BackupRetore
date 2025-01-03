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
    public class ServerWrapper : IServer
    {
        private readonly Server _server;

        public ServerWrapper(Server server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
        }

        public bool ContainsDatabase(string databaseName)
        {
            return _server.Databases.Contains(databaseName);
        }

        public Database GetDatabase(string databaseName)
        {
            return _server.Databases[databaseName];
        }

        public void AddServerMessageHandler(Action<ServerMessageEventArgs> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _server.ConnectionContext.ServerMessage += (sender, e) => handler(e);
        }
    }

}
