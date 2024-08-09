using System;
using System.Data;

namespace Collette.Data
{
    public interface IDbConnectionWrapper : IDisposable
    {
        IDbConnection GetConnection(IDbTransaction? transaction = null, IDbConnection? connection = null);
        IDbConnection CreateConnection(bool autoOpen = false);
    }
}
