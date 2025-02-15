﻿using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MSSQL.BackupRestore.Interfaces
{
    public interface IBackupRestore
    {
        string DatabaseName { get; }

        event ServerMessageEventHandler Complete;
        event ServerMessageEventHandler Information;
        event PercentCompleteEventHandler PercentComplete;

        Task ExecuteAsync(Server server, CancellationToken ct = default);
    }
}
